using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class SceneController : MonoBehaviour
{
    private EventManager eventManager;
    public Vector2 respawnCoordinates = Vector2.zero;

    private SceneData currentScene;

    //scene transition items
    Animator transitionAnimator;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to event manager
        eventManager = GameController.instance.EventManager;

        // Subscribe to input events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.ChangeScene, OnChangeSceneHandler);
            eventManager.Subscribe(EventType.ReloadScene, OnReloadSceneHandler);
        }

        //transition animator setup
        transitionAnimator = GameObject.FindGameObjectWithTag("FadeUI")?.GetComponent<Animator>();

        if (transitionAnimator == null)
        {
            Debug.LogWarning("Fade Animator not found!");
        }
        else
        {
            transitionAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    void OnChangeSceneHandler(object value)
    {
        //make sure event holds right datatype
        if (value is SceneData data)
        {
            //get scene name from scenedata
            string targetSceneName = data.SceneName;
            respawnCoordinates = data.RespawnPosition;

            //An invalid scene will have a negative index so we can use that to verify the sceneName is valid
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(targetSceneName);
            if (buildIndex >= 0)
            {
                // Fire LeaveScene event BEFORE transitioning
                OnExitScene();

                //use coroutine to handle room transition
                StartCoroutine(TransitionToRoom(targetSceneName));
            }
            else
            {
                Debug.Log($"Error Loading Scene, scenename {targetSceneName} not found!");
            }
        }
    }

    /// <summary>
    /// Handles scenechange transitions such as transitionscreen, events, etc.
    /// </summary>
    /// <returns></returns>
    private IEnumerator TransitionToRoom(string targetSceneName)
    {
        if (eventManager)
        {
            transitionAnimator.SetTrigger("End");
            eventManager.Publish(EventType.LeaveScene, null);
            // Wait for fade-out duration (match your animator transition time)
            float fadeDuration = transitionAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSecondsRealtime(fadeDuration);

            transitionAnimator.SetTrigger("Start");

            // Subscribe to the inbuilt Unity Event that is raised once a scene is loaded
            // This will ensure the scene is fully loaded before trying to set it as the active scene
            SceneManager.sceneLoaded += OnSceneLoadedHandler;

            // Attempt to load the scene
            SceneManager.LoadScene(targetSceneName);
        }
    }

    /// <summary>
    /// Reloads the current scene from last entrance
    ///     Important: Yes a direct call could work, but use event in case other listeners need scenechange data
    /// </summary>
    /// <param name="obj"></param>
    private void OnReloadSceneHandler(object obj = null)
    {
        eventManager.Publish(EventType.ChangeScene, currentScene);
    }

    // Event raised when entering a scene
    private void OnEnterScene(Scene scene)
    {
        eventManager.Publish(EventType.EnterScene,currentScene);
    }

    // Event raised when leaving a scene
    private void OnExitScene()
    {
        eventManager.Publish(EventType.LeaveScene, null);

        //if current room is farm then we are leaving the farm. raise LeaveFarm event
        if (SceneManager.GetActiveScene().name == "Farm")
        {
            OnLeaveFarm();
        }
    }

#region Events
    // Event raised when leaving the farm
    private void OnLeaveFarm()
    {
        eventManager.Publish(EventType.LeaveFarm, null);
    }

    // Event raised when entering the farm
    private void OnEnterFarm()
    {
        eventManager.Publish(EventType.EnterFarm, null);
    }

    /// <summary>
    /// This responds to a UNITY event when a scene has been loaded
    ///     This is used to set the loaded scene as active when able to do so
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="mode"></param>
    public void OnSceneLoadedHandler(Scene scene, LoadSceneMode mode)
    {
        // Unsubscribe from the event (only want to subscribe when trying to load a scene)
        SceneManager.sceneLoaded -= OnSceneLoadedHandler;

        // Set the newly loaded scene as the active scene
        SceneManager.SetActiveScene(scene);

        //Update stored sceneData
        currentScene = new SceneData(SceneManager.GetActiveScene().name, respawnCoordinates);

        // Now that the scene is active, raise event knowing the player has changed rooms
        OnEnterScene(scene);

        //if current room is farm then we are leaving the farm. raise EnterFarm event
        if (SceneManager.GetActiveScene().name == "Farm")
        {
            OnEnterFarm();
        }


        GameObject player = GameObject.FindWithTag("PlayerTag");

        //if player exists and coordinates are not zero then move to coordinates
        if (player && respawnCoordinates != Vector2.zero)
        {
            //move player to coordinates
            player.transform.position = respawnCoordinates;
        }
    }
#endregion
}
