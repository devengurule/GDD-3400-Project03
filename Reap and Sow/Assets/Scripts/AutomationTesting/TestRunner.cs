using System.Collections;           // Coroutines (IEnumerator) for async sequences like Bootstrap()
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// Bootstraps automated tests for the scene
/// Responsibilities:
/// - Configure time scale
/// - Optionally clear out scene clutter based on inspector filters
/// - Ensure HUD and a "GameController" object exist
/// - Wait for GameController.instance, its EventManager, and a tagged Player to be present
/// - Optionally enable player "god mode"
/// - Start the selected test (BoundaryTest or BalanceTest) and feed it the InputSimulator
/// </summary>

public class TestRunner : MonoBehaviour
{
    #region Fields


    [Header("Tests")]
    [SerializeField, Tooltip("set how many times to run test per room- Tests the explorable area of each room")]
    [Range(0, 20)] 
    private int boundaryTestsPerRoom = 1;

    [SerializeField, Tooltip("set how many times to run test per room- Simulates playing the room and attempts to defeat all enemies")]
    [Range (0,20)]
    private int balanceTestsPerRoom = 1;

    [Header("Test Settings")]
    [SerializeField, Tooltip("Maximum amount of time before a test stops running")]
    public float testTimeout = 100f;

    [SerializeField, Tooltip("Extra logs to the console.")]
    private bool verboseLogging = true;             // If true, print extra Debug.Log messages to aid debugging

    [SerializeField, Tooltip("1 = normal, 2 = double, 0.5 = half")]
    [Range(0.1f, 5f)] //adds a slider capped for safety (speed x10+ causes issues)
    private float gameSpeed = 1f;                   // Desired Time.timeScale

    [Header("Scene Objects- Must be set prior to running")]
    [SerializeField] private GameObject controller; // Provides the GameController

    //Edit: Have ALL Logging file strings stored here, and give to tests as needed.
    [Header("Logging")]
    [SerializeField] private string balanceLogName = "BalanceTest-Results.csv"; // Output CSV file written by GameLogger at end of run

    [SerializeField]
    public string testStatus = "None";

    #endregion
    //Store reference to the logger object used to log test results into a csv file
    private GameLogger logger;

    [SerializeField] private GameObject AStarPrefab;

    #region Internal Defaults
    private const string playerTag = "PlayerTag";           // Tag used to find the Player
    private const string gameControllerName = "GameController"; // Exact name required by HealthScript's GameObject.Find

    // Future proofing in case we ever use any physics
    private const float defaultFixedDelta = 0.02f;          // Unity default physics step (seconds) in case we use physics anywhere ever

    //Saved time valued
    private float originalTimeScale;                        // Cached Time.timeScale to restore later
    private float originalFixedDelta;                       // Cached Time.fixedDeltaTime to restore later

    private RoomMap map;
    private MapTest mapTest;                                // Component added to map accessible game area
    private EventManager eventManager;
    private GameObject player = null;
    public HealthScript healthScript = null;
    SceneData currentRoom;
    #endregion

    [ContextMenu("Start Test")]
    public void Start()
    {
        StartCoroutine(InitializeTestRunner());
    }
    private IEnumerator InitializeTestRunner()
    {
        //Store initial values such as tiemscale so they can be restored
        StoreInitialValues();

        //Wait until required features exist (Gamecontroller, EventManager, Player, ) 
        yield return WaitForAssetLoad();

        //disable tutorial
        eventManager.Publish(EventType.TutorialToggle, false);

        //Subscribe to eventManager event for logging
        eventManager.Subscribe(EventType.RoomTestResults, OnRoomTestResultHandler);
        eventManager.Subscribe(EventType.GetAStarPrefab, OnRoomGetAStarPrefabHandler);

        //setup final result logger for balancetest data
        logger = new GameLogger();

        // put column names at top of logger
        //Edit: Have individual tests handle their own loggers, as logging needs may vary
        logger.SessionStart(TestResult.getColNames());

        //Start selected Test
        yield return StartTest();
    }

    private IEnumerator ResetRoom()
    {
        //Revive all enemies
        GameController.instance.EventManager.Publish(EventType.ClearDeathTimer);

        //reload room
        yield return map.LoadRoom(currentRoom);
    }

