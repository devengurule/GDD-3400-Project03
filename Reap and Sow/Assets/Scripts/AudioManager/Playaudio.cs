using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Playaudio : MonoBehaviour
{
    #region Fields
    [System.Serializable]
    public class SceneAudioPair
    {
        public string sceneName;
        public AudioClipName audioClip;
    }

    [SerializeField] private List<SceneAudioPair> sceneAudioMappings;
    [SerializeField] private AudioClipName startClip;

    private AudioClipName currentClip;
    private Dictionary<string, AudioClipName> sceneAudioMap;
    private EventManager eventManager;
    #endregion

    #region Unity Methods
    /// <summary>
    /// This is just initilizing all variables and subscribing events
    /// </summary>
    void Start()
    {
        // this is initializing the audiomap
        sceneAudioMap = new Dictionary<string, AudioClipName>();
        foreach (var pair in sceneAudioMappings)
        {
            if (!sceneAudioMap.ContainsKey(pair.sceneName))
                sceneAudioMap.Add(pair.sceneName, pair.audioClip);
        }

        // this is setting the start farm clip by default
        currentClip = startClip;
        AudioManager.Play(currentClip, loop: true, AudioType.Music, gameObject, false);

        // this is just the event setup
        eventManager = GameController.instance?.EventManager;

        if (eventManager == null)
        {
            Debug.Log("Playaudio: EventManager is NULL");
            return;
        }

        eventManager.Subscribe(EventType.EnterScene, SwitchSounds);
    }


    void OnDestroy()
    {
        if (eventManager != null) 
            eventManager.Unsubscribe(EventType.EnterScene, SwitchSounds);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// this is the event to swap the sounds based on what level you are currently in.
    /// </summary>
    /// <param name="obj"></param>
    private void SwitchSounds(object obj)
    {
        Scene scene = SceneManager.GetActiveScene();
        string currentScene = scene.name;

        // We are checking what the current scene is and if there is a new clip connected to that scene we change the clip
        if (sceneAudioMap.TryGetValue(currentScene, out AudioClipName newClip))
        {
            if (currentClip != newClip)
            {
                AudioManager.StopLoopedSoundByName(currentClip);
                currentClip = newClip;
                AudioManager.Play(currentClip, loop: true, AudioType.Music, gameObject, false);
            }
        }
        else
        {
            Debug.LogWarning($"No Music for scene: {currentScene}");
        }
    }
    #endregion

}
