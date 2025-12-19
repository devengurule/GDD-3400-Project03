using UnityEngine;                  // Unity core engine APIs (MonoBehaviour, GameObject, Vector types, Time, Camera)
using System.Collections;           // Provides IEnumerator so we can write coroutines (yield return ...)
using System.Collections.Generic;   // for Dictionary, List, etc.
using UnityEngine.Tilemaps;
using System;
using static UnityEngine.GraphicsBuffer;
using Unity.Burst.Intrinsics;

public class BalanceTest : AbstractTest
{
    #region Fields
    //Override test identifier
    public override string TestType => "Balance";
    protected override string LogFileName => "Balance Test Details.csv";


    //  Seek configuration 
    private float retargetInterval = 0.35f; // How often we refresh the nearest enemy

    // Per-frame state
    private GameObject closestReachableEnemy;                      // The nearest Enemy
    private GameObject closestUnreachableEnemy;           //stores the closest enemy that is attackable
    private GameObject closestItem;                       // The nearest Pickup

    // Decision Making Priorities (Higher value = more likely when considered)
    private float playerMeleeRange = .35f; // melee range (used to set meleeRange)
    private int chasePriority = 10;
    private int pickupPriority = 10;
    private int projectilePriority = 10;
    private int healPriority = 3;

    // Inventory / health
    private Inventory inventory;
    private HealthScript healthScript;

    // Effective melee range used by logic (wired from playerMeleeRange)
    private float meleeRange = 1f;

    // Result variables
    private int healthStart = 0;
    private int health = 0;
    private int healthMax = 0;
    private int damageTaken = 0;
    private bool isDead = false;
    private int healsUsed = 0;
    private int projectilesUsed = 0;
    private int meleeAttacks = 0;

    // ==== Decision commitment ====
    [Header("Decision Commit Settings")]
    private float decisionMinCommit = 0.01f;     // Minimum time to stick with a decision
    private float decisionMaxCommit = 2.00f;     // Maximum time to stick with a decision if goal not reached
    private float emergencyHealThreshold = 0.5f; // Interrupt to heal if HP% <= this
    private float pickupReachDistance = 0.6f;     // Distance that counts as "reached pickup"
    private float rangedFireInterval = 0.35f;     // Cadence for ranged fire while committed

    // --- Retargeting ---
    [Header("Retargeting")]
    [SerializeField] private float retargetCloserRatio = 0.20f; // 20% closer than current target
    [SerializeField] private float retargetCloserMin = 0.35f; // or at least 0.35m closer (absolute)
    private float lastEnemyRetargetTime = -10f;
    private float lastPickupRetargetTime = -10f;

    //If unable to reach enemies, have number of possible failures
    private int noViableDecisionMax = 100;
    private int noViableDecisionCounter = 0;


    [SerializeField] private PlayerDecision currentDecision = PlayerDecision.Null;
    private float decisionLockUntil = 0f;
    private float decisionStartedAt = 0f;
    private GameObject decisionEnemyTarget = null;   // Snapshot of target at decision start
    private GameObject decisionPickupTarget = null;  // Snapshot of pickup at decision start
    private float lastRangedFireTime = -999f;

    private Coroutine testLoop;
    private bool testComplete = false;

    // --- Pathfinding (runtime) ---
    private Pathfinder _pathfinder;
    private NodeGraph _graph;
    private GameObject aStar;
    private GameObject aStarPrefab;
    public GameObject AStarPrefab { get => aStarPrefab; set => aStarPrefab = value; }
    private TestRoomManager testManager; //holds data for testing in individual rooms

    //Weaving so not walking directly into bullets
    private float weaveAmplitude = 0.4f; // max lateral offset
    private float weaveFrequency = 2f;   // oscillations per second
    private float weaveTime = 0f;        // keeps track of time for sine wave

    // LoS & path-follow state
    private readonly List<Vector2> path = new List<Vector2>();
    private int _wpIndex = 0;
    private float _lastPathBuild = -999f;

    // Tunables (no inspector needed; tweak here)
    private const float _pathRecomputeInterval = 0.5f;
    private const float _waypointEpsilonWorld = 0.06f;
    private const bool _drawPathGizmos = true;

    private const float _pathStopDistance = 0.5f; // end A* when inside this range

    //Player inventory initialization
    List<(Item, float)> itemList = new List<(Item, float)>();

    //stuck detection
    private Vector2 lastPlayerPos;
    private float stuckCheckTimer = 0f;
    private float stuckCheckInterval = 0.3f;  // check every 0.3s
    private float stuckThreshold = 0.05f;     // moved less than this = stuck

    // unreachable detection
    private Dictionary<int, int> failedPathAttemptsDict = new Dictionary<int, int>();
    private int maxFailedPathAttempts = 20;

    // how long to keep a target blacklisted (seconds)
    private float unreachableBlacklistSeconds = 1f;
    private Dictionary<int, float> blacklistExpirationDict = new Dictionary<int, float>();

    // Current wander state
    private Vector2 currentWanderDirection = Vector2.zero;
    private float wanderDirectionLockUntil = 0f;   // timestamp until we keep this direction
    private float wanderDirectionDuration = 2f;  // seconds to keep wandering in one direction

