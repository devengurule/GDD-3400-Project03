using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractTest : MonoBehaviour
{
    //Child classes should overwrite these values if needing to change
    public virtual string TestType => "Unidentified";
    protected virtual bool PlayerGodMode => false;          // If true, set "god mode" flag on the player
    protected virtual bool DisableSceneChangers => true;
    protected virtual string LogFileName => "Unnamed Test.csv";

    protected virtual string[] TagsToDisable => new string[0]; // Default excludes EnemyTag so existing enemies can remain

    #region Variables

    public float testTimeout = 100f;
    public float elapsed = 0;
    protected int testIteration = 0;
    public bool verboseLogging { get; set; }

    //Store all disabled objects so they can be reenabled
    protected List<GameObject> disabledObjects = new List<GameObject>();

    //Reference to player object
    protected GameObject player;


    //  Tags used for lookups
    protected string pickupTag = "PickupTag";
    protected string enemyTag = "EnemyTag";

    //Time Tracking
    protected bool testRunning = false;
    protected float testStart = 0f;
    protected string roomName;

    // Logging
    public GameLogger logger = new GameLogger();

    protected Coroutine testCoroutine;
    #endregion

    #region Astract Methods (Must be implemented in tester)
    //Required Methods
    protected abstract IEnumerator RunTest(); //The actual Test logic
    protected abstract IEnumerator StopTest();

    //Unsubscribe and subscribe to events in a reliable location
    protected virtual void UnsubscribeAllEvents() { }
    protected virtual void SubscribeAllEvents() { }

    /// <summary>
    /// Any other desired setup
    /// </summary>
    protected virtual void Setup() { }
    #endregion

    /// <summary>
    /// Call ths to run the test
    /// </summary>
    /// <param name="roomName">The name of the current room, used for logging</param>
    /// <param name="testIteration">an int for the current iteration if running test more than once. default = 1</param>
    /// <returns></returns>
    public virtual IEnumerator Run(string roomName, int testIteration = 1)
    {
        //Pre-runtime checks to ensure test is needed
        if (TestRunCheck())
        {
            this.roomName = roomName;
            this.testIteration = testIteration;

            //Subscribe all events
            SubscribeAllEvents();

            //clear room of any unwanted objects
            DisableObjectsByTag();


            //Do any required test setup (defined by child class if needed)
            Setup();

            //Get reference to the player from GameController
            //wait until a valid player is found and keep trying
            if (verboseLogging)
                Debug.Log($"Waiting for player spawn");
            while (player == null)
            {
                GetPlayer();

                //Wait for a second then try again
                yield return new WaitForSeconds(1);
            }

            // If needed toggle invulnerability on the player's HealthScript via its public bool
            EnableGodModeDirect(player, PlayerGodMode);

            // remove scene changers if needed
            //  These wont be restored after the test if removed this way
            //  Edit: I have no clue why, but trying to simply disable scenechangers doesnt work, if can figure out why then make sure to reenable in Restore()
            if (DisableSceneChangers)
            {
                GameController.instance.EventManager.Publish(EventType.DisableSceneChanger);

                //wait all scenechangers have been disabled
                if (verboseLogging)
                    Debug.Log($"Disable ScemeChangers");
                while (FindObjectsOfType<SceneChanger>().Length > 0)
                {
                    if (verboseLogging)
                        Debug.Log($"Waiting for Scenechangers to be disabled");
                    yield return null; // wait until next frame
                }
            }

            //Test Logic runs here
            if (verboseLogging)
                Debug.Log($"Running {TestType} Test.RunTest");
            yield return testCoroutine = StartCoroutine(RunTest());

            //After test Stop() ensures proper closeout and logging
            Stop();
        }
        else
        {
            Debug.Log($"Test Criteria not met, skipping {TestType} test for {roomName}");
        }
    }

    /// <summary>
    /// Returns true if test is able to run
    ///     To be overridden if needed
    /// </summary>
    /// <returns></returns>
    protected virtual bool TestRunCheck()
    {
        return true;
    }

    public void Stop()
    {
        StartCoroutine(SafeStop());
    }

    protected virtual IEnumerator SafeStop()
    {
        //Only attempt to stop if test is actually running
        if (testCoroutine != null)
        {
            //Stop coroutine
            StopCoroutine(testCoroutine);
            testCoroutine = null;

            testRunning = false;

            //Run tests personal stop logic
            yield return StopTest();

            //Restore game environment
            Restore();

            //Unsubscribe from any events
            UnsubscribeAllEvents();
        }
    }

    protected virtual void Restore()
    {
        //Restore any initial values such as godmode
        EnableGodModeDirect(player, false);
        RestoreDisabledObjects();

        //Stop movement input to player
        InputSimulator.StopMove();
    }

    /// <summary>
    /// Directly enable god mode via HealthScript's public bool
    /// </summary>
    /// <param name="player">player object</param>
    /// <returns></returns>
    private bool EnableGodModeDirect(GameObject player, bool enable = true)
    {
        if (player != null)
        {
            // find player healthscript
            var healthScript = player.GetComponent<HealthScript>();

            if (!healthScript)
            {
                if (verboseLogging) Debug.LogWarning("[TestRunner] HealthScript not found on player; cannot enable god mode.");
                return false;
            }

            healthScript.godMode = enable;
            return true;
        }
        else
        {
            if (verboseLogging) Debug.LogWarning("[TestRunner] Player not found; cannot enable god mode.");
            return false;
        }
    }


    #region Events and handlers
    protected void GetPlayer()
    {
        //Publish a request for the player
        Action<GameObject> setPlayer = SetPlayer;
        GameController.instance.EventManager.Publish(EventType.GetPlayer, setPlayer);
    }

    /// <summary>
    /// Receives the response from GameController with the player object
    /// </summary>
    /// <param name="player"></param>
    public void SetPlayer(GameObject player)
    {
        if (player != null)
        {
            this.player = player;
        }
    }
    #endregion

    #region Helper Functions

    protected bool CheckTimeOut()
    {
        // Timeout
        elapsed = Time.time - testStart;
        bool timeout = elapsed >= (testTimeout);
        return timeout; //adjust for timescale
    }

    /// <summary>
    /// Remove existing scene objects that could interfere with a clean test run
    /// </summary>
    protected void DisableObjectsByTag()
    {
        // get objects by tag
        if (TagsToDisable != null)
        {
            for (int i = 0; i < TagsToDisable.Length; i++)
            {
                var tag = TagsToDisable[i];

                //use List .AddRange to add the array returned from Find method as seperate elements
                disabledObjects.AddRange(
                    GameObject.FindGameObjectsWithTag(tag));
            }
        }

        // Disable each object
        foreach (GameObject obj in disabledObjects)
        {
            SetObjectActiveState(obj, false);
        }
    }

    /// <summary>
    /// Safely enable and disable an object IF IT EXISTS
    /// </summary>
    /// <param name="item"> item to be diabled or enabled</param>
    /// <param name="enabled">whether to enable (true) or disable (false)</param>
    private void SetObjectActiveState(GameObject item, bool enabled)
    {
        if (item != null)
        {
            item.SetActive(enabled);
        }
    }

    private void RestoreDisabledObjects()
    {
        // Disable each object
        foreach (GameObject obj in disabledObjects)
        {
            SetObjectActiveState(obj, true);
        }
    }
    #endregion
}