    /// <summary>
    /// Ensure all required assetts have fully loaded before running
    /// </summary>
    /// <returns></returns>
    private IEnumerator WaitForAssetLoad()
    {
        int waitTimeout = 1000;
        int waitTime = 0;

        // Wait until the singleton GameController has been initialized
        GameController gameController = null;
        if (verboseLogging)
            Debug.Log($"Waiting for GameController instance");
        while ((gameController = GameController.instance) == null && waitTime < waitTimeout)
        {
            waitTime++;
            yield return null;
        }
        waitTime = 0;

        if (gameController == null)
            Debug.Log("GameController not started Properly!");
        else
        {
            // Wait until GameController exposes a valid EventManager reference
            eventManager = null;
            if (verboseLogging)
                Debug.Log($"Wait until eventManager exists");
            while ((eventManager = gameController.EventManager) == null && waitTime < waitTimeout)
            {
                waitTime++;
                yield return null;
            }
            waitTime = 0;
        }


        if (eventManager == null)
            Debug.Log("EventManager not started Properly!");

        // Wait up to ~1000 frames for a Player tagged "PlayerTag" to exist
        if (verboseLogging)
            Debug.Log($"Wait until player exists");
        while ((player = GameObject.FindGameObjectWithTag(playerTag)) == null && waitTime < waitTimeout)
        {
            waitTime++;
            yield return null;
        }
        if (player == null)
            Debug.Log("Player not spawned Properly!");
    }

    /// <summary>
    /// Determine which test to run and start it.
    /// </summary>
    private IEnumerator StartTest()
    {
        //publish test start event
        GameController.instance.EventManager.Publish(EventType.AutoTestStart);

        //Scene Setup for Tests
        ApplyTime();        // Apply time scale

        //Setup the Map and get reference to starting room
        map = new RoomMap();
        string startRoom = SceneManager.GetActiveScene().name;

        //get player position. if possible use player position else use gamecontrollers position
        Vector2 startPosition = (player != null) ? player.transform.position : transform.position;

        //===========================================
        //                Global Tests
        //      Is run once. may use multiple rooms
        //===========================================

        //mapTest will populate a map of the game, initialize a RoomMap object for it to use
        //  But we need to give it the map to use, so we can reference it later
        mapTest = GetComponent<MapTest>();
        mapTest.Map = map;
        Debug.Log("Tester: Mapping out accessible scenes");
        yield return RunGlobalTest(mapTest);

        //===========================================
        //                Room Tests
        //      Each room test is run room by room
        //===========================================

        ////Once test has completed, then we need to run the subtests for each room
        List<SceneData> roomList = map.GetAllEntranceData();
        Queue<SceneData> roomQueue = new Queue<SceneData>(roomList);
        Debug.Log("Tester: Initiating room by room testing");
        int totalRooms = roomQueue.Count;
        int remainingRooms = totalRooms;

        //Go through queue and test each room
        while (roomQueue.Count > 0)
        {
            //get next scene and load it
            currentRoom = roomQueue.Dequeue();
            yield return map.LoadRoom(currentRoom);
            remainingRooms --;

            //format scenename plus entrance coordinates (to distinguish between entrances in logs)
            //  formatting of coordinates makes them only display in #.# format
            string sceneName = $"{currentRoom.SceneName} " +
                $"({currentRoom.RespawnPosition.x:F1}," +
                $"{currentRoom.RespawnPosition.y:F1})";

            testStatus = $"[{remainingRooms}/{totalRooms}]" + sceneName;

            //run balance test
            yield return RunRoomTest(typeof(BalanceTest), balanceTestsPerRoom, sceneName);

            //run Boundary test
            yield return RunRoomTest(typeof(BoundaryTest), boundaryTestsPerRoom, sceneName);
        }


        //publish test end
        GameController.instance.EventManager.Publish(EventType.AutoTestStop);
        Debug.Log("Test Complete! Ending Game");

        //Exit game
        //this is done differently if in unity Editor or in executable
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
        #else
                        Application.Quit();
        #endif
    }

