using UnityEngine;

/// <summary>
/// Represents an item ingame
/// </summary>
[CreateAssetMenu(fileName = "Item", menuName = "Item")]
public class Item : ScriptableObject
{
    public ItemType itemType; // The item's type (Empty, seed, equipment, and other).
    public Sprite itemSprite; // The sprite the item uses as a pickup
}