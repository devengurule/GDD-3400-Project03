using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class PausemenuScript : MonoBehaviour
{
    private Button resumeButton;
    private Button exitButton;
    private Button settingsButton;
    private Button seedButton;
    private Button enemyButton;
    private Button unstuckButton;
    private Button devMenuButton;
    public GameObject pausePanel;
    public GameObject seedsPanel;
    public GameObject enemyPanel;
    public GameObject devPanel;
    public GameObject settingsPanel;
    [SerializeField] private EventManager eventManager;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName hoverClip;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName clickClip;
    private bool activatesounds;
    Animator transitionAnimator;
    [SerializeField] private string targetSceneName;
    [SerializeField] private float spawnX;
    [SerializeField] private float spawnY;


    private void Start()
    {
        eventManager = GameController.instance.EventManager;
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
        var basicBook = root.Q<VisualElement>(className: "Basicbook");
        if (basicBook != null)
        {
            basicBook.SetEnabled(false); // Start disabled (opacity 0)
            basicBook.AddToClassList("Basicbook"); // Ensure class is applied
            StartCoroutine(EnableWithDelay(basicBook, 0.05f)); // Fade in
        }

        Buttonevents(resumeButton, root, "Resume", OnStartButtonClick);

        Buttonevents(exitButton, root, "Exit", OnExitButtonClick);

        Buttonevents(settingsButton, root, "Settings", OnSettingsButtonClick);

        Buttonevents(seedButton, root, "Seeds", OnSeedsButtonClick);

        Buttonevents(enemyButton, root, "Enemies", OnEnemyButtonClick);

        Buttonevents(devMenuButton, root, "DevMenu", OnDevButtonClick);

        transitionAnimator = GameObject.FindGameObjectWithTag("FadeUI")?.GetComponent<Animator>();

        if (transitionAnimator == null)
            Debug.LogWarning("Fade Animator not found!");
    }

    #region Event/Button Handlers

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
        if(activatesounds)
        {
            AudioManager.Play(hoverClip, false, AudioType.SFX, gameObject, false);
        }
        
    }
    void PlayClickSound()
    {
        activatesounds = false;
        AudioManager.Play(clickClip, false, AudioType.SFX, gameObject, false);
    }


    // this is just deactivating the pause panel and setting the time scale so the player can move
    void OnStartButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, null, true));
        }
    }

    // this is just destroying all objects and moving to the main menu scene
    void OnExitButtonClick()
    {

        
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(Enablefadeblack(basicBook));
        }

    }

    // this is just deactivating the pause panel and setting the time scale so the player can move
    void OnSettingsButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
           
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, settingsPanel,false));
        }
    }

    // this is just activating the seed panel and deactivating the pause panel 
    void OnSeedsButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, seedsPanel));
        }
    }
    void OnEnemyButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, enemyPanel));
        }
    }

    // this is just activating the dev panel and deactivating the pause panel 
    void OnDevButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutfarm(basicBook, 0.5f, devPanel));
        }
    }





    // Method that runs when the Stuck button is clicked
    void OnUnstuckButtonClick()
    {
        pausePanel.SetActive(false);

        // Get ref to player
        GameObject player = GameObject.FindWithTag("PlayerTag");

        // Get ref to scene controller
        SceneController sceneController = GameController.instance.GetComponent<SceneController>();

        // If there are respawn coordinates (there should be with each scene)
        if (sceneController.respawnCoordinates != Vector2.zero)
        {
            // Move the player to the respawn coordinates (usually a door)
            player.transform.position = sceneController.respawnCoordinates;
        }
        else
        {
            // Do nothing
        }

        // Probably going to take the respawn coords from the scenedata
        Time.timeScale = 1;
    }

    IEnumerator Enablefadeblack(VisualElement element)
    {
   
        Time.timeScale = 1;
        eventManager.Publish(EventType.SaveGame);
        element.SetEnabled(false);
        transitionAnimator.SetTrigger("End");
        yield return new WaitForSecondsRealtime(2.0f);
        
        
        foreach (GameObject o in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            Destroy(o);

        }
        SceneManager.LoadScene("Mainmenu");
       

    }

    IEnumerator EnableWithDelay(VisualElement element, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        element.SetEnabled(true);
        activatesounds = true;
    }

    IEnumerator FadeOutAndDisable(VisualElement element, float duration = 0.3f, GameObject currentPanel = null, bool exit = false)
    {
        
        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
        if (exit)
        {
            eventManager.Publish(EventType.Pause);

        }
        else
        {
            pausePanel.SetActive(false);
            currentPanel.SetActive(true);
        }

       
    }

    IEnumerator FadeOutfarm(VisualElement element, float duration = 0.3f, GameObject currentPanel = null)
    {

        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
            eventManager.Publish(EventType.Pause);
            eventManager.Publish(EventType.ChangeScene, GetSceneData());
    }

    public SceneData GetSceneData()
    {
        return new SceneData(targetSceneName, new Vector2(spawnX, spawnY));
    }

    void DisableButtonClick()
    {
        if (resumeButton != null)
        {
            // this is just unsubscribing event to prevent memory leaks
            resumeButton.clicked -= OnStartButtonClick;
            exitButton.clicked -= OnExitButtonClick;
            settingsButton.clicked -= OnSettingsButtonClick;
            seedButton.clicked -= OnSeedsButtonClick;
            enemyButton.clicked -= OnEnemyButtonClick;
            devMenuButton.clicked -= OnDevButtonClick;
        }
    }

    void OnDisable()
    {
        DisableButtonClick();
    }
#endregion
}