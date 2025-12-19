
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class CreditsMenu : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName hoverClip;
    [SerializeField]
    [Tooltip("Setting audio clips ")]
    private AudioClipName clickClip;
    private bool activatesounds;
    public GameObject pausePanel;
    private bool loaded = false;
    private UnityEngine.UIElements.Button CreditExit;

    void OnEnable()
    {
        if (loaded == false && gameObject.activeInHierarchy == true)
        {
            loaded = true;
            //DontDestroyOnLoad(gameObject);
         

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
            var basicBook = root.Q<VisualElement>(className: "CreditsRoll");
            if (basicBook != null)
            {
                basicBook.SetEnabled(false); // Start disabled (opacity 0)
                basicBook.AddToClassList("CreditsRoll"); // Ensure class is applied
                StartCoroutine(EnableWithDelay(basicBook, 0.05f, 22.0f)); // Fade in
            }

            Buttonevents(CreditExit, root, "ExitCredits", OnSkipButtonClick);



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





    IEnumerator EnableWithDelay(VisualElement element, float delay, float duration)
    {
        yield return new WaitForSecondsRealtime(delay);
        element.SetEnabled(true);
        yield return new WaitForSecondsRealtime(duration);
        pausePanel.SetActive(true);
        gameObject.SetActive(false);
    }

    IEnumerator FadeOutAndDisable(VisualElement element, float duration = 0.3f)
    {
        element.SetEnabled(false);
        yield return new WaitForSecondsRealtime(duration);
        pausePanel.SetActive(true);
        gameObject.SetActive(false);
    }

    // this is just destroying all objects and moving to the main menu scene
    void OnMenuButtonClick()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "Basicbook");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f));
        }

        Debug.Log("Menu clicked");

    }

    void OnSkipButtonClick()
    {
        PlayClickSound();
        var root = GetComponent<UIDocument>().rootVisualElement;
        var basicBook = root.Q<VisualElement>(className: "CreditsRoll");

        if (basicBook != null)
        {
            StartCoroutine(FadeOutAndDisable(basicBook, 0.5f));
        }

        Debug.Log("Menu clicked");
    }


}