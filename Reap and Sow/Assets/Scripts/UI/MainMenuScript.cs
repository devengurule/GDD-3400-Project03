using System;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System.IO;

public class MainMenuScript : MonoBehaviour
{
    private Button startButton;
    private Button newgameButton;
    private Button settingsButton;
    private Button creditsButton;
    private Button exitButton;
    public GameObject creditsPanel;
    public GameObject settingsPanel;
    public GameObject pausePanel;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName hoverClip;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName clickClip;
    private bool activatesounds;
    Animator transitionAnimator;
    private string savePath;

    void Awake()
    {
        savePath = Path.Combine(Application.persistentDataPath, "save.json");

        // Force directory creation on first launch
        Directory.CreateDirectory(Application.persistentDataPath);
    }

    void OnEnable()
    { 
        // this is just getting the UIDocument component
        var uiDocument = pausePanel.GetComponent<UIDocument>();

        // this is just getting the root UI element
        var root = uiDocument.rootVisualElement;

        var styleSheet = Resources.Load<StyleSheet>("StyleSheets/PanelAnimation");
        if (styleSheet != null && !root.styleSheets.Contains(styleSheet))
            root.styleSheets.Add(styleSheet);

        // Animate any VisualElement with the "Basicbook" class
        var basicMenu = root.Q<VisualElement>(className: "basicmainmenu");
        if (basicMenu != null)
        {
            basicMenu.SetEnabled(false); // Start disabled (opacity 0)
            basicMenu.AddToClassList("basicmainmenu"); // Ensure class is applied
            StartCoroutine(EnableWithDelay(basicMenu, 0.05f)); // Fade in
        }

        if (File.Exists(savePath))
        {
            Buttonevents(startButton, root, "Continue", OnStartButtonClick);
        }
  
        Buttonevents(newgameButton, root, "NewGame", OnNewGameButtonClick);

        Buttonevents(settingsButton, root, "Settings", OnSettingsButtonClick);

        Buttonevents(creditsButton, root, "Credits", OnCreditsButtonClick);

        Buttonevents(exitButton, root, "Exit", OnExitButtonClick);

        activatesounds = true;
        transitionAnimator = GameObject.FindGameObjectWithTag("FadeUI")?.GetComponent<Animator>();

        if (transitionAnimator == null)
            Debug.LogWarning("Fade Animator not found!");
    }

    void Buttonevents(Button button, VisualElement root, string buttonname, Action buttonevent)
    {
        // this is just getting the "next" button by name
        button = root.Q<Button>(buttonname);

        button.RegisterCallback<MouseEnterEvent>(evt => PlayHoverSound());

        // this is just adding click event listener
        button.clicked += buttonevent;
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
        AudioManager.Play(clickClip, false, AudioType.SFX, gameObject, false);
        activatesounds = true;
    }

    IEnumerator EnableWithDelay(VisualElement element, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        element.SetEnabled(true);
    }

    IEnumerator FadeOutAndDisable(VisualElement element, float duration = 0.3f,int whichmenu = 0)
    {
        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
        //Debug.Log("Settings");
        switch (whichmenu)
        {
         case 0:
                transitionAnimator.SetTrigger("End");
                yield return new WaitForSecondsRealtime(0.5f);
                Destroy(settingsPanel);
                // this is just loading the game scene
                SceneManager.LoadScene("Farm");
                break;
           case 1:
                transitionAnimator.SetTrigger("End");
                yield return new WaitForSecondsRealtime(0.5f);
                Destroy(settingsPanel);
                // this is just loading the game scene
                SceneManager.LoadScene("Farm");
                break;

            case 2:
                pausePanel.SetActive(false);
                settingsPanel.SetActive(true);
                break;
         case 3:
                pausePanel.SetActive(false);
                creditsPanel.SetActive(true);
                break;

        }
      
    }

   
  


    void OnStartButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicmenu = root.Q<VisualElement>(className: "basicmainmenu");

        if (basicmenu != null)
        {
            StartCoroutine(FadeOutAndDisable(basicmenu, 0.5f, 0));
        }
    }

    void OnNewGameButtonClick()
    {
        PlayClickSound();
        ClearSave();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicmenu = root.Q<VisualElement>(className: "basicmainmenu");

        if (basicmenu != null)
        {
            StartCoroutine(FadeOutAndDisable(basicmenu, 0.5f, 1));
        }
    }

    void OnSettingsButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicmenu = root.Q<VisualElement>(className: "basicmainmenu");

        if (basicmenu != null)
        {
            StartCoroutine(FadeOutAndDisable(basicmenu, 0.5f,2));
        }

      
    }

    void OnCreditsButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicmenu = root.Q<VisualElement>(className: "basicmainmenu");

        if (basicmenu != null)
        {
            StartCoroutine(FadeOutAndDisable(basicmenu, 0.5f, 3));
        }
    }

    // this is just exiting the the application
    void OnExitButtonClick()
    {
        PlayClickSound();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stops play mode in the editor
#else
            Application.Quit();
#endif
    }


    public void ClearSave()
    {
        try
        {
            if (File.Exists(savePath))
            {
                File.Delete(savePath);
                Debug.Log("Save file deleted.");
            }
            else
            {
                Debug.Log("No save file to clear (first launch).");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Save clear failed: " + e.Message);
        }
    }



    void DisableButtonClick()
    {
        if (startButton != null)
        {
            // this is just unsubscribing event to prevent memory leaks
            startButton.clicked -= OnStartButtonClick;
            settingsButton.clicked -= OnSettingsButtonClick;
            exitButton.clicked -= OnExitButtonClick;
        }
    }

    void OnDisable()
    {
        DisableButtonClick();
    }
}