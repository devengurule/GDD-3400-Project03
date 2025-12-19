using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomMap
{
    //Create a dictionary to hold our nodes
    Dictionary<string, RoomNode> nodes = new Dictionary<string, RoomNode>();
    List<SceneData> entranceList = new List<SceneData>();
    private bool sceneLoaded = false; //used to wait until the scene is loaded before continuing

    RoomNode currentRoom;
    SceneData pendingScene;
    //Add a new roomNode, return true if it is newly added false if not
    private RoomNode AddNode(string roomID)
    {
        //if Node is not already tracked, then add it
        if (!nodes.ContainsKey(roomID))
            nodes[roomID] = new RoomNode(roomID);
        return nodes[roomID];
    }

    public void UpdateCurrentRoom(string roomID)
    {
        //Ensure current room is listed as a room and get reference to it
        currentRoom = AddNode(roomID);
    }

    /// <summary>
    /// Loads a specific room
    /// </summary>
    public IEnumerator LoadRoom(SceneData sceneData)
    {
        int i = 1;

        if (sceneData != null)
        {
            //If scene cant be loaded its sceneIndex will be -1
            int buildIndex = SceneUtility.GetBuildIndexByScenePath(sceneData.SceneName);
            if (buildIndex >= 0)
            {
                //IF map has the roomID listed, and eventManager exist (can request scenechange)
                EventManager eventManager = GameController.instance.EventManager;

                //Publish an event to load next room
                GameController.instance.EventManager.Publish(EventType.ChangeScene, sceneData);

                //wait until next room loads before proceeding by subscribing to sceneLoaded event
                sceneLoaded = false;
                pendingScene = sceneData;
                GameController.instance.EventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler); //in case somehow subscribed still
                GameController.instance.EventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
                yield return new WaitUntil(() => sceneLoaded);
            }
            else
            {
                Debug.Log($"Error Loading Scene, scenename {sceneData.SceneName} not found!");
            }
        }
        else
        {
            Debug.Log($"Error Loading Scene, sceneData is null!");
        }
    }

    /// <summary>
    /// Returns the current room name
    /// </summary>
    /// <returns></returns>
    public string GetCurrentRoom()
    {
        if (currentRoom == null)
            return "";
        return currentRoom.roomID;
    }

    /// <summary>
    /// Returns a list of all room IDs in this map
    /// </summary>
    public List<string> GetAllRoomIDs()
    {
        return new List<string>(nodes.Keys);
    }

    /// <summary>
    /// Returns a list of all doorways from all rooms as sceneData
    /// </summary>
    public List<SceneData> GetAllEntranceData()
    {
        return entranceList;
    }

    /// <summary>
    /// Adds a connection from currentNode to destination
    /// </summary>
    /// <param name="targetRoom">room being entered</param>
    public void AddConnection(string originRoom, string targetRoom, Vector2 spawnPoint)
    {
        //ensure both nodes are already added
        AddNode(originRoom);
        AddNode(targetRoom);

        //Add exit node connection to originNode
        RoomNode originNode = nodes[originRoom];
        originNode.AddExit(targetRoom, spawnPoint);

        //Create new sceneData, update the connections
        SceneData data = new SceneData(targetRoom, spawnPoint);

        //Only add connection if not already added
        if (!entranceList.Contains(data))
        {
            entranceList.Add(data);
        }
    }

    /// <summary>
    /// RoomNode used to store details for each room
    /// </summary>
    class RoomNode
    {
        public string roomID;
        Dictionary<string, SceneData> exitDictionary = new Dictionary<string, SceneData>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="id"></param>
        public RoomNode(string id)
        {
            roomID = id;
        }

        /// <summary>
        /// Adds a new exit connection to another room
        /// </summary>
        /// <param name="target"></param>
        public void AddExit(string targetID, Vector2 spawnPoint)
        {

            //Create new sceneData, update the connections
            SceneData data = new SceneData(targetID, spawnPoint);

            //Only add connection if not already added
            if (!exitDictionary.ContainsKey(targetID))
            {
                exitDictionary[targetID] = data;
            }
        }
    }

    public void OnEnterSceneHandler(object value = null)
    {
        if (value is SceneData data)
        {
            if (data.Equals(pendingScene))
            {

                sceneLoaded = true;
                pendingScene = null; //nullify to avoid weird behavior if the same scene reloads twice in a row

                //unsubscribe from event
                GameController.instance.EventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler);
            }
        }
    }
}