    //possible movement directions
    Vector2[] directions = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
        new Vector2(1, 1).normalized,
        new Vector2(1, -1).normalized,
        new Vector2(-1, 1).normalized,
        new Vector2(-1, -1).normalized
    };

    private enum PlayerDecision
    {
        MeleeAttack,
        RangedAttack,
        ChaseTarget,
        ChaseTargetAnyway,
        GrabPickup,
        Heal,
        Wander,
        Null
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        logger = new GameLogger(); // Create a fresh logger at startup
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Waits until Player, Camera, and Simulator exist; then configures and runs the loop.
    /// Also teleports the player to this prefab's transform as the spawn point.
    /// </summary>
    protected override IEnumerator RunTest()
    {
        testRunning = true;
        isDead = false;


        //setup A Star pathfinding and wait for it to exist
        yield return InitializeAStar();

        //Setup the player inventory depending on the iteration
        PlayerInventorySetup(testIteration);

        // Session header
        logger.SessionStart(
            "BalanceTest",
            $"retarget={retargetInterval:F2}s timeout={testTimeout:F1}s"
        );

        // Wire up meleeRange from the serialized designer field
        meleeRange = Mathf.Max(0.01f, playerMeleeRange);

        // Find the path components that live under this tester
        _pathfinder = GetComponentInChildren<Pathfinder>(true);
        _graph = GetComponentInChildren<NodeGraph>(true);

        // Ensure Pathfinder knows its graph (in case prefab wasn't wired)
        yield return SetupPathfinder();

        //run main test
        yield return testLoop = StartCoroutine(TestLoop());

        testRunning = false;
    }
    #endregion

    #region Test Setup Methods
    private IEnumerator SetupPathfinder()
    {
        if (_pathfinder != null && _graph != null)
        {
            _pathfinder.SetGraph(_graph);
            // Wait until NodeGraph finishes Start() and builds connections
            // (gives it a frame; guard avoids infinite loop)
            int guard = 0;
            while (_graph.Connections != null && _graph.Connections.Count == 0 && guard++ < 180)
                yield return null;
        }
        else
        {
            Debug.LogWarning("[BalanceTest] Pathfinder/NodeGraph not found under this tester.");
        }
    }

    private IEnumerator InitializeAStar()
    {

        //Raise event asking for AStarPrefabRef from Tester
        //  Then wait until prefab exists
        //      however I dont want to add AStarPrefabRef to the abstractclass when most tests dont need it
        //      and trying to assign a value in case it exists feels risky
        GameController.instance.EventManager.Publish(EventType.GetAStarPrefab, new Action<GameObject>(prefab => aStarPrefab = prefab));
        yield return new WaitUntil(() => AStarPrefab != null);

        yield return null;
        if (AStarPrefab != null)
        {
            // Parent under this tester so GetComponentInChildren works
            aStar = Instantiate(AStarPrefab, transform);
            aStar.name = "A* (Runtime)";

            // Grab components now (they�ll also be discoverable later by GetComponentInChildren)
            _pathfinder = aStar.GetComponentInChildren<Pathfinder>(true);
            _graph = aStar.GetComponentInChildren<NodeGraph>(true);

            if (_pathfinder == null || _graph == null)
            {
                Debug.LogWarning("[BalanceTest] A* prefab missing Pathfinder/NodeGraph components.");
            }
            else
            {
                // If your Pathfinder has a serialized 'graph' field, make sure it�s wired:
                _pathfinder.SetGraph(_graph);                // requires a SetGraph method (see next snippet)
                _graph.SetAgentFromCapsule(player.GetComponent<CapsuleCollider2D>());
            }
        }
        else
        {
            Debug.LogWarning("[BalanceTest] Initialize called without A* prefab ref.");
        }
    }


    private void PlayerInventorySetup(int iteration)
    {
        // Find the TestManager object by tag
        GameObject testManagerObj = GameObject.FindWithTag("TestManager");
        if (testManagerObj == null)
        {
            Debug.LogError("No object with tag 'TestManager' found!");
            return; //early return
        }
        // Grab the TestRoomManager component
        TestRoomManager testManager = testManagerObj.GetComponent<TestRoomManager>();
        if (testManager == null)
        {
            Debug.LogError("TestRoomManager component not found on TestManager object!");
        }
        else
        {
            // Assign the tuple list from TestRoomManager
            itemList = testManager.ItemListAsTuples;
            // itemList is List<(Item, float)> float allows some items to only appear after 2 or 3 iterations
            foreach (var tuple in itemList)
            {
                // tuple.Item1 is the Item, tuple.Item2 is the quantity
                //      quanity is multiplied by the iteration count -1 (so the first iteration has 0 items)
                int totalQuantity = (int)(tuple.Item2 * (iteration - 1));
                var itemData = (tuple.Item1, (int)totalQuantity); //cant reassign tuple since its being iterated so make a new one

                // Publish the event
                if (totalQuantity > 0)
                    GameController.instance.EventManager.Publish(EventType.PlayerAddItem, itemData);
            }

        }
    }

    #endregion

    #region Test Implementation

    /// <summary>
    /// Main test loop with decision commitment:
    /// - Interrupts: melee-in-range, emergency heal
    /// - Re-pick only when: no decision, lock expired, goal achieved, or target invalid
    /// - Execute tick for the committed decision
    /// </summary>
    private IEnumerator TestLoop()
    {
        //initialize test timer
        testStart = Time.time;
        elapsed = 0f;

        //store starting health
        healthScript = player.GetComponent<HealthScript>();
        if (healthScript != null)
        {
            healthStart = healthScript.GetHealth;
            health = healthScript.GetHealth;
            healthMax = healthScript.GetMaxHealth;
        }

        //Keep running until timeout
        while (testRunning && !CheckTimeOut())
        {
            //only proceed if player isnt null, else wait until not
            if (player != null)
            {
                UpdateTargets(); // refresh nearest enemy & pickup on cadence

                // Update current health
                if (healthScript != null) health = healthScript.GetHealth;

                // End if there are truly no enemies left (safer than only checking closestEnemy)
                GameObject[] enemiesLeftNow = GameObject.FindGameObjectsWithTag(enemyTag);
                if (enemiesLeftNow == null || enemiesLeftNow.Length == 0) break;

                // ===== Interrupts that preempt current decision =====

                PickandCommitImmediateDecision();

                // ===== Need a new decision? =====
                //if current decision isnt set, or goal has been acheived, or decisiontarget is invalid
                if (currentDecision == PlayerDecision.Null
                    || DecisionGoalAchieved(currentDecision)  //accomplished goal
                    || DecisionTargetInvalid(currentDecision) //no longer able to chase target
                    || Time.time > decisionLockUntil) //timeout
                {
                    PickAndCommitDecision();
                }

                // ===== Execute current decision tick =====
                ExecuteDecisionTick(currentDecision);
            }
            yield return null; // next frame
        }
    }

    /// <summary>
    /// Only run test if enemies are present
    /// </summary>
    /// <returns></returns>
    protected override bool TestRunCheck()
    {
        bool enemiesPresent = false;

        //return true if at least one enemy in room with enemytag
        if (GameObject.FindGameObjectsWithTag(enemyTag).Length > 0)
        {
            enemiesPresent = true;
        }

        return enemiesPresent;
    }

    protected override IEnumerator StopTest()
    {
        TestResult results = CompileResults();
        GameController.instance.EventManager.Publish(EventType.RoomTestResults, results);

        //UnSubscribe to player death to avoid running until next test
        GameController.instance.EventManager.Unsubscribe(EventType.PlayerDeath, OnPlayerDeathHandler);
        GameController.instance.EventManager.Unsubscribe(EventType.PlayerDamaged, OnPlayerDamagedHandler);
        testComplete = true;

        //delete aStar 
        Destroy(aStar);
        aStar = null;

        yield return null; //give a frame to deal with the published events
    }

    #region Decision Handling
    private void SetDecision(PlayerDecision decision, float commitSeconds)
    {
        SetDecision(decision, null, null, commitSeconds);
    }
    private void SetDecision(PlayerDecision decision, GameObject enemy, GameObject pickup, float commitSeconds)
    {
        currentDecision = decision;
        decisionStartedAt = Time.time;

        // Clamp to global min/max but allow per-decision override via parameter
        float clamped = Mathf.Clamp(commitSeconds, decisionMinCommit, decisionMaxCommit);
        decisionLockUntil = Time.time + clamped;

        // Snapshot targets so we don't thrash mid-commit
        decisionEnemyTarget = enemy != null ? enemy : closestReachableEnemy;
        decisionPickupTarget = pickup != null ? pickup : closestItem;

        logger.Info("DecisionCommit", decision.ToString(), player != null ? player.transform.position : Vector2.zero, elapsed);
    }


    private void PickandCommitImmediateDecision()
    {
        if (ShouldEmergencyHeal() && CanHeal())
        {
            SetDecision(PlayerDecision.Heal, null, null, 0.05f);
        }
        else if (EnemyInMeleeNow())
        {
            SetDecision(PlayerDecision.MeleeAttack, closestReachableEnemy, null, 0.6f);
        }
    }
    private void PickAndCommitDecision()
    {
        Dictionary<PlayerDecision, int> decisions = new Dictionary<PlayerDecision, int>();
        int total = 0;

        //pre-determine what decisions are possible in one place for easy debugging

        //can we ranged attack
        bool canRangedAttack = false;
        bool inventoryExists = InventoryExists();
        if (inventoryExists && closestUnreachableEnemy != null)
        {
            canRangedAttack = inventory.FindAmmoItem();
        }
        //can we heal?
        bool canHeal = inventoryExists && CanHeal();
        //can we melee
        bool canMelee = (closestReachableEnemy != null);
        bool enemyIsReachable = IsReachable(player, closestReachableEnemy, playerMeleeRange);
        canMelee = canMelee && enemyIsReachable;
        //can we pickup an item?
        bool canPickup = closestItem != null && IsReachable(player, closestItem);

        // Ranged (only if we "have ammo" according to inventory)
        if (canRangedAttack)
        {
            decisions.Add(PlayerDecision.RangedAttack, projectilePriority);
            total += projectilePriority;
        }

        // Chase is available if we have any enemy AND they are reachable
        if (canMelee)
        {
            //Determine priority of chasing nearest enemy depending on distance
            int tempPriority = Mathf.Max(1, chasePriority - (int)DistanceToTarget(closestReachableEnemy));
            decisions.Add(PlayerDecision.ChaseTarget, tempPriority);
            total += tempPriority;
        }

        // Pickup (closer = higher weight; clamp to >=1)
        //     Ignore pickup if it's out of range
        if (canPickup)
        {
            //Determine priority of chasing nearest pickup depending on distance
            int tempPriority = Mathf.Max(1, pickupPriority - (int)DistanceToTarget(closestItem));
            decisions.Add(PlayerDecision.GrabPickup, tempPriority);
            total += tempPriority;
        }

        // Heal (missing HP = priority)
        if (canHeal)
        {
            //Determine priority
            //the more damage taken, multiply by healPriority to increase urgency
            int tempPriority = (healthMax - health) * healPriority; decisions.Add(PlayerDecision.Heal, tempPriority);
            total += tempPriority;
        }

        //If decision was made, reset noViableDecision Counter
        if (total != 0)
        {
            noViableDecisionCounter = 0;
        }
        // Fallback: If nothing else to do, either wander or try to approach the nearest enemy (even if they cant be reached)
        else
        {
            //stop any movement
            InputSimulator.StopMove();

            //wander
            decisions.Add(PlayerDecision.Wander, 1);

            //or if able, chase an "unreachable" target, else wander is the only option find out why
            if (closestReachableEnemy != null)
                decisions.Add(PlayerDecision.ChaseTargetAnyway, 4);
            else
            {
                //count how often this is an issue
                noViableDecisionCounter++;

                //If unable to make a decision too many times in a row, provide decision logic and log error
                //  will still wander until a decision is made or test timeout
                if (noViableDecisionCounter > noViableDecisionMax)
                {
                    //If not able to chase any enemies, clear the blacklist in case it's holding valid targets
                    blacklistExpirationDict.Clear();
                    UpdateTargets();

                    //determine number of remaining enemies
                    GameObject[] enemiesLeftNow = GameObject.FindGameObjectsWithTag(enemyTag);
                    string errorMsg = NoDecisionToString(inventoryExists, ref canPickup, enemiesLeftNow);

                    Debug.Log(errorMsg);
                    logger.Info("Error", errorMsg, player.transform.position, elapsed);

                    noViableDecisionCounter = 0;
                }
            }
        }

        PlayerDecision picked = PlayerDecision.Null;
        if (total > 0)
        {
            int roll = UnityEngine.Random.Range(0, total); // int: max-exclusive
            int value = 0;
            foreach (var kv in decisions)
            {
                value += kv.Value;
                if (roll < value) { picked = kv.Key; break; }
            }

            // Per-decision commit windows (tweak feel here)
            float commit =
                (picked == PlayerDecision.ChaseTarget) ? UnityEngine.Random.Range(1.2f, decisionMaxCommit) :
                (picked == PlayerDecision.GrabPickup) ? UnityEngine.Random.Range(0.9f, decisionMaxCommit) :
                (picked == PlayerDecision.RangedAttack) ? UnityEngine.Random.Range(0.8f, 1.5f) :
                (picked == PlayerDecision.Heal) ? 0.05f :
                (picked == PlayerDecision.MeleeAttack) ? 0.6f :
                UnityEngine.Random.Range(decisionMinCommit, decisionMaxCommit);

            switch (picked)
            {
                case PlayerDecision.ChaseTargetAnyway: //chase unreachable target anyway
                case PlayerDecision.RangedAttack:
                    //ranged attact may have a different target
                    SetDecision(picked, closestUnreachableEnemy, closestItem, commit);
                    break;

                //everything else uses closest reachable enemy
                default:
                    SetDecision(picked, closestReachableEnemy, closestItem, commit);
                    break;
            }
        }
    }

    private string NoDecisionToString(bool inventoryExists, ref bool canPickup, GameObject[] enemiesLeftNow)
    {
        string errorMsg = $"Unable to reach {enemiesLeftNow.Length} remaining enemies.";
        if (player == null)
        {
            errorMsg += "Player not found! ";
        }
        else
        {
            //can we use items?
            if (inventoryExists)
            {
                //Why can't we heal?
                if (healthScript == null)
                {
                    errorMsg += "Healthscript not found, cannot use heal items! ";
                }
                else if (health >= healthMax)
                {
                    errorMsg += "Full health, do not need to heal! ";
                }
                else if (!inventory.FindHealItem())
                {
                    errorMsg += "No health item, cannot heal! ";
                }

                //why cant we use ranged attacks?
                if (closestUnreachableEnemy == null)
                {
                    errorMsg += "No valid target for ranged attacks! ";
                }
                else if (!inventory.FindAmmoItem())
                {
                    errorMsg += "No ammo for ranged attacks! ";
                }

                //why can't we pick up an item?
                if (closestItem == null)
                {
                    errorMsg += "No pickups in range. ";
                }
                else if (!canPickup)
                {
                    //determine how many pickups cannot be grabbed
                    GameObject[] pickups = GameObject.FindGameObjectsWithTag(pickupTag);

                    errorMsg += $"{pickups.Length} Pickups out of range!";
                }
            }
            else
            {
                errorMsg += "Inventory not found, cannot use items! ";
            }

            //why can't chase the enemy?
            if (closestReachableEnemy == null)
            {
                errorMsg += "No reachable targets to melee! ";
            }
        }

        return errorMsg;
    }

    private bool DecisionTargetInvalid(PlayerDecision decision)
    {
        switch (decision)
        {
            case PlayerDecision.ChaseTarget:
            case PlayerDecision.ChaseTargetAnyway:
            case PlayerDecision.MeleeAttack:
            case PlayerDecision.RangedAttack:
                return decisionEnemyTarget == null;
            case PlayerDecision.GrabPickup:
                return decisionPickupTarget == null;
            default:
                return false;
        }
    }

    private bool DecisionGoalAchieved(PlayerDecision decision)
    {
        switch (decision)
        {
            case PlayerDecision.ChaseTarget:
            case PlayerDecision.ChaseTargetAnyway:
                // Goal: get close enough that melee can happen
                return decisionEnemyTarget == null || TargetInRange(decisionEnemyTarget, meleeRange);

            case PlayerDecision.GrabPickup:
                // Goal: reached pickup (or it got consumed/destroyed)
                if (decisionPickupTarget == null) return true;
                return DistanceToTarget(decisionPickupTarget) <= pickupReachDistance;

            case PlayerDecision.RangedAttack:
                // Goal: ran out of ammo, or melee now viable
                bool hasAmmo = InventoryExists() && inventory.FindAmmoItem();
                bool meleeSoon = decisionEnemyTarget != null && TargetInRange(decisionEnemyTarget, meleeRange);
                return !hasAmmo || meleeSoon;

            case PlayerDecision.Heal:
                // Goal: healed enough or no item available
                if (!InventoryExists() || !inventory.FindHealItem()) return true;
                if (healthScript == null) return true;
                return healthScript.GetHealth >= healthScript.GetMaxHealth;

            case PlayerDecision.MeleeAttack:
                // Goal: target died or moved out of range
                if (decisionEnemyTarget == null) return true;
                return !TargetInRange(decisionEnemyTarget, meleeRange);
            case PlayerDecision.Wander:
                //stop wandering if there is something to chase
                return (closestReachableEnemy != null || closestItem != null);
            default:
                return true;
        }
    }

    private void ExecuteDecisionTick(PlayerDecision decision)
    {
        switch (decision)
        {
            case PlayerDecision.MeleeAttack:
                if (decisionEnemyTarget != null && TargetInRange(decisionEnemyTarget, meleeRange))
                {
                    Attack(decisionEnemyTarget);
                    meleeAttacks++;
                }
                break;

            case PlayerDecision.ChaseTargetAnyway:
            case PlayerDecision.ChaseTarget:
                if (decisionEnemyTarget != null)
                {
                    MoveToObject(decisionEnemyTarget);
                }
                //stop moving
                else
                {
                    InputSimulator.StopMove();
                }
                break;

            case PlayerDecision.GrabPickup:
                if (decisionPickupTarget != null)
                {
                    MoveToObject(decisionPickupTarget);
                }
                //stop moving
                else
                {
                    InputSimulator.StopMove();
                }
                break;

            case PlayerDecision.RangedAttack:
                if (decisionEnemyTarget != null &&
                    Time.time >= lastRangedFireTime + rangedFireInterval)
                {
                    RangedAttack(decisionEnemyTarget);
                    projectilesUsed++;
                    lastRangedFireTime = Time.time;
                }
                break;

            case PlayerDecision.Heal:
                // Fire heal once on enter; idle during commit
                if (Time.time < decisionLockUntil && InventoryExists() && inventory.FindHealItem()
                    && health < healthMax)
                {
                    Heal();
                    healsUsed++;
                }
                break;

            default: //Wander
                Wander();

                break;
        }
    }

    private void Wander()
    {
        if (verboseLogging)
            Debug.Log("Wandering");

        if (Time.time >= wanderDirectionLockUntil || currentWanderDirection == Vector2.zero || IsPlayerStuck())
        {
            //choose starting direction
            int startIndex = UnityEngine.Random.Range(0, directions.Length);
            Vector2 direction = Vector2.zero;

            //try to find a good direction starting from startIndex
            for (int i = 0; i < directions.Length; i++)
            {
                // Pick a random direction
                //  if result is more than the number of directions, start counting from start
                int index = (startIndex + i) % directions.Length;
                direction = directions[index];
                Vector2 targetPos = (Vector2)transform.position + direction;

                if (IsReachable(player.transform.position, targetPos))
                {
                    break; //leave for loop, we found a valid direction
                }
            }
            //Update wander direction duration
            wanderDirectionLockUntil = Time.time + UnityEngine.Random.Range(0.5f * wanderDirectionDuration, 1.2f * wanderDirectionDuration);

            //nudge the player
            currentWanderDirection = ApplyRandomNudge(direction, IsPlayerStuck() ? 5f : .3f);
        }

        //move player
        InputSimulator.Move(currentWanderDirection);
    }
    #endregion
    #region Helpers


    // Reacquire nearest enemy/pickup at interval
    private void UpdateTargets()
    {
        //cant find targets if player issnt findable
        if (player == null)
            return;

        float now = Time.time;

        // -------- ENEMIES --------
        if (now - lastEnemyRetargetTime >= retargetInterval)
        {
            //update both the closest melee enemy (reachable by pathing) and unreachable (requires projectiles)
            closestReachableEnemy = FindNearestNonBlacklisted(enemyTag, playerMeleeRange);
            closestUnreachableEnemy = FindNearestWithTag(enemyTag);
            lastEnemyRetargetTime = now;
        }

        // -------- PICKUPS --------
        if (now - lastPickupRetargetTime >= retargetInterval)
        {
            var newPickup = FindNearestNonBlacklisted(pickupTag);
            if (newPickup != null)
            {
                closestItem = newPickup;
            }
            lastPickupRetargetTime = now;
        }
    }

    //finds the nearest enemy that is not blacklisted (Marked as unreachable)
    private GameObject FindNearestNonBlacklisted(string tag, float withinRange = 0f)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs == null || objs.Length == 0) return null;

        GameObject best = null;
        float bestDist = float.MaxValue;
        Vector3 p = player.transform.position;

        foreach (var obj in objs)
        {
            if (obj == null) continue;

            //skip if target is blacklisted
            if (IsTargetBlacklisted(obj) || !IsReachable(player, obj, withinRange)) continue;

            float d = (obj.transform.position - p).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = obj;
            }

        }
        return best;
    }


    private bool EnemyInMeleeNow()
    {
        return closestReachableEnemy != null && TargetInRange(closestReachableEnemy, meleeRange);
    }

    private bool ShouldEmergencyHeal()
    {
        if (player == null || healthScript == null) return false;

        float frac = (float)health / (float)healthMax;
        return frac <= emergencyHealThreshold;
    }

    /// <summary>
    /// Lazy access to inventory ensures inventory exists without multiple calls for it
    /// </summary>
    /// <returns></returns>
    private bool InventoryExists()
    {
        if (inventory == null)
        {
            // Acquire from your GameController singleton (as in your original)
            inventory = GameController.instance != null ? GameController.instance.ItemInventory : null;
        }
        return inventory != null;
    }

    private GameObject FindNearestWithTag(string tag)
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag(tag);
        if (objects == null || objects.Length == 0) return null;

        GameObject nearest = null;
        float nearestDistance = Mathf.Infinity;
        Vector2 currentPosition = player != null ? player.transform.position : transform.position;

        foreach (GameObject obj in objects)
        {
            if (obj == null) continue;
            float distance = Vector2.Distance(currentPosition, obj.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = obj;
            }
        }
        return nearest;
    }

    private float DistanceToTarget(GameObject target)
    {
        if (target == null || player == null) return 0f;
        return Vector2.Distance(player.transform.position, target.transform.position);
    }

    private bool TargetInRange(GameObject target, float range)
    {
        float distanceToTarget = DistanceToTarget(target);
        return distanceToTarget <= range;
    }

    /// <summary>
    /// Move towards object with a slight arc (so not walking directly into bullets)
    /// </summary>
    /// <param name="target"></param>
    private void MoveToObject(GameObject target)
    {
        if (target == null || player == null)
        {
            InputSimulator.StopMove();
            if (player != null) logger.Info("Chase", "target lost", player.transform.position, elapsed);
            return;
        }

        //get initial direction towards target
        Vector2 direction = (target.transform.position - player.transform.position).normalized;

        // Lateral vector perpendicular to movement direction (2D)
        Vector2 lateral = new Vector2(-direction.y, direction.x);

        // Update weave timer
        weaveTime += Time.deltaTime;

        // Add arc
        Vector2 weaveOffset = lateral * Mathf.Sin(weaveTime * weaveFrequency * Mathf.PI * 2) * weaveAmplitude;

        //Apply a slight nudge to help avoid getting stuck. If already stuck apply a larger nudge
        if (IsPlayerStuck())
        {
            direction = ApplyRandomNudge(direction + weaveOffset, 3f); // Larger than normal weave
        }
        else
        {
            direction = ApplyRandomNudge(direction + weaveOffset); // normal weave
        }

        //move player
        InputSimulator.Move(direction);
    }

    /// <summary>
    /// Apply a random nudge to movement to help avoid getting stuck
    /// </summary>
    /// <param name="moveDirection"></param>
    /// <param name="maxNudge"></param>
    /// <returns></returns>
    private Vector2 ApplyRandomNudge(Vector2 moveDirection, float maxNudge = .3f)
    {
        // Adds a small random vector perpendicular to the movement direction
        Vector2 lateral = new Vector2(-moveDirection.y, moveDirection.x);
        float nudgeAmount = UnityEngine.Random.Range(-maxNudge, maxNudge);
        return (moveDirection + lateral * nudgeAmount).normalized;
    }


    private void Attack(GameObject target)
    {
        if (!target || Camera.main == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);
        InputSimulator.Attack(screenPos);
        logger.Info("Attack", "pressed", player.transform.position, elapsed);
    }

    private void RangedAttack(GameObject target)
    {
        if (!target || Camera.main == null) return;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(target.transform.position);

        // Ensure projectile is selected
        if (InventoryExists()) inventory.FindAmmoItem();

        InputSimulator.Fire(screenPos);
        logger.Info("Projectile Fire", "pressed", player.transform.position, elapsed);
    }

    private void Heal()
    {
        // Ensure heal item is selected
        if (InventoryExists()) inventory.FindHealItem();

        InputSimulator.Fire(Vector2.zero);
        logger.Info("Heal Item", "pressed", player.transform.position, elapsed);
    }

    /// <summary>
    /// Returns true if the player is missing HP and has a heal item available.
    /// Also updates healPriority = (maxHP - currentHP) so healing gets more likely the lower you are.
    /// </summary>
    private bool CanHeal()
    {
        // Must have player + health to evaluate
        if (player == null || healthScript == null) return false;

        // Check inventory for a heal item (using your inventory API).
        // Note: FindHealItem() may also select it; that's fine for our use-case.
        bool hasHealItem = InventoryExists() && inventory.FindHealItem();

        return hasHealItem;
    }
    private bool ShouldSwitchTarget(float oldDist, float newDist)
    {
        // new must be closer by a ratio OR by an absolute margin
        bool ratioBetter = newDist < oldDist * (1f - retargetCloserRatio);
        bool absBetter = (oldDist - newDist) >= retargetCloserMin;
        return ratioBetter || absBetter;
    }

    #endregion
    #endregion

    #region A* methods
    // ---------- LoS + A* helpers ----------

    // 2D line-of-sight: treat Tilemap/Composite or BarrierTag objects as blockers.
    // No LayerMask needed.
    private bool HasLineOfSight2D(Vector2 from, Vector2 to)
    {
        Vector2 a = from; Vector2 b = to;
        Vector2 dir = b - a;
        if (dir.sqrMagnitude < 1e-8f) return true;

        var hits = Physics2D.LinecastAll(a, b);
        foreach (var h in hits)
        {
            var c = h.collider;
            if (c == null || c.isTrigger) continue;
            if (c.CompareTag("BarrierTag") || c is CompositeCollider2D || c is TilemapCollider2D)
                return false;
        }
        return true;
    }

    private bool IsReachable(GameObject source, GameObject target, float withinRange = 0f)
    {
        //return false if either object is false
        if (target == null || source == null)
            return false;

        //else get the positions and call overload
        Vector2 from = source.transform.position;
        Vector2 to = target.transform.position;
        return IsReachable(from, to, target, withinRange);
    }
    private bool IsReachable(Vector2 from, Vector2 to, GameObject target = null, float withinRange = 0f)
    {
        //without a pathfinder we cant determine if a path is possible
        //  if null, assume movement fallback will handle it
        if (_pathfinder == null) return true;

        // If blacklisted, the target cannot be reached return false
        if (target != null && IsTargetBlacklisted(target)) return false;

        //Use a temporary path and determine if it is followable
        var path = _pathfinder.FindPathWorld(from, to);
        bool isReachable = !(path == null || path.Count == 0);

        //if not reachable, try a point within range of target (if applicable)
        if (!isReachable && withinRange > 0)
        {
            //set a new point within range of target 
            to = WithinRangeOf(from, to, withinRange);

            path = _pathfinder.FindPathWorld(from, to);
            isReachable = !(path == null || path.Count == 0);
        }

        return isReachable;
    }

    private static Vector2 WithinRangeOf(Vector2 from, Vector2 to, float withinRange)
    {
        //If trying to get within withinRange of target, determine where that point would be
        if (withinRange > 0)
        {
            //ensure withinRange isnt further than the actual distance to target
            float dist = Vector2.Distance(from, to);
            withinRange = Mathf.Min(withinRange, dist);

            //Compute vector from source to target
            Vector2 direction = (to - from).normalized;

            //Reduce distance by withinRange to get a new target point
            to = to - direction * withinRange;
        }

        return to;
    }


    // Build (or rebuild) a path as needed
    private bool BuildPathTo(GameObject fromObject, GameObject target, float withinRange = 0f)
    {
        return BuildPathTo(fromObject.transform.position, target, withinRange);
    }
    private bool BuildPathTo(Vector2 from, GameObject target, float withinRange = 0f)
    {
        return BuildPathTo(from, target.transform.position, target, withinRange);
    }
    private bool BuildPathTo(Vector2 from, Vector2 to, GameObject target = null, float withinRange = 0f)
    {

        // If blacklisted, the target cannot be reached return false
        if (target != null && IsTargetBlacklisted(target)) return false;

        //check if path is stale/needs to be recalculated
        bool stale = this.path.Count == 0 || _wpIndex >= this.path.Count || (Time.time - _lastPathBuild) >= _pathRecomputeInterval;
        if (!stale) return true;

        // Compute a temporary path and determine if it is reachable
        var path = _pathfinder.FindPathWorld(from, to);
        bool isReachable = !(path == null || path.Count == 0);

        //if not reachable, try a point within range of target (if applicable)
        if (!isReachable && withinRange > 0)
        {
            //set a new point within range of target 
            to = WithinRangeOf(from, to, withinRange);

            path = _pathfinder.FindPathWorld(from, to);
            isReachable = !(path == null || path.Count == 0);
        }

        //if reachable, path to target and return true
        if (isReachable)
        {
            //update path
            this.path.Clear();
            this.path.AddRange(path);
            _wpIndex = 0;
            _lastPathBuild = Time.time;

            // if we havent returned false already, then we can route to target
            //  reset counters for target being unreachable
            if (target != null)
            {
                int id = target.GetInstanceID();
                failedPathAttemptsDict.Remove(id);
                blacklistExpirationDict.Remove(id);
            }

            return true;
        }
        //else target is not reachable, update blacklist
        else
        {
            //If target is repeatedly unreachable blacklist them so we stop checking
            if (target != null)
            {
                int id = target.GetInstanceID();
                if (!failedPathAttemptsDict.ContainsKey(id)) failedPathAttemptsDict[id] = 0;
                failedPathAttemptsDict[id]++;

                if (failedPathAttemptsDict[id] >= maxFailedPathAttempts)
                {
                    // Add to blacklist (expiry)
                    blacklistExpirationDict[id] = Time.time + unreachableBlacklistSeconds;
                    Debug.Log($"[AI] Target {target.name} marked unreachable (failed path attempts).");
                }
            }

            //target not reachable currently
            return false;
        }
    }

    private bool IsTargetBlacklisted(GameObject target)
    {
        if (target == null) return false;
        int id = target.GetInstanceID();
        if (blacklistExpirationDict.TryGetValue(id, out float expiry))
        {
            if (Time.time > expiry)
            {
                // expired — remove and allow retries
                blacklistExpirationDict.Remove(id);
                return false;
            }
            return true;
        }
        return false;
    }

    // Follow one step of the current path using your InputSimulator
    private void FollowPathStep()
    {
        if (_wpIndex >= path.Count || player == null)
        {
            InputSimulator.StopMove();
            return;
        }

        Vector3 wp = path[_wpIndex];
        wp.z = player.transform.position.z;

        Vector2 delta = (wp - player.transform.position);
        if (delta.sqrMagnitude <= _waypointEpsilonWorld * _waypointEpsilonWorld)
        {
            _wpIndex++;
            InputSimulator.StopMove();
            return;
        }

        InputSimulator.Move(delta.normalized);
    }

    // Smart move: direct if LoS, else A* path
    // Smart move: start A* if LoS is blocked and we're not already close.
    // Once we start following a path, we KEEP following it until distance <= _pathStopDistance.
    private void MoveTowardWithLoS(GameObject target)
    {
        if (!target || !player) { InputSimulator.StopMove(); return; }


        if (IsPlayerStuck())
        {
            Debug.Log("[AI] Player stuck! Recomputing path...");
            path.Clear();
            _wpIndex = 0;
            BuildPathTo(player.transform.position, target.transform.position); // recompute path
        }

        Vector2 from = player.transform.position;
        Vector2 to = target.transform.position;
        float dist = Vector2.Distance(from, to);

        bool hasActivePath = path.Count > 0 && _wpIndex < path.Count;
        bool closeEnough = dist <= _pathStopDistance;

        // If we already have a path, keep following it until we're closeEnough
        if (hasActivePath)
        {
            if (closeEnough)
            {
                // stop pathing when near target; switch to direct steering
                path.Clear();
                _wpIndex = 0;
                MoveToObject(target);
            }
            else
            {
                BuildPathTo(from, to);
                FollowPathStep();
            }
            return;
        }

        // No active path yet:
        // If LoS is blocked and we're not already close, start pathfind.
        if (!HasLineOfSight2D(from, to) && !closeEnough)
        {
            BuildPathTo(from, to);
            FollowPathStep();
        }
        else
        {
            // Either LoS is clear OR we're already close -> go direct
            MoveToObject(target);
        }
    }
    private bool IsPlayerStuck()
    {
        stuckCheckTimer += Time.deltaTime;
        if (stuckCheckTimer >= stuckCheckInterval)
        {
            stuckCheckTimer = 0f;

            if (Vector2.Distance(player.transform.position, lastPlayerPos) < stuckThreshold)
            {
                lastPlayerPos = player.transform.position;

                //Edit: Add a check here "can i move in any direction" if not then raise an exception
                return true;
            }

            lastPlayerPos = player.transform.position;
        }

        return false;
    }


    #endregion

    #region TestLogging
    private TestResult CompileResults()
    {
        // Get last player position if still alive
        Vector2 playerPosition = Vector2.zero;
        if (player != null) playerPosition = player.transform.position;

        // Remaining enemies
        int remainingEnemies = 0;
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        if (enemies != null) remainingEnemies = enemies.Length;

        // Time formatting
        int minutes = Mathf.FloorToInt(elapsed / 60f);
        float seconds = elapsed % 60f;

        // End health
        int endHealth = 0;
        if (healthScript != null && !isDead)
        {
            endHealth = healthScript.GetHealth;
        }

        // Status
        string status;
        if (endHealth <= 0 || isDead)
        {
            status = "Dead";
        }
        else if (elapsed >= testTimeout)
        {
            status = "Timeout";
        }
        else status = "Cleared";

        // Results
        string resultString = "Results: " +
                                $"Starting Health =[{healthStart}] " +
                                $"Heals Used =[{healsUsed}] " +
                                $"Damage Taken =[{damageTaken}] " +
                                $"Projectiles Fired =[{projectilesUsed}] " +
                                $"Melee Attacks =[{meleeAttacks}] ";
        if (remainingEnemies > 0)
            resultString = resultString + $"Remaining Enemies =[{remainingEnemies}]";
        if (minutes > 0)
            resultString = resultString + $"Time =[{minutes} minutes and {seconds:F2} seconds]";
        else
            resultString = resultString + $"Time =[{seconds:F2} seconds]";

        logger.Result(status, resultString, playerPosition, elapsed);
        logger.Write(LogFileName);

        //raise event with all data from tests
        TestResult results = new TestResult();
        results.roomName = roomName;
        results.status = status;
        results.resultString = resultString;
        results.healthStart = healthStart;
        results.healthEnd = endHealth;
        results.damageTaken = damageTaken;
        results.healsUsed = healsUsed;
        results.projectilesUsed = projectilesUsed;
        results.meleeAttacks = meleeAttacks;
        results.remainingEnemies = remainingEnemies;
        results.totalTimeSeconds = elapsed;
        return results;
    }

    #endregion

    #region Event Handling

    protected override void SubscribeAllEvents()
    {
        //Subscribe to player death so can record result
        GameController.instance.EventManager.Subscribe(EventType.PlayerDeath, OnPlayerDeathHandler);
        GameController.instance.EventManager.Subscribe(EventType.PlayerDamaged, OnPlayerDamagedHandler);
    }
    protected override void UnsubscribeAllEvents()
    {
        //Subscribe to player death so can record result
        GameController.instance.EventManager.Unsubscribe(EventType.PlayerDeath, OnPlayerDeathHandler);
        GameController.instance.EventManager.Unsubscribe(EventType.PlayerDamaged, OnPlayerDamagedHandler);
    }


    public void OnPlayerDeathHandler(object data = null)
    {
        // End test and record failure (the testloop will exit when it sees testRunning = false)
        isDead = true;
        testRunning = false;
    }

    public void OnPlayerDamagedHandler(object data = null)
    {
        //incoming data should be passed as (damage, currenthealth, healthCap) is (int, int , int)
        if (data is ValueTuple<int, int, int> healthData)
        {
            damageTaken += healthData.Item1;
        }
    }

    private float DistanceFromPlayer(GameObject go)
    {
        if (player == null || go == null) return float.PositiveInfinity;
        return Vector2.Distance(player.transform.position, go.transform.position);
    }

    #endregion

    #region Debug Gizmos
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || player == null) return;

        // Draw closest melee enemy in red
        if (closestReachableEnemy != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(closestReachableEnemy.transform.position, 0.2f);
            Gizmos.DrawLine(player.transform.position, closestReachableEnemy.transform.position);
        }

        // Draw closest ranged enemy in blue
        if (closestUnreachableEnemy != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(closestUnreachableEnemy.transform.position, 0.2f);
            Gizmos.DrawLine(player.transform.position, closestUnreachableEnemy.transform.position);
        }

        // Draw closest pickup in green with radius
        if (closestItem != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(closestItem.transform.position, 0.15f);
            Gizmos.DrawWireSphere(closestItem.transform.position, pickupReachDistance); // optional pickup reach
            Gizmos.DrawLine(player.transform.position, closestItem.transform.position);
        }

        // Draw melee range around player
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.transform.position, meleeRange);

        // Draw A* path
        if (path != null && path.Count > 0)
        {
            Gizmos.color = Color.cyan;
            Vector3 last = player.transform.position;
            for (int i = _wpIndex; i < path.Count; i++)
            {
                Gizmos.DrawLine(last, path[i]);
                last = path[i];
            }
        }

        // draw AI "nudge" area
        if (_drawPathGizmos)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(player.transform.position, weaveAmplitude);
        }
    }


    #endregion
}
