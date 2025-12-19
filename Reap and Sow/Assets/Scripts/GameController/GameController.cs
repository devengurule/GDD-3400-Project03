using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// GameController is a singleton globally accessible script used to interact with eventManager and collisionManager.
/// </summary>
public class GameController : MonoBehaviour
{
    public static GameController instance;   // Singleton instance of the GameController, making it globally accessible

    // References to EventManager and CollisionManager, responsible for handling game events and collisions
    private EventManager eventManager;
    private CollisionManager collisionManager;
    private bool isPaused = false;

    // References to inventories for item usage
    private Inventory itemInventory;

    // Public properties for accessing EventManager and CollisionManager safely
    public EventManager EventManager { get => eventManager; private set => eventManager = value; }
    public CollisionManager CollisionManager { get => collisionManager; private set => collisionManager = value; }

    // Public properties for accessing inventories safely
    public Inventory ItemInventory { get => itemInventory; private set => itemInventory = value; }
    
    // Player spawn settings (position on the map where the player should be instantiated)
    public float playerSpawnX;
    public float playerSpawnY;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerSpawnPrefab;

    public Dictionary<string, bool> tutorialDict; // Dictionary for tutorial triggers
    public GameObject PausePanel;

    // List of bosses beaten 
    [SerializeField]
    private List<BossEnum> bossList;

    private GameObject player;
    private GameObject playerSpawner;
    private bool hasLoadedSaveThisScene = false;

    [SerializeField]
    private string playerTag = "PlayerTag";

    private Coroutine playerCoroutine;// used to ensure only one thing tries to spawn player at once

    public GameObject Player
    {
        get
        {
            if (player == null && (player = GameObject.FindGameObjectWithTag(playerTag)) == null)
            {
                TrySpawnPlayer();
            }

            return player;
        }
        set => player = value;
    }

    void Awake()
    {
        // Ensure only one instance exists
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Initialize the boss list 
        bossList = new List<BossEnum>();

        // Ensure tutorialDict is initialized
        tutorialDict = new Dictionary<string, bool>();
        // Store references to EventManager and CollisionManager attached to object
        if (EventManager == null)
        {
            eventManager = GetComponent<EventManager>();
        }
        if (CollisionManager == null)
        {
            CollisionManager = GetComponent<CollisionManager>();
        }

        TrySpawnPlayer();
    }

