using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gets the enemy health for the project
/// </summary>
public class HealthScript : MonoBehaviour
{
    #region Constant Variables
    //Tag Labels
    private const string HUDTag = "HUDTag";
    private const string PlayerTag = "PlayerTag";
    private const string HealthBarName = "HealthBar";
    private const string GameControllerName = "GameController";
    private const string PoisonCloudTag = "PoisonCloud";
    #endregion

    #region Fields
    //Serialized Fields always placed first

    //Simple Data Types
    [SerializeField] private int healthCap;
    [SerializeField] private int health;
    [SerializeField] public bool godMode;
    [SerializeField] private bool SpawnsAtNight;
    private string timeOfDay;
    [SerializeField] private string enemyNum;
    private string roomName;
    [SerializeField] private BossEnum bossType = BossEnum.None;
    private bool invincible = false;
    private bool preventDeathForTest = false;
    private Timer invicibilityTimer;
    private Timer AOEInvicibilityTimer;
    [SerializeField] private float invincibilityDuration = .05f;
    [SerializeField] private float AOEInvicibilityDuration = 1f;
    [SerializeField] private bool isImmuneToAOE;
    [SerializeField] private bool trackDeath;
    private bool isPlayer;

    //Other data types
    [SerializeField] private GameObject[] destroyWithMe;
    [SerializeField] private List<GameObject> shieldGenerators = new List<GameObject>();
    private Color defaultColor = Color.white;
    private GameObject HUD;
    private GlobalClockScript globalClockScript;
    private HealthBarScript healthBarScript;
    private MonsterDeathTracker monsterDeathTracker;
    private SpriteRenderer spriteRenderer;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private GameObject Deathparticle;
    [SerializeField] private GameObject Hitparticle;
    [SerializeField] private AudioClipName currentclip;
    private ScreenShake screenShake;
    [SerializeField] private bool IsBoss = false;
    private EventManager eventManager;
    private DeathDropPickup dropScript;
    private bool isInPoison;
    private int poisonDamage;

    #endregion

