using UnityEngine;
using UnityEngine.Tilemaps;


public enum FloorTypes
{
    Grass,
    Wood,
}




[CreateAssetMenu]
public class Tiledata : ScriptableObject
{
    public TileBase[] tiles;
    public AudioClip[] clips;
    public FloorTypes floorType;  
}
