using UnityEngine;
using UnityEngine.UIElements;

public class DevMenuScript : MonoBehaviour
{
    private Button tutorialOnButton;
    private Button tutorialOffButton;
    private Button menuButton;
    public GameObject PausePanel;
    public GameObject DevPanel;
    private bool loaded = false;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        // this is just getting the UIDocument component
        var uiDocument = DevPanel.GetComponent<UIDocument>();


        // this is just getting the root UI element
        var root = uiDocument.rootVisualElement;

        // this is just getting the "Start" button by name
        tutorialOnButton = root.Q<Button>("TutorialOn");

        // this is just adding click event listener
        tutorialOnButton.clicked += OnTutorialOn;

        // this is just getting the "exit" button by name
        tutorialOffButton = root.Q<Button>("TutorialOff");

        // this is just adding click event listener
        tutorialOffButton.clicked += OnTutorialOff;

        // this is just getting the "Start" button by name
        menuButton = root.Q<Button>("MenuButton");

        // this is just adding click event listener
        menuButton.clicked += OnMenuButtonClick;

        if (loaded == false)
        {
            loaded = true;
            DevPanel.SetActive(false);
        }
    }

    // this is just an event to activate tutorial
    void OnTutorialOn()
    {
        // Publish toggle event
        GameController.instance.EventManager.Publish(EventType.TutorialToggle,true);
    }

    // this is just an event to deactivate tutorial
    void OnTutorialOff()
    {
        // Publish toggle event
        GameController.instance.EventManager.Publish(EventType.TutorialToggle,false);
    }

    // this is just activating the pause panel and deactivating the seeds panel 
    void OnMenuButtonClick()
    {

        Debug.Log("Seeds");
        DevPanel.SetActive(false);
        PausePanel.SetActive(true);
    }

    void OnDisable()
    {
        // Unsubscribe event to prevent memory leaks
        tutorialOnButton.clicked -= OnTutorialOn;
        tutorialOffButton.clicked -= OnTutorialOff;
        menuButton.clicked -= OnMenuButtonClick;
    }
}