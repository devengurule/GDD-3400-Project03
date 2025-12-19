using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MonsterDeathTracker : MonoBehaviour
{
    [SerializeField] private float defaultRespawnTime = 30f;

    private string currentScenename = "";
    private EventManager eventManager;

    //create a new list to store the dead enemies and the rooms they exist in
    private Dictionary<string, ComplexTimer> respawnTimers;

    //Dictionary holding all non-respawned enemies key = roomname, data = (List of uniqueIDs)
    private Dictionary<string, List<string>> corpseDict;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to event manager
        eventManager = GameController.instance.EventManager;

        // Subscribe to input events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.ChangeScene, OnChangeSceneHandler);
            eventManager.Subscribe(EventType.ClearDeathTimer, OnClearDeathTimerHandler);
            eventManager.Subscribe(EventType.PlayerDeath, OnGameOverHandler);
        }

        currentScenename = SceneManager.GetActiveScene().name;
        corpseDict = new Dictionary<string, List<string>>();
        respawnTimers = new Dictionary<string, ComplexTimer>();
    }

    void OnGameOverHandler(object value)
    {
        //reset rooms
        corpseDict.Clear();
    }
        public void OnChangeSceneHandler(object value)
    {
        //make sure event holds right datatype
        if (value is SceneData sceneData)
        {
            //Update current scene name and reset sceneID
            currentScenename = sceneData.SceneName;
        }
    }
    

    public void OnClearDeathTimerHandler(object value)
    {
        ClearDeaths();
    }

    //This method gets called from other scripts in order to
    //add a new enemy instance to the list so that they remain
    //dead for a certain amount of time
    public void AddCorpse(string room, string objectID)
    {
        //If room is not in the corpseDict already then add it, then add the enemyID for the corpse
        if (!corpseDict.ContainsKey(room))
        {
            corpseDict.Add(room, new List<string>() { objectID });
        }
        else
        {
            corpseDict[room].Add(objectID);
        }


        //Add respawn Timer (if not already added)
        if (!respawnTimers.ContainsKey(room))
        {
            ComplexTimer respawnTimer = gameObject.AddComponent<ComplexTimer>();
            respawnTimer.AddFinishedListener(Respawn);
            respawnTimer.TimerName = room;
            respawnTimer.Duration = defaultRespawnTime;
            respawnTimer.Run();
            respawnTimers.Add(room, respawnTimer);
        }
    }

    public void Respawn(Timer respawnTimer)
    {
        if (respawnTimer != null)
        {
            //find out what timer went off
            string roomName = respawnTimer.TimerName;

            //remove all enemies for that room from respawn table (so they respawn next time you enter the room)
            if (corpseDict.ContainsKey(roomName))
            {
                corpseDict.Remove(roomName);
            }


            if (respawnTimers.ContainsKey(roomName))
            {
                Destroy(respawnTimer);  // remove the Timer component to clean up
                respawnTimers.Remove(roomName); //remove timer dictionary entry
            }
        }
    }

    //This method gets called in order to check if an enemy has been killed.
    //If they are dead then they don't respawn when the player re-enters the room
    public bool IsDead(string room, string objectID)
    {
        if (corpseDict != null && corpseDict.ContainsKey(room))
        {
            return corpseDict[room].Contains(objectID);
        }
        return false;
    }

    public void ClearDeaths()
    {
        //clear dictionaries
        corpseDict.Clear();
    }

    //Any script can call this method to get a new unique identifier assigned to it
    public string GetUniqueID(string name = "")
    {
        string UniqueID = currentScenename + "_" + name;

        return UniqueID;
    }

    private void OnDestroy()
    {
        eventManager?.Unsubscribe(EventType.ChangeScene, OnChangeSceneHandler);
    }
}