    // Runs a selected test type designed for use across all rooms
    private IEnumerator RunGlobalTest(AbstractTest test)
    {
        //reset player to ensure they exisat
        ResetPlayer();

        //Run the actual test
        Debug.Log($"Running {test.TestType} Test across all rooms");
        test.verboseLogging = verboseLogging;
        test.testTimeout = testTimeout;
        yield return test.Run("All Rooms");
        yield return null; //give a frame to finish closing test
    }

    /// <summary>
    /// Runs the selected test type
    ///     This will actually delete any old instances of the test and re-add them.
    ///     The reason for this is to avoid artifacts from old tests affecting new tests
    /// </summary>
    /// <param name="testType"></param>
    /// <param name="numTests"></param>
    /// <returns></returns>
    private IEnumerator RunRoomTest(System.Type testType, int numTests, string roomName)
    {
        //if somehow a previous test still exists delete if
        Component oldTest = gameObject.GetComponent(testType);
        if (oldTest != null)
        {
            Destroy(oldTest);
            yield return null; // ensure destruction before adding a new one
        }

        for (int iteration = 1; iteration <= numTests; iteration++)
        {
            //add new test of the test type
            AbstractTest test = (AbstractTest)gameObject.AddComponent(testType);

            //Reset room and player (health/inventory/respawn if needed)
            yield return ResetRoom();
            ResetPlayer();
            yield return null; //give a frame to finish resetting

            //Run the actual test
            Debug.Log($"{test.TestType} ({iteration}/{numTests}) Test: {roomName}");
            test.verboseLogging = verboseLogging;
            test.testTimeout = testTimeout;
            yield return test.Run(roomName,iteration);

            yield return null; //give a frame to finish closing test

            //delete test of the testType
            Destroy(test);
            yield return null; //give a frame to complete deletion
        }
    }

    private void ResetPlayer()
    {
        GameController.instance.EventManager.Publish(EventType.ResetPlayer);
    }

    #region Utility Functions


    /// <summary>
    /// Stores initial data to be restored later
    /// </summary>
    private void StoreInitialValues()
    {
        // Save current time settings so we can restore them after tests complete
        originalTimeScale = Time.timeScale;
        originalFixedDelta = Time.fixedDeltaTime;
    }

    /// <summary>
    /// Restore settings to defaults, so gameplay can proceed normally if desired
    /// Edit: Add OnDestroy and OnDisable calls to this
    /// </summary>
    private void RestoreInitialValues()
    {
        //Restore time
        RestoreTime();

        //Edit: Restore disabled objects

    }

    /// <summary>
    /// Set Time.timeScale and optionally compensate Time.fixedDeltaTime so physics (if any are ever added) remains consistent
    /// </summary>
    private void ApplyTime()
    {
        // Do not allow 0 here; that would pause and effectively freeze FixedUpdate()
        var timeScale = Mathf.Max(gameSpeed, 0.001f);
        Time.timeScale = timeScale;

        // Physics(FixedUpdate) advances in "game time" steps of fixedDeltaTime.
        // Real-time interval between physics ticks = fixedDeltaTime / timeScale.
        Time.fixedDeltaTime = defaultFixedDelta / timeScale;

        if (verboseLogging)
            Debug.Log($"[TestRunner] timeScale={Time.timeScale:0.###} fixedDelta={Time.fixedDeltaTime:0.###}");
    }

    /// <summary>
    /// Restore previously saved Time settings
    /// </summary>
    private void RestoreTime()
    {
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;

        if (verboseLogging)
            Debug.Log($"[TestRunner] restored timeScale={Time.timeScale:0.###} fixedDelta={Time.fixedDeltaTime:0.###}");
    }
    #endregion

    #region Event Handlers
    /// <summary>
    /// When room testresults are returned, log them
    /// </summary>
    /// <param name="data"></param>
    public void OnRoomTestResultHandler(object data = null)
    {
        if (data is TestResult result)
        {
            logger.Result(result);
            logger.Write(balanceLogName);
        }
    }

    public void OnRoomGetAStarPrefabHandler(object data = null)
    {
        if (data is Action<GameObject> listener && AStarPrefab != null)
        {
            listener.Invoke (AStarPrefab);
        }
    }
        
    #endregion


    ///The following code is used exclusively in Unity Editor
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        // Draw a small marker where this TestRunner sits; used as a visual hint for bootstrapper spawn
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
#endif
}
