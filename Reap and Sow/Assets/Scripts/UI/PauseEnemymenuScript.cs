using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;

public class PauseEnemymenuScript : MonoBehaviour
{
    #region Variables
    private Button previousButton;
    private Button nextButton;
    private Button seedButton;
    private Button menuButton;
    private VisualElement leftImg;
    private VisualElement rightImg;
    private Label leftTitle;
    private Label rightTitle;
    private Label leftBottomText;
    private Label rightBottomText;
    public GameObject pausePanel;
    public GameObject seedsPanel;
    public GameObject enemyPanel;
    public Sprite[] spriteArray;
    public string[] titleTextArray;
    public string[] bottomTextArray;
    private int[] enemyArray;
    private int leftEnemy;
    private int rightEnemy;
    private bool loaded = false;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName hoverClip;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName clickClip;
    private bool activatesounds;
    #endregion

    void Awake()
    {
        // this is just setting the basic values
        enemyArray = new int [spriteArray.Length - 1];
        leftEnemy = enemyArray[0];
        rightEnemy = enemyArray[1];
    }

    void OnEnable()
    {
        // this is just getting the UIDocument component
        var uiDocument = enemyPanel.GetComponent<UIDocument>();

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

        // this is just getting the "Lplantimg" visualelement by name
        leftImg = root.Q<VisualElement>("LPlantIMG");

        // this is just getting the "Rplantimg" visualelement by name
        rightImg = root.Q<VisualElement>("RPlantIMG");

        // this is just getting the "Ltoptext" label by name
        leftTitle = root.Q<Label>("LTopText");

        // this is just getting the "Rtoptext" label by name
        rightTitle = root.Q<Label>("RTopText");

        // this is just getting the "Lbottomtext" label by name
        leftBottomText = root.Q<Label>("LBottomText");

        // this is just getting the "Rbottomtext" label by name
        rightBottomText = root.Q<Label>("RBottomText");

        Buttonevents(menuButton, root, "Menu", OnMenuButtonClick);

        Buttonevents(seedButton, root, "Seeds", OnSeedButtonClick);

        Buttonevents(previousButton, root, "Previous", OnPrevButtonClick);

        Buttonevents(nextButton, root, "Next", OnNextButtonClick);

        // this is just setting the basic starting values for the left and right seed
        leftEnemy = 0;
        rightEnemy = 1;
        SetArrays();
        if (loaded == false && gameObject.activeInHierarchy == true)
        {
            loaded = true;

            if (gameObject.activeInHierarchy == true)
            {
                gameObject.SetActive(false);
            }
        }
    }

    #region Event/Button Handlers

    void Buttonevents (Button button, VisualElement root, string buttonname, Action buttonevent)
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
        activatesounds = false;
        AudioManager.Play(clickClip, false, AudioType.SFX, gameObject, false);
    }

    // this is just subtracting the left and right seed sprites based on the values in the ints
    void OnPrevButtonClick()
    {
        PlayClickSound();
        leftEnemy -= 2;
        rightEnemy -= 2;
       if (leftEnemy < 0)
        {
            leftEnemy = 0;
            rightEnemy = 1;
        }
        SetArrays();
    }

    // this is just adding the left and right seed sprites based on the values in the ints
    void OnNextButtonClick()
    {
        PlayClickSound();
        leftEnemy += 2;
        rightEnemy += 2;
        if (rightEnemy > enemyArray.Length)
        {
            leftEnemy = enemyArray.Length - 1;
            rightEnemy = enemyArray.Length;
        }
        SetArrays();
    }

    // this is just activating the pause panel and deactivating the seeds panel 
    void OnMenuButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, pausePanel));
        }
    }

    void OnSeedButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f, seedsPanel));
        }
        
    }

    void DisableButtonClick()
    {
        if (menuButton != null)
        {
            // this is just unsubscribing event to prevent memory leaks
            menuButton.clicked -= OnMenuButtonClick;
            nextButton.clicked -= OnNextButtonClick;
            previousButton.clicked -= OnPrevButtonClick;
            seedButton.clicked -= OnSeedButtonClick;
        }
    }

    void OnDisable()
    {
        DisableButtonClick();
    }
    #endregion

    // this is just setting the arrays based on certain int values
    void SetArrays()
    {
        leftImg.style.backgroundImage = new StyleBackground(spriteArray[leftEnemy]);
        rightImg.style.backgroundImage = new StyleBackground(spriteArray[rightEnemy]);
        leftTitle.text = (titleTextArray[leftEnemy]);
        rightTitle.text = (titleTextArray[rightEnemy]);
        leftBottomText.text = (bottomTextArray[leftEnemy]);
        rightBottomText.text = (bottomTextArray[rightEnemy]);
    }

    IEnumerator EnableWithDelay(VisualElement element, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        element.SetEnabled(true);
        activatesounds = true;
    }

    IEnumerator FadeOutAndDisable(VisualElement element, float duration = 0.3f, GameObject currentPanel = null)
    {
        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
        enemyPanel.SetActive(false);
        currentPanel.SetActive(true);
    }
}