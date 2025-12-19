using UnityEngine;

public class Plantandharvestscript : MonoBehaviour
{
    [Header("Event Manager")]
    [SerializeField] private EventManager eventManager;

    [Header("Plot Settings")]
    [SerializeField] private PlotScript plotScript;

    [Header("Plant Types")]
    [SerializeField] private PlantType[] availablePlantTypes;

    [SerializeField] private Plant plant;

    string seedTutorial2 = "plantingtutorial2";

    #region Unity Methods
    private void Start()
    {
        // This just Subscribing to events
        eventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
        eventManager.Subscribe(EventType.LeaveFarm, OnExitFarmHandler);
    }

    private void Update()
    {
        // This is just checking for button input to trigger different actions

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            PlantSeed(plant);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnEnterSceneHandler(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            OnExitFarmHandler(null);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            HarvestPlant();
        }
    }

    /// <summary>
    /// OnDestroy is called when the GameObject or component it is attached to is destroyed.
    /// </summary>
    void OnDestroy()
    {
        eventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler);
        eventManager.Unsubscribe(EventType.LeaveFarm, OnExitFarmHandler);
    }
    #endregion

    // This is where you plant a seed in the plot
    private void PlantSeed(Plant plant)
    {
        if (plotScript != null)
        {
            plotScript.PlantSeed(plant);
            Debug.Log($"Planted a {plant} seed.");
            HUDTextManager.Instance.StartDialogue(seedTutorial2);
        }
    }

    // This is where you would harvest the plant if it's ready
    private void HarvestPlant()
    {
        if (plotScript != null)
        {
            Item harvestedItem = plotScript.HarvestPlant();
            if (harvestedItem != null)
            {
            }
        }
    }

    #region events
    // This is where you simulate entering the farm and resume the timer
    private void OnEnterSceneHandler(object obj)
    {
        // Checks if the current active scene is the farm scene
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Farm")
        {
            // If plot script exists
            if (plotScript != null)
            {
                plotScript.OnEnterSceneHandler(obj);  // Resumes the plot's timer (if paused)
            }
        }
    }

    // This is where you simulate exiting the farm and pause the timer
    private void OnExitFarmHandler(object obj)
    {
        // If plot script exists
        if (plotScript != null)
        {
            plotScript.OnExitFarmHandler(obj);  // Pauses the plot's timer
        }
    }
    #endregion
}