    private void Start()
    {
        // Store references to inventories attatched to object
        if (ItemInventory == null)
        {
            itemInventory = new Inventory();
        }
        // Get reference to event manager
        EventManager eventManager = GameController.instance.EventManager;

        // Subscribe to input events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.PauseTime, OnPauseTimeHandler);
            eventManager.Subscribe(EventType.ResumeTime, OnResumeTimeHandler);
            eventManager.Subscribe(EventType.Pause, OnPauseHandler);
            eventManager.Subscribe(EventType.BossDeath, OnBossDeath);
            eventManager.Subscribe(EventType.GetPlayer, OnGetPlayerHandler);
            eventManager.Subscribe(EventType.EnterFarm, OnEnterFarmHandler);
            eventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
            eventManager.Subscribe(EventType.ResetPlayer, OnResetPlayerHandler); //on reset make sure player exists
        }
    }

    /// <summary>
    /// Checks if a specified boss has been beaten
    /// </summary>
    /// <param name="boss">Boss to search for</param>
    /// <returns>A bool for if boss has been beaten</returns>
    public bool IsBossBeaten(BossEnum boss)
    {
        return bossList.Contains(boss);
    }

    public BossEnum[] GetAllBosses()
    {
        BossEnum[] returnArray = new BossEnum[bossList.Count]; // Array to be returned

        // Copy all items from itemList to returnArray
        for (int i = 0; i < bossList.Count; i++)
        {
            returnArray[i] = bossList[i];
        }

        return returnArray;
    }

    public void SetAllBosses(BossEnum[] bosses)
    {
        bossList.Clear(); // Clear current list

        if (bosses == null) return;

        for (int i = 0; i < bosses.Length; i++)
        {
            bossList.Add(bosses[i]);
        }
    }

    /// <summary>
    /// When called raises a return event with reference to the player object This can be used to get player object reliably
    /// </summary>
    /// <param name="value"></param>
    public void OnGetPlayerHandler(object value = null)
    {
        //Determine who is asking and return the player object
        if (value is Action<GameObject> returnMethod)
        {
            //Use the lazyloading Player variable and send reference
            returnMethod(Player);
        }

        //else try to load player
        TrySpawnPlayer();
    }

    public void OnResetPlayerHandler(object value = null)
    {
        //try to load player
        TrySpawnPlayer();
    }

    /// <summary>
    /// Toggles Pause Menu when when event is raised
    /// </summary>
    /// <param name="target"></param>
    public void OnPauseHandler(object value = null)
    {
        //Pause and display pause menu if not already paused. else unpause and unfreeze time
        if (isPaused == true)
        {
            //Unpause
            PausePanel.SetActive(false);
            eventManager.Publish(EventType.ResumeTime, value);
            isPaused = false;
        }
        else
        {
            //Pause
            //  delay timestop so that it still works without interfering with pause menu
            eventManager.DelayedPublish(EventType.PauseTime, value);
            PausePanel.SetActive(true);
            isPaused = true;
        }
    }

    /// <summary>
    /// Actually pauses GAME TIME when when event is raised
    /// </summary>
    /// <param name="target"></param>
    public void OnPauseTimeHandler(object value = null)
    {
        Time.timeScale = 0;
    }

    /// <summary>
    /// Unpauses GAME TIME the game when is raised
    /// </summary>
    /// <param name="target"></param>
    public void OnResumeTimeHandler(object value = null)
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// Attempts to spawn the player when reentering the farm
    /// </summary>
    /// <param name="value"></param>
    public void OnEnterFarmHandler(object value = null)
    {
        TrySpawnPlayer();
    }

    public void OnEnterSceneHandler(object value = null)
    {
        // Get the main camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogWarning("[OnEnterSceneHandler] No Main Camera found!");
            return;
        }

        // Move this object to the camera's position
        transform.position = cam.transform.position;
    }

    /// <summary>
    /// Tries to spawn player through playerSpawn object (respawn animation)
    /// </summary>
    public void TrySpawnPlayer()
    {
        if (playerCoroutine == null)
        {
            playerCoroutine = StartCoroutine(GetPlayerWhenReady());
        }
    }

    /// <summary>
    /// Spawn the actual player (not the player spawner)
    /// </summary>
    /// <param name="location"></param>
    public void SpawnPlayerActual(Vector2 location)
    {
        if (player == null)
        { 
            player = Instantiate(playerPrefab, location, Quaternion.identity);
            if (!hasLoadedSaveThisScene)
            {
                Debug.Log("load save game event");
                hasLoadedSaveThisScene = true;
                GameController.instance.EventManager.Publish(EventType.loadsave, false);
            }
        }
    }

        private IEnumerator GetPlayerWhenReady()
    { 
        // Spawn Player at designated location if they don't already exist
        if (playerSpawner == null && player == null)
        {
            Vector3 spawnLocation = new Vector3(playerSpawnX, playerSpawnY, 0);
            playerSpawner = Instantiate(playerSpawnPrefab, spawnLocation, Quaternion.identity);

            // Get SpawnAnimation component
            SpawnAnimation spawnScript = playerSpawner.GetComponent<SpawnAnimation>();
            if (spawnScript != null)
            {
                // Assign callback to spawn the real player when animation finishes
                spawnScript.OnSpawnComplete(SpawnPlayerActual);
            }
        }

        //wait until player actually exists
        float timeout = 1f; // seconds
        float elapsedTime = 0f;

        while (player == null && elapsedTime < timeout)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        if (player == null)
        {
            Debug.LogWarning("Player failed to spawn!");
        }

        //nullify reference to playerCoroutine so it can be run again if needed
        playerCoroutine = null;
    }

    public void OnBossDeath(object value)
    {
        //try to cast value to boss enumeration if possible
        if (value is BossEnum boss)
        {
            //make sure a valid enum was provided
            if (boss != BossEnum.None)
            {
                bossList.Add(boss);
                Debug.Log($"Boss added: {boss}. BossList now has {bossList.Count} entries.");
            }
        }
        else
        {
            Debug.LogWarning($"OnBossDeath called with wrong type: {value?.GetType()}");
        }
    }
}