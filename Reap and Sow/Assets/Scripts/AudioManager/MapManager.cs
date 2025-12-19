using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap map;

    [SerializeField] private List <Tiledata> tileData;

    private Dictionary<TileBase,Tiledata> dataFromTiles;

    private void awake()
    {
        dataFromTiles = new Dictionary<TileBase, Tiledata>();

        foreach (var tileD in tileData)
        {
            foreach (var tile in tileD.tiles) 
            { 
                dataFromTiles.Add (tile, tileD);
            }
        }
    }

    public AudioClip GetCurrentFloorClip(Vector2 worldPosition)
    {
        Vector3Int gridPosition = map.WorldToCell(worldPosition);
        TileBase tile = map.GetTile(gridPosition);

        int index = Random.Range(0, dataFromTiles[tile].clips.Length);
        AudioClip currentFloorClip = dataFromTiles[tile].clips[index];

        return currentFloorClip;
    }


}
