using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveScript : MonoBehaviour
{
    private PlayerScript playerScript;
    private HealthScript healthScript;
    private EventManager eventManager;
    private Inventory inventoryScript;
    private PersistenceScript persistenceScript;
    private SceneChanger sceneChange;
    private GameController gameController;
    private string savePath;

    void Start()
    {
        savePath = Application.persistentDataPath + "/save.json";
        Findscripts(null);
        eventManager.Subscribe(EventType.EnterScene, Findscripts);

    }

    public void Findscripts(object obj)
    {

        eventManager = GameController.instance.EventManager;
        var player = GameObject.FindGameObjectWithTag("PlayerTag");
        playerScript = player?.GetComponent<PlayerScript>();
        healthScript = player?.GetComponent<HealthScript>();
        inventoryScript = playerScript?.PlayerInventory;
        sceneChange = GameObject.FindAnyObjectByType<SceneChanger>();
        var Gamecontroller = GameObject.FindGameObjectWithTag("GameController");
        gameController = Gamecontroller?.GetComponent<GameController>();


        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.SaveGame, SaveGame);
        }
        else
        {
            Debug.LogWarning("EventManager not found!");
        }

        if (playerScript == null)
        {
            Debug.LogWarning("PlayerScript not found!");
        }

        if (healthScript == null)
        {
            Debug.LogWarning("HealthScript not found!");
        }

        if (inventoryScript == null)
        {
            Debug.LogWarning("inventoryScript not found!");
        }
        if (sceneChange == null)
        {
            Debug.LogWarning("persistance not found!");
        }
        if (gameController == null)
        {
            Debug.LogWarning("Gamecontroller not found!");
        }
    }

    // Called by EventManager (Pause Menu Exit)
    public void SaveGame(object target)
    {
        SaveGameNow();
    }

    // Called when player quits the game or app closes
    void OnApplicationQuit()
    {
        SaveGameNow();
    }

    public void SetCurrentScene(string cs)
    {
       // currentScene = cs;
    }

    private void SaveGameNow()
    {

        if (playerScript == null || healthScript == null)
        {
            Debug.LogWarning("Cannot save � missing references.");
            return;
        }
        //Vector2 playerLoc = playerScript.transform.position;

        //Vector2 plotLoc = persistenceScript.transform.position;

    
        // Get all inventory data
        Item[] items = inventoryScript.GetAllItems();
        int[] amounts = inventoryScript.GetAllAmounts();

        string[] itemNames = new string[items.Length];
        for (int i = 0; i < items.Length; i++)
        {
            itemNames[i] = items[i] != null ? items[i].name : ""; 
        }



        //Scene scene = SceneManager.GetActiveScene();
        //string currentScene = scene.name;

        SaveData saveData = new SaveData
        {
            //playerLocation = playerLoc,
            //playerRoom = currentScene,
            playerHealth = healthScript.GetHealth,
            inventoryItems = itemNames,
            inventoryAmounts = amounts,
            bossCompleted = gameController.GetAllBosses(),

        };
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(savePath, json);

        Debug.Log($"Game saved to {savePath}");
    }
}
