
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UIElements;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PauseMenuSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    private DropdownField resolutionDropdown;
    private DropdownField qualityDropdown;
    private DropdownField textureDropdown;
    private DropdownField aaDropdown;
    private UnityEngine.UIElements.Slider MasterSlider;
    private UnityEngine.UIElements.Slider SFXSlider;
    private UnityEngine.UIElements.Slider MusicSlider;
    private float currentMAVolume;
    private float currentMUVolume;
    private float currentSVolume;
    Resolution[] resolutions;
    private UnityEngine.UIElements.Button SaveButton;
    private UnityEngine.UIElements.Button menuButton;
    private UnityEngine.UIElements.Button seedButton;
    private UnityEngine.UIElements.Button enemyButton;
    private UnityEngine.UIElements.Button controlButton;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    public GameObject seedsPanel;
    public GameObject enemyPanel;
    public GameObject devPanel;
    private bool loaded = false;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName hoverClip;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName clickClip;
    private bool activatesounds;
    private InputActionRebindingExtensions.RebindingOperation rebindOp;
    private const string RebindSaveKey = "Rebind_";

    [System.Serializable]
    public struct RebindEntry
    {
        public string buttonName;                  // Name of UI Toolkit button in UXML
        public InputActionReference action;        // Input Action to rebind
        [HideInInspector] public UnityEngine.UIElements.Button uiButton;  // Runtime reference
    }

    [SerializeField] private List<RebindEntry> rebinds = new();


    void OnEnable()
    {
        if (loaded == false && gameObject.activeInHierarchy == true)
        {
            loaded = true;
         

            if (gameObject.activeInHierarchy == true)
            {
               gameObject.SetActive(false);
            }
        }
        if(gameObject.activeInHierarchy == true)
        {
         
            // this is just getting the UIDocument component
            var uiDocument = gameObject.GetComponent<UIDocument>();

            // this is just getting the root UI element
            var root = uiDocument.rootVisualElement;

            var styleSheet = Resources.Load<StyleSheet>("StyleSheets/PanelAnimation");
            if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
                root.styleSheets.Add(styleSheet);

            // Animate any VisualElement with the "Basicbook" class
            var basicBook = root.Q<VisualElement>(className: "Basicbook");
            if (basicBook != null)
            {
                basicBook.SetEnabled(false); // Start disabled (opacity 0)
                basicBook.AddToClassList("Basicbook"); // Ensure class is applied
                StartCoroutine(EnableWithDelay(basicBook, 0.05f)); // Fade in
            }


            menuButton = root.Q<UnityEngine.UIElements.Button>("Menu");
            SaveButton = root.Q<UnityEngine.UIElements.Button>("Savebutton");

            MasterSlider = root.Q<UnityEngine.UIElements.Slider>("Master");
            SFXSlider = root.Q<UnityEngine.UIElements.Slider>("SFX");
            MusicSlider = root.Q<UnityEngine.UIElements.Slider>("Music");

            textureDropdown = root.Q<UnityEngine.UIElements.DropdownField>("Texture");


            SetupRebindButtons(root);
            LoadAllRebinds();

            Buttonevents(menuButton, root, "Menu", OnMenuButtonClick);

            Buttonevents(SaveButton, root, "Savebutton", OnSaveButtonClick);

            Sliderevents(MasterSlider, root, "Master");

            Sliderevents(SFXSlider, root, "SFX");

            Sliderevents(MusicSlider, root, "Music");

            Dropdownevents(textureDropdown, root, "Texture");

            LoadSettings();
            Time.timeScale = 0;


        }



    }

    void Buttonevents(UnityEngine.UIElements.Button button, VisualElement root, string buttonname, Action buttonevent)
    {
        // this is just getting the "next" button by name
        button = root.Q<UnityEngine.UIElements.Button>(buttonname);

      
        button.RegisterCallback<MouseEnterEvent>(evt => PlayHoverSound());

        // this is just adding click event listener
        button.clicked += buttonevent;
    }

    void Sliderevents(UnityEngine.UIElements.Slider slider, VisualElement root, string slidername)
    {
        // this is just getting the "next" button by name
        slider = root.Q<UnityEngine.UIElements.Slider>(slidername);

        slider.RegisterCallback<MouseEnterEvent>(evt => PlayHoverSound());
        
    }

    void Dropdownevents(DropdownField dropdown, VisualElement root, string dropname)
    {
        // this is just getting the "next" button by name
        dropdown = root.Q<UnityEngine.UIElements.DropdownField>(dropname);

        dropdown.RegisterCallback<MouseEnterEvent>(evt => PlayHoverSound());
    }

    void PlayHoverSound()
    {
        if (activatesounds)
        {
            AudioManager.Play(hoverClip, false, AudioType.SFX, gameObject, false);
        }

    }
    void PlayClickSound()
    {
        activatesounds = false;
        AudioManager.Play(clickClip, false, AudioType.SFX, gameObject, false);
    }

    IEnumerator EnableWithDelay(VisualElement element, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        element.SetEnabled(true);
        activatesounds = true;
    }

    IEnumerator FadeOutAndDisable(VisualElement element, float duration = 0.3f)
    {
        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    // this is just destroying all objects and moving to the main menu scene
    void OnMenuButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f));
        }

        Debug.Log("Menu clicked");

    }


    public void SetMaster(float volume)
    {
        audioMixer.SetFloat("Master", volume);
        currentMAVolume = volume;
    }

    public void SetSFX(float volume)
    {
        audioMixer.SetFloat("SFX", volume);
        currentSVolume = volume;
    }

    public void SetMusic(float volume)
    {
        audioMixer.SetFloat("Music", volume);
        currentMUVolume = volume;
    }


    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

    // Method to set resolution
    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width,
                  resolution.height, Screen.fullScreen);
    }

    // Method to set texture quality
    public void SetTextureQuality(int textureIndex)
    {
        Debug.Log("saved texture");
        Debug.Log(textureIndex);
        if (textureIndex == 0)
        {
            Debug.Log("tutorial on");
            if (GameController.instance.EventManager != null)
            {
                GameController.instance.EventManager.Publish(EventType.TutorialToggle, true);
            }
        }
        if (textureIndex == 1)
        {
            Debug.Log("tutorial off");
            if (GameController.instance.EventManager != null)
            {
                GameController.instance.EventManager.Publish(EventType.TutorialToggle, false);
            }
            
        }
    }

    // Method to set antialiasing
    public void SetAntiAliasing(int aaIndex)
    {
        QualitySettings.antiAliasing = aaIndex;
    }

    // Method to set graphics quality
    public void SetQuality(int qualityIndex)
    {
        if (qualityIndex != 6) 
                               
            QualitySettings.SetQualityLevel(qualityIndex);
        switch (qualityIndex)
        {
            case 0: // quality level - very low
                textureDropdown.index = 3;
                aaDropdown.index = 0;
                break;
            case 1: // quality level - low
                textureDropdown.index = 2;
                aaDropdown.index = 0;
                break;
            case 2: // quality level - medium
                textureDropdown.index = 1;
                aaDropdown.index = 0;
                break;
            case 3: // quality level - high
                textureDropdown.index = 0;
                aaDropdown.index = 0;
                break;
            case 4: // quality level - very high
                textureDropdown.index = 0;
                aaDropdown.index = 1;
                break;
            case 5: // quality level - ultra
                textureDropdown.index = 0;
                aaDropdown.index = 2;
                break;
        }

        qualityDropdown.index = qualityIndex;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    // Method to save player preferences
    public void SaveSettings()
    {

        if (MasterSlider != null)
        {
            float volume = MasterSlider.value;
            SetMaster(volume);
            PlayerPrefs.SetFloat("Master", volume);
            PlayerPrefs.GetFloat("Master");
        }

        if (SFXSlider != null)
        {
            float volume = SFXSlider.value;
            SetSFX(volume);
            PlayerPrefs.SetFloat("SFX", volume);
        }

        if (MusicSlider != null)
        {
            float volume = MusicSlider.value;
            SetMusic(volume);
            PlayerPrefs.SetFloat("Music", volume);
        }

        if (textureDropdown != null)
        {
            Debug.Log("texture set");
            int textureIndex = textureDropdown.index;
            SetTextureQuality(textureIndex);
            PlayerPrefs.SetInt("TextureQuality", textureIndex);
        }
        Debug.Log("saving");
        PlayerPrefs.Save();
        QualitySettings.SetQualityLevel(QualitySettings.GetQualityLevel(), true);

        Time.timeScale = 0;

        // if (resolutions == null || resolutionDropdown == null || resolutionDropdown.index < 0)
        // {
        //  Debug.LogWarning("Resolution dropdown or data is not initialized.");
        // return;
        // }

        // Resolution selectedResolution = resolutions[resolutionDropdown.index];
        //Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);
    }



    // this is just deactivating the pause panel and setting the time scale so the player can move
    void OnSaveButtonClick()
    {
        PlayClickSound();
        Debug.Log("save");
        SaveSettings();
    }

  

    void LoadSettings()
    {
        // Volume
        if (PlayerPrefs.HasKey("Master") && MasterSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Master");
            MasterSlider.value = savedVolume;
            SetMaster(savedVolume);
            Debug.Log("saving master");
        }

        if (PlayerPrefs.HasKey("SFX") && SFXSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("SFX");
            SFXSlider.value = savedVolume;
            SetSFX(savedVolume);
            Debug.Log("saving sfx");
        }

        if (PlayerPrefs.HasKey("Music") && MusicSlider != null)
        {
            float savedVolume = PlayerPrefs.GetFloat("Music");
            MusicSlider.value = savedVolume;
            SetMusic(savedVolume);
            Debug.Log("saving music");
        }

        // Texture Quality
        if (PlayerPrefs.HasKey("TextureQuality") && textureDropdown != null)
        {
            int textureIndex = PlayerPrefs.GetInt("TextureQuality");
            textureDropdown.index = textureIndex;
            SetTextureQuality(textureIndex);
            Debug.Log("saving tutorial");
        }



    }


    void SaveRebind(InputAction action)
    {
        string rebinds = action.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(RebindSaveKey + action.name, rebinds);
        PlayerPrefs.Save();
    }

    void LoadAllRebinds()
    {
        foreach (var entry in rebinds)
        {
            var action = entry.action.action;
            string saved = PlayerPrefs.GetString(RebindSaveKey + action.name, "");

            if (!string.IsNullOrEmpty(saved))
                action.LoadBindingOverridesFromJson(saved);

            UpdateRebindDisplay(entry);
        }
    }


    void UpdateRebindDisplay(RebindEntry entry)
    {
        var action = entry.action.action;

        if (action.bindings.Count > 0)
            entry.uiButton.text = action.bindings[0].ToDisplayString();
    }


    void SetupRebindButtons(VisualElement root)
    {
        for (int i = 0; i < rebinds.Count; i++)
        {
            var entry = rebinds[i];

            // Get the UI Button by name
            entry.uiButton = root.Q<UnityEngine.UIElements.Button>(entry.buttonName);

            if (entry.uiButton == null)
            {
                Debug.LogError("Rebind button not found: " + entry.buttonName);
                continue;
            }

            // Hover sound
            entry.uiButton.RegisterCallback<MouseEnterEvent>(evt => PlayHoverSound());

            // Assign rebind start
            entry.uiButton.clicked += () => StartInteractiveRebind(entry);

            // Put the modified entry back (only needed if it's a struct)
            rebinds[i] = entry;
        }
    }


    void StartInteractiveRebind(RebindEntry entry)
    {
        var action = entry.action.action;
        var button = entry.uiButton;

        button.text = "...";

        action.Disable();

        rebindOp = action.PerformInteractiveRebinding()
            //.WithControlsExcluding("Mouse")
            .OnMatchWaitForAnother(0.1f)
            .OnCancel(op =>
            {
                UpdateRebindDisplay(entry);
                action.Enable();
                op.Dispose();
            })
            .OnComplete(op =>
            {
                action.Enable();
                SaveRebind(action);
                UpdateRebindDisplay(entry);
                op.Dispose();
            });

        rebindOp.Start();
    }















    void OnDisable()
    {
        if (SaveButton != null || menuButton != null)
        {
            // this is just unsubscribing event to prevent memory leaks
            SaveButton.clicked -= OnSaveButtonClick;
            menuButton.clicked -= OnMenuButtonClick;
        }
    }
}