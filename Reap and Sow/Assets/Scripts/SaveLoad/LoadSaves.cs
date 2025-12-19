using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using static PauseMenuSettings;
using UnityEngine.InputSystem;
using System.Linq;

public class LoadSaves : MonoBehaviour
{
    public AudioMixer audioMixer;
    [SerializeField] private bool mainMenu = true;
    private EventManager eventManager;
    [SerializeField] private string [] bossrooms;
    private string levelLoad;
    private bool levelBoss;
    [SerializeField] private List<RebindEntry> rebinds = new();
    private const string RebindSaveKey = "Rebind_";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(EnableWithDelay(0.01f));
        if (GameController.instance != null && GameController.instance.EventManager != null)
        {
            eventManager = GameController.instance.EventManager;
            eventManager.Subscribe(EventType.loadsave, loadgamesaves);
        }
        else
        {
            Debug.LogWarning("LoadSaves: GameController or EventManager is NULL ");
        }

    }
    IEnumerator EnableWithDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        loadallsaves();
    }

    void loadallsaves()
    {
      
       Loadsettings();

    }

    void loadgamesaves(object obj)
    {
        if (mainMenu == false)
        {
            LoadGame();
        }
    }

    private void Loadsettings()
    {
        LoadAllRebinds();
        if (PlayerPrefs.HasKey("Master"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Master");
            audioMixer.SetFloat("Master", savedVolume);
            //Debug.Log("saving");
        }
        else
        {
            float basicvolume = 6.0f;
            PlayerPrefs.SetFloat("Master", basicvolume);
            audioMixer.SetFloat("Master", basicvolume);
        }

        if (PlayerPrefs.HasKey("SFX"))
        {
            float savedVolume = PlayerPrefs.GetFloat("SFX");
            audioMixer.SetFloat("SFX", savedVolume);
            //Debug.Log("saving");
        }
        else
        {
            float basicvolume = 4.0f;
            PlayerPrefs.SetFloat("SFX", basicvolume);
            audioMixer.SetFloat("SFX", basicvolume);
        }

        if (PlayerPrefs.HasKey("Music"))
        {
            float savedVolume = PlayerPrefs.GetFloat("Music");
            audioMixer.SetFloat("Music", savedVolume);
            //Debug.Log("saving");
        }
        else
        {
            float basicvolume = 2.0f;
            PlayerPrefs.SetFloat("Music", basicvolume);
            audioMixer.SetFloat("Music", basicvolume);
        }

        if (PlayerPrefs.HasKey("TextureQuality"))
        {
            int savedtutorial = PlayerPrefs.GetInt("TextureQuality");
            if(GameController.instance != null && GameController.instance.EventManager != null)
            {
                if (savedtutorial == 0)
                {
                    GameController.instance.EventManager.Publish(EventType.TutorialToggle, true);
                }
                if (savedtutorial == 1)
                {
                    GameController.instance.EventManager.Publish(EventType.TutorialToggle, false);
                }
            }
        }
        else
        {
            int basictutorial = 0;
            PlayerPrefs.SetInt("TextureQuality", basictutorial);
            if (GameController.instance.EventManager != null)
            {
                if (basictutorial == 0)
                {
                    GameController.instance.EventManager.Publish(EventType.TutorialToggle, true);
                }
                if (basictutorial == 1)
                {
                    GameController.instance.EventManager.Publish(EventType.TutorialToggle, false);
                }
            }
        }
    }



    void LoadAllRebinds()
    {
        foreach (var entry in rebinds)
        {
            var action = entry.action.action;
            string saved = PlayerPrefs.GetString(RebindSaveKey + action.name, "");

            if (!string.IsNullOrEmpty(saved))
                action.LoadBindingOverridesFromJson(saved);
        }
    }

    public void LoadGame()
    {
        string path = Application.persistentDataPath + "/save.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            StartCoroutine(LoadSceneAndRestore(data));
        }
        else
        {
            GameController.instance.EventManager.Publish(EventType.BossDeath, false);
        }
    }

    private IEnumerator LoadSceneAndRestore(SaveData data)
    {
        yield return new WaitForSecondsRealtime(1.0f);

      
        var player = GameObject.FindGameObjectWithTag("PlayerTag");
        var healthScript = player?.GetComponent<HealthScript>();
        var playerScript = player?.GetComponent<PlayerScript>();
        var inventory = playerScript?.PlayerInventory;
        var Gamecontroller = GameObject.FindGameObjectWithTag("GameController");
        var gameController = Gamecontroller?.GetComponent<GameController>();

        if (gameController != null)
        {
            gameController.SetAllBosses(data.bossCompleted);
        }

        if (healthScript != null)
        {
            healthScript.Sethealth(data.playerHealth);
        }

        // Restore inventory
        if (inventory != null && data.inventoryItems != null)
        {
            for (int i = 0; i < data.inventoryItems.Length; i++)
            {
                string itemName = data.inventoryItems[i];
                int amount = data.inventoryAmounts[i];

                if (!string.IsNullOrEmpty(itemName))
                {
                    // Load the Item ScriptableObject by name (from Resources/Items/)
                    Item item = Resources.Load<Item>($"Items/{itemName}");
                    if (item != null)
                    {
                        inventory.AddItem(item, amount);
                        Debug.Log("Inventory set item.");
                        eventManager.Publish(EventType.ItemAdded);
                    }
                    else
                    {
                        Debug.LogWarning($"Item '{itemName}' not found in Resources/Items/");
                    }
                }
            }
            GameController.instance.EventManager.Publish(EventType.TutorialToggle, false);
            
            Debug.Log("Inventory restored successfully.");
            //eventManager.Publish(EventType.ItemUsed);
        }
       


        yield return new WaitForSecondsRealtime(0.01f);
    }


    // Update is called once per frame
    void Update()
    {
       
    }
}
