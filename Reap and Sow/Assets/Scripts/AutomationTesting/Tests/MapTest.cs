using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


/// <summary>
/// Maps out entire playable game map and connections.
/// Determines what scenes are actually reachable, and creates a usable map for other tests
/// Makes heavy usage of RoomMap object
/// </summary>
public class MapTest : AbstractTest
{
    //Override test values
    //  testtype to have correct name
    //  godMode to prevent accidental deaths
    //  dont disable scenechangers (so they can be mapped)
    //  disable enemies, to prevent accidental deaths
    public override string TestType => "Map";
    protected override string LogFileName => "Map Test.csv";
    protected override bool PlayerGodMode => true;          // If true, set "god mode" flag on the player
    protected override bool DisableSceneChangers => false;
    protected override string[] TagsToDisable => new[] { enemyTag }; // Default excludes EnemyTag so existing enemies can remain


    private RoomMap map;

    public RoomMap Map { get => map; set => map = value; }
    protected override IEnumerator RunTest()
    {
        //ensure map is initialized
        if (map == null)
            map = new RoomMap();

        //Setup a dictionary to store scene data, and a queue of rooms to check
        Dictionary<string, SceneData> sceneDictionary = new Dictionary<string, SceneData>();
        Queue<string> roomsToMap = new Queue<string>();

        //get current room name for start of the loop
        string currentRoom = SceneManager.GetActiveScene().name; 
        //if possible use player position else use gamecontrollers position
        Vector2 roomPosition = (player != null) ? player.transform.position : transform.position;
        sceneDictionary.Add(currentRoom,new SceneData(currentRoom, roomPosition));

        //Add current room to roomsToMap
        roomsToMap.Enqueue(currentRoom);

        //Loop until no more rooms that can be mapped
        while (roomsToMap.Count > 0)
        {

            //Update currentRoom to the next mappable room
            currentRoom = roomsToMap.Dequeue();

            //Load the new currentRoom
            SceneData sceneData = sceneDictionary[currentRoom];

            yield return map.LoadRoom(sceneData); //use the static function for RoomMap to load the new room

            //Delete enemies from current room
            DisableObjectsByTag();

            //get exits by calling GetSceneData method of all scenechangers in current scene.
            SceneChanger[] exits = GameObject.FindObjectsByType<SceneChanger>(FindObjectsSortMode.None);

            //for each exit
            foreach (SceneChanger exit in exits)
            {
                SceneData exitData = exit.GetSceneData();

                //get rooms name
                string targetRoom = exitData.SceneName;

                //Add the returned SceneData items to roomsToMap and sceneDictionaryt
                if (!sceneDictionary.ContainsKey(targetRoom))
                {
                    sceneDictionary.Add(targetRoom, exitData);
                    roomsToMap.Enqueue(targetRoom);
                }

                //Add connections to Map
                map.AddConnection(currentRoom, targetRoom, exitData.RespawnPosition);
            }

            //Allow any code to continue before trying to change scenes again
            yield return null;
        }

        //Clear dictionary
        sceneDictionary.Clear();
    }
    protected override IEnumerator StopTest()
    {
        //Stop all coroutines
        StopAllCoroutines();

        //Edit: add logging maybe, I'm not actually sure if its useful yet
        yield return null;
    }
}
