using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory
{ 
    EventManager eventManager;

    [SerializeField]
    private List<Slot> itemList; // List that holds all the items

    private int currentIndex = 0; // The array index of the current item

    public int CurrentIndex { get => currentIndex; }
    public int ItemCount { get => itemList.Count; }

    // Class constructor
    public Inventory()
    {
        // Initialization of itemList and seedList
        itemList = new List<Slot>();

        //Event Subscriptions
        ManageSubscriptions();
    }

    /// <summary>
    /// sets up all subscriptions needed
    /// </summary>
    private void ManageSubscriptions()
    {
        eventManager = GameController.instance.EventManager; // Reference to event manager for publishing events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.PlayerDeath, OnPlayerDeathHandler);
            eventManager.Subscribe(EventType.EnterFarm, OnEnterFarmHandler);
            eventManager.Subscribe(EventType.LeaveFarm, OnLeaveFarmHandler);
            eventManager.Subscribe(EventType.ResetPlayer, OnResetPlayerHandler);
            eventManager.Subscribe(EventType.PlayerAddItem, OnPlayerAddItemHandler);
        }
    }

    ~Inventory()
    {
        ManageUnsubscriptions();
    }

    private void ManageUnsubscriptions()
    {
        eventManager = GameController.instance.EventManager; // Reference to event manager for publishing events
        if (eventManager != null)
        {
            eventManager.Unsubscribe(EventType.PlayerDeath, OnPlayerDeathHandler);
            eventManager.Unsubscribe(EventType.EnterFarm, OnEnterFarmHandler);
            eventManager.Unsubscribe(EventType.LeaveFarm, OnLeaveFarmHandler);
            eventManager.Unsubscribe(EventType.ResetPlayer, OnResetPlayerHandler);
        }
    }

    #region Item Methods
    /// <summary>
    /// Checks if heal item is in inventory and changes current item index to use it
    /// </summary>
    /// <returns></returns>
    public bool FindHealItem()
    {
        int itemIndex = itemList.FindIndex(x => x.item is HealItem);

        if (itemIndex >= 0)
        {
            currentIndex = itemIndex;
            eventManager.Publish(EventType.InventoryChanged);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if ammo item is in inventory
    /// </summary>
    /// <returns></returns>
    public bool FindAmmoItem()
    {
        int itemIndex = itemList.FindIndex(x => x.item is AmmoItem);

        if (itemIndex >= 0)
        {
            currentIndex = itemIndex;
            eventManager.Publish(EventType.InventoryChanged);
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Sets item at item array at specific index to provided Item and amount
    /// </summary>
    /// <param name="itemIndex">Index of item to set</param>
    /// <param name="newType">New item type</param>
    /// <param name="newAmount">New amount of item</param>
    public void SetItem(int itemIndex, Item newItem, int newAmount)
    {
        if (itemIndex >= 0 && itemIndex < itemList.Count)
        {
            Slot currentSlot = itemList[itemIndex];
            currentSlot.item = newItem;
            currentSlot.amount = newAmount;
        }
    }

    /// <summary>
    /// Adds amount of new item if item already exists in inventory.
    /// Changes first empty item to new item if item doesn't exist.
    /// </summary>
    /// <param name="newItem">New item to add</param>
    /// <returns>Bool representing if adding item succeeded</returns>
    public void AddItem(Item newItem, int quantity = 1)
    {
        var slot = itemList.Find(x => x.item == newItem);
        if (slot != null)
        {
            slot.amount += quantity;
        }
        else
        {
            itemList.Add(new Slot(newItem) { amount = quantity });
        }

        eventManager.Publish(EventType.ItemAdded);
    }

    /// <summary>
    /// Uses item and returns it 
    /// </summary>
    /// <return>ItemType of item</return>
    public Item UseItem()
    {
        Item currentItem = null;

        //Make sure item can even be acessed
        bool canUse = canAccessInventoryIndex("UseItem", currentIndex);

        //if so then attempt to use item
        if (canUse)
        {
            currentItem = itemList[currentIndex].item;
            if (currentItem != null)
            {
                itemList[currentIndex].amount--;

                if (itemList[currentIndex].amount <= 0)
                {
                    //remove the iten entry from the list, and then check for null selection
                    itemList.RemoveAt(currentIndex);
                    CheckNullItem();
                }
                eventManager.Publish(EventType.ItemUsed);
            }
        }
        return currentItem;
    }

    /// <summary>
    /// Ensures that attempting to access a specific index in inventory is valid
    ///     automatically raises debug errors if invalid inputs received
    /// </summary>
    /// <param name="methodName"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private bool canAccessInventoryIndex(string methodName, int index)
    {
        if (itemList.Count <= 0)
        {
            //There are no items in inventory. Normal behavior, but cannot access
            return false;
        }
        // only attempt to access index if between 0 and number of items in inventory
        else if (index < 0)
        {
            Debug.Log($"Critical Error: " +
                $"Inventory.{methodName} attempted with invalid index {index}");
            return false;
        }
        else if (index >= itemList.Count)
        {
            Debug.Log($"Critical Error: " +
                $"Inventory.{methodName} attempted with index {index} " +
                $"but Inventory only has {itemList.Count - 1} items");
            return false;
        }
        return true;
    }

    /// <summary>
    /// Check if the current item is valid, if not then find the next valid item
    /// </summary>
    /// <returns></returns>
    public bool CheckNullItem()
    {

        //remove any empty entries from inventory current slot somehow empty
        //  move from end of list to start as the list size will shorten if items removed
        for (int i = itemList.Count - 1; i >= 0; i--)
        {
            if (itemList[i].item == null)
                itemList.RemoveAt(i);
        }

        //Clamp the currentIndex to ensure it is within valid range (-1 if list is empty)
        currentIndex = itemList.Count > 0 ? Mathf.Clamp(currentIndex, 0, itemList.Count - 1) : -1;

        return currentIndex >= 0;
    }

    /// <summary>
    /// Get the current item
    /// </summary>
    /// <returns>An Item object</returns>
    public Item GetCurrentItem()
    {
        //first make sure the current index references a valid item
        //   if so then return it else return null
        if (CheckNullItem())
        {
            return itemList[currentIndex].item;
        }
        return null;
    }

    /// <summary>
    /// Get the item in next slot 
    /// </summary>
    /// <returns>An Item object</returns>
    public Item GetNextItem()
    {
        //if the inventory is entry return null
        if (itemList.Count == 0) return null;

        //else use modulo arithmetic to get the next item in the list, or the first and return it
        int nextIndex = (currentIndex + 1) % itemList.Count;
        return itemList[nextIndex].item;
    }

    /// <summary>
    /// Get the item in the previous slot
    /// </summary>
    /// <returns>An Item object</returns>
    public Item GetPrevItem()
    {
        //if the inventory is entry return null
        if (itemList.Count == 0) return null;

        //else use modulo arithmetic to get the previous item in the list, or the last and return it
        int prevIndex = (currentIndex - 1 + itemList.Count) % itemList.Count;
        return itemList[prevIndex].item;
    }

    /// <summary>
    /// Get all the items in the inventory 
    /// </summary>
    /// <returns>An array of Item objects</returns>
    public Item[] GetAllItems()
    {
        Item[] returnArray = new Item[itemList.Count]; // Array to be returned
        
        // Copy all items from itemList to returnArray
        for (int i = 0; i < itemList.Count; i++)
        {
            returnArray[i] = itemList[i].item;
        }

        return returnArray;
    }

    /// <summary>
    /// Get the current item from the inventory
    /// </summary>
    /// <returns></returns>
    public int GetCurrentAmount()
    {
        return itemList.Count > 0 ? itemList[currentIndex].amount : -1;
    }

    /// <summary>
    /// Get all the amounts in the inventory
    /// </summary>
    /// <returns>An array of ints</returns>
    public int[] GetAllAmounts()
    {
        int[] returnArray = new int[itemList.Count]; // Array to be returned

        // Copy all amounts from itemList to returnArray
        for (int i = 0; i < itemList.Count; i++)
        {
            returnArray[i] = itemList[i].amount;
        }

        return returnArray;
    }

    /// <summary>
    /// Gets the type of the current item.
    /// </summary>
    /// <returns>An ItemType enum and ItemType.Empty If slot is empty</returns>
    public ItemType GetCurrentItemType()
    {
        if (CheckNullItem())
            return itemList[currentIndex].item.itemType;
        else
            return ItemType.Empty;
    }

    /// <summary>
    /// Changes the current item to the next item
    /// </summary>
    public void NextItem()
    {
        if (itemList.Count == 0) return;

        currentIndex = (currentIndex + 1) % itemList.Count;
    }

    /// <summary>
    /// Changes the current item to the previous item
    /// </summary>
    public void PrevItem()
    {
        if (itemList.Count == 0) return;

        currentIndex = (currentIndex - 1 + itemList.Count) % itemList.Count;
    }
    #endregion

    #region Event Methods
    public void OnResetPlayerHandler(object target = null)
    {
        //clear item list
        itemList.Clear();
    }
    public void OnPlayerDeathHandler(object target = null)
    {
        //clear item list
        itemList.Clear();
    }

    public void OnPlayerAddItemHandler(object target = null)
    {
        if (target is Item newItem)
        {
            AddItem(newItem);
        }
        else if (target is (Item item, int quantity))
        {
            // add item with requested quantity
            // ValueTuple (item, quantity)
            AddItem(item, quantity);
        }
    }

    /// <summary>
    /// Automatically select first ammo item upon leaving farm
    /// </summary>
    /// <param name="target"></param>
    public void OnLeaveFarmHandler(object target = null)
    {
        if (itemList.Count == 0) return;
        if (itemList[currentIndex].item is not AmmoItem)
        {
            FindAmmoItem();
        }
    }

    /// <summary>
    /// Automatically select first seed item upon entering farm
    /// </summary>
    /// <param name="target"></param>
    public void OnEnterFarmHandler(object target = null)
    {
        if (itemList.Count == 0) return;
        if (itemList[currentIndex].item is not AmmoItem)
        {
            FindHealItem();
        }
    }

    #endregion

    #region Slot Struct
    /// <summary>
    /// Struct that holds an item and its amount.
    /// </summary>
    private class Slot : IEquatable<Slot>
    {
        public int amount; // The amount of items in the slot
        public Item item; // The item in the slot

        public Slot(Item item)
        {
            amount = 1;
            this.item = item;
        }

        public bool Equals(Slot other) => other.item == item;
    }
    #endregion
}
