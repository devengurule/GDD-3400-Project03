using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Holds useful testing parameters for room tests
/// </summary>
public class TestRoomManager : MonoBehaviour
{
    //holds an item and a quantity for reference from itemList (basically a serialized tuple)
    [Serializable]
    private struct ItemEntry
    {
        public Item item;
        public float quantity ;
    }

    [SerializeField]
    private List<ItemEntry> itemList = new List<ItemEntry>();

    // Getter returns a list of tuples
    public List<(Item, float)> ItemListAsTuples =>
        itemList.Select(entry => (entry.item, entry.quantity)).ToList();
}