    public int GetHealth
    {
        get { return health; }
    }
    public int GetMaxHealth
    {
        get { return healthCap; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isInPoison = false;
        //check if mob has a dropitemscript attached
        dropScript = GetComponent<DeathDropPickup>();

        eventManager = GameController.instance.EventManager;


        HUD = GameObject.FindWithTag(HUDTag);
        globalClockScript = HUD.GetComponent<GlobalClockScript>();
        timeOfDay = globalClockScript.dayNight;

        // Setup invicibility timer
        invicibilityTimer = gameObject.AddComponent<Timer>();
        invicibilityTimer.AddTimerFinishedListener(InvincibilityEnd);

        AOEInvicibilityTimer = gameObject.AddComponent<Timer>();
        AOEInvicibilityTimer.AddTimerFinishedListener(PoisonDamage);

        //Get room name. these are used together to identify this mob for death/life tracking
        roomName = SceneManager.GetActiveScene().name;

        //enemyNum = get unique enemy number
        GameObject gameController = GameObject.Find(GameControllerName);
        monsterDeathTracker = gameController.GetComponent<MonsterDeathTracker>();
        enemyNum = monsterDeathTracker.GetUniqueID(this.name);

        //get spriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        defaultColor = spriteRenderer.color;

        //if the object is the player character reset health and subscribe to ResetPlayer events
        isPlayer = gameObject.CompareTag(PlayerTag);
        if (isPlayer)
        {
            ResetHealth();
        }

        //subscribe to resetPlayer event
        if (eventManager != null)
        {
            if (isPlayer)
            {
                eventManager.Subscribe(EventType.ResetPlayer, OnResetPlayerHandler);
                eventManager.Subscribe(EventType.AutoTestStart, OnAutoTestStartHandler);
                eventManager.Subscribe(EventType.AutoTestStop, OnAutoTestStopHandler);
            }

            //if shield generators exist, listen for their deaths
            else if (shieldGenerators.Count > 0)
            {
                eventManager.Subscribe(EventType.EnemyDeath, OnEnemyDeathHandler);
            }
        }

        //Check time of day
        if (timeOfDay == "Day")
        {
            if (SpawnsAtNight == true)
            {
                Destroy(gameObject);
            }
        }

        //if tracking death, determine if should be dead
        if (trackDeath)
        {
            bool dead = monsterDeathTracker.IsDead(roomName, enemyNum);
            if (dead)
            {
                Destroy(gameObject);
            }
        }
    }

    private void OnEnemyDeathHandler(object obj)
    {
        //Edit: Move this to an event and handler relationship
        //hide shield if applicable
        if (!IsShielded() && transform.childCount > 0)
        {
            Transform shieldTransform = transform.GetChild(0);
            if (shieldTransform != null)
            {
                shieldTransform.gameObject.SetActive(false);
                FlashColor(Color.red);
            }
        }
    }

    private bool IsShielded()
    {
        bool isShielded = false;

        //For each shield generator, if they no longer exist, remove them
        //  cant use foreach however, because we will change the list. so instead, iterate from end of list to start
        for (int i = shieldGenerators.Count - 1; i >= 0; i--)
        {
            if (shieldGenerators[i] == null)
            {
                shieldGenerators.RemoveAt(i);
            }
        }
        //only allow damage if no "Shield generators" alive
        if (shieldGenerators.Count > 0)
        {
            isShielded = true;
        }

        return isShielded;
    }

    public void PoisonDamage()
    {
        //if currently inside a poison area and not on damage cooldown from poison, then take damage
        if (isInPoison && !AOEInvicibilityTimer.Running)
        {
            TakeDamage(poisonDamage);

            //reset timer
            AOEInvicibilityTimer.Duration = AOEInvicibilityDuration;
            AOEInvicibilityTimer.Run();
        }
    }

    public void Sethealth(int h)
    {
        health = h;
        GetCheckHealthBar();
        healthBarScript.HeartChange(health, healthCap);
    }

    private void GetCheckHealthBar()
    {
        //recapture healthBarScript if needed
        if (healthBarScript == null)
        {
            GameObject HealthBarEditor = GameObject.Find(HealthBarName);
            healthBarScript = HealthBarEditor.GetComponent<HealthBarScript>();
        }
    }

    private bool CanTakeDamage()
    {
        //make sure mortality hasnt been disabled
        if (godMode || invincible || IsShielded())
        {
            return false;
        }

        //if havent already returned false, then return true (can take damage)
        return true;
    }

    /// <summary>
    /// This helps enemy to lose health
    /// </summary>
    /// <param name="damage"></param>
    public int TakeDamage(int damage)
    {
        //only damage if no "Shield generators" alive
        bool canDamage = CanTakeDamage();
        int actualDamage = 0;

        //only try to damage if not invincible/godmode
        //  But allow heals even when invincible (negative damage)
        if (canDamage || damage < 0)
        {
            //Temporarily store old health value
            int oldHealth = health;

            // health goes down from attack
            health -= damage;
            health = Mathf.Clamp(health, 0, healthCap);

            // Calculate how much health actually changed (positive = damage, negative = healing)
            //  This is used for the return
            actualDamage = oldHealth - health;

            // activates flash effect when hit
            // If damage is >0 then it hurt
            if (actualDamage > 0)
            {
                StartCoroutine(FlashColor(Color.red));
                AudioManager.Play(currentclip, loop: false, AudioType.SFX, gameObject, false);
                //if a valid hitparticle the use it
                if (Hitparticle != null)
                    Instantiate(Hitparticle, transform.position, Quaternion.identity);

                screenShake = Camera.main.GetComponent<ScreenShake>();
                // Trigger camera shake
                if (screenShake != null)
                {
                    screenShake.start = true;
                }
            }
            //If damage <0 then it healed
            else if (actualDamage < 0)
            {
                StartCoroutine(FlashColor(Color.green));
            }

            //Enemy dies when health is 0
            if (health <= 0)
            {
                //calls die function
                Die();
            }

            //if you take damage change the array size
            if (isPlayer)
            {
                //raise event saying Player took this much damage out of this much
                //  This should update the healthbarscript listener
                eventManager.Publish(EventType.PlayerDamaged, (actualDamage, health, healthCap));
            }
        }
        return actualDamage;
    }

    /// <summary>
    /// Function to help the enemy flash red
    /// </summary>
    /// <returns></returns>
    public IEnumerator FlashColor(Color color)
    {
        //sets the color and only flashes for 0.1 seconds
        spriteRenderer.material = flashMaterial;
        spriteRenderer.color = color;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.material = defaultMaterial;
        spriteRenderer.color = defaultColor;
    }

    /// <summary>
    /// Destroys Enemy Object
    /// </summary>
    [ContextMenu("Kill")]
    private void Die()
    {
        //if a valid hitparticle the use it
        if (Deathparticle != null)
            Instantiate(Deathparticle, transform.position, Quaternion.identity);

        if (!preventDeathForTest)
        {
            AudioManager.Play(currentclip, loop: false, AudioType.SFX, null, false);



            //load event manager to publish events
            EventManager eventManager = GameController.instance.EventManager;

            //If enemy was a boss play cutscenes before destroying self else destroy self
            if (IsBoss == true)
            {
                // Disable boss visuals and interactions instantly
                SpriteVisibilityToggle(false);
                GetComponent<Collider2D>().enabled = false;
                enabled = false;

                // Immediately register death so gameplay reacts ASAP
                eventManager.DelayedPublish(EventType.BossDeath, bossType);
            }

            //Attempt to drop items if applicable
            if (dropScript)
            {
                // Check drop chance
                dropScript.Die();
            }

            // If this is an enemy log their death
            if (trackDeath)
            {

                monsterDeathTracker.AddCorpse(roomName, enemyNum);
                eventManager.DelayedPublish(EventType.EnemyDeath, enemyNum);
            }

            //destroy object
            Destroy(gameObject);
        }
    }

    // Method that resets the health of the player
    public void ResetHealth()
    {
        health = healthCap;
        GetCheckHealthBar();
        healthBarScript.ResetHealth(healthCap);
    }

    // Method that makes the spriteRenderer toggleable outside of this script
    public void SpriteVisibilityToggle(bool state)
    {

        spriteRenderer.enabled = state;

    }

    public void RunInvincibilityFrames(float duration)
    {
        if (!invincible)
        {
            invincible = true;
            invicibilityTimer.Duration = duration;
            invicibilityTimer.Run();
        }
    }

    // Turn off invincibility upon timer end
    void InvincibilityEnd()
    {
        invincible = false;
        //Debug.Log("Invincibility Ended");
    }

    private void OnDestroy()
    {
        //unsubscribe all events
        GameController.instance.EventManager.Unsubscribe(EventType.ResetPlayer, OnResetPlayerHandler);
        GameController.instance.EventManager.Unsubscribe(EventType.AutoTestStart, OnAutoTestStartHandler);
        GameController.instance.EventManager.Unsubscribe(EventType.AutoTestStop, OnAutoTestStopHandler);

        if (trackDeath)
            GameController.instance.EventManager.Publish(EventType.EnemyDestroyed, enemyNum);

        //clear any "deleteondeath" items
        if (destroyWithMe != null && destroyWithMe.Length > 0)
        {
            foreach (GameObject obj in destroyWithMe)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //If entering a poisonous area, start taking damage
        if (collision.gameObject.tag == PoisonCloudTag && !AOEInvicibilityTimer.Running && !isImmuneToAOE)
        {
            poisonDamage = collision.gameObject.GetComponent<PoisonCloud>().GetDamage();
            isInPoison = true;

            //start taking damage
            PoisonDamage();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == PoisonCloudTag)
        {
            //mark that no longer in poison, and stop timer
            isInPoison = false;
            AOEInvicibilityTimer.Stop();
        }
    }

    void OnAutoTestStartHandler(object value)
    {
        if (isPlayer)
            preventDeathForTest = true;
    }
    void OnAutoTestStopHandler(object value)
    {
        preventDeathForTest = false;
    }

    void OnResetPlayerHandler(object value)
    {
        ResetHealth();
    }
}
