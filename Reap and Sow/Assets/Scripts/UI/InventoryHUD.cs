using TMPro;
using UnityEngine;
using UnityEngine.UI;
//using static UnityEditor.Progress;  //commented out for building

public class InventoryHUD : MonoBehaviour
{
    private EventManager eventManager; 
    private Animator anim;
    private Inventory inventory;
    private Image childImage;
    private Image childNextImage;
    private Image childPrevImage;
    private TextMeshProUGUI childText;

    #region Unity Methods
    // Awake is called before Start
    void Start()
    {
        // Gets the animator and the event manager
        anim = GetComponent<Animator>();
        eventManager = GameController.instance.EventManager;

        // Get Inventory reference and update inventory
        GetInventory();

        // Get all information needed for child objects
        GetChildren();

        // Event subscribing
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.NextItem, OnNextHandler);
            eventManager.Subscribe(EventType.PrevItem, OnPrevHandler);
            eventManager.Subscribe(EventType.ItemAdded, OnItemAddedHandler);
            eventManager.Subscribe(EventType.ItemUsed, OnItemUsedHandler);
            eventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
            eventManager.Subscribe(EventType.PlayerDeath, OnGameOverHandler);
            eventManager.Subscribe(EventType.ResetPlayer, OnResetPlayerHandler);
        }
    }

    void OnDestroy()
    {
        // Event Unsubscribing
        if (eventManager != null)
        {
            eventManager.Unsubscribe(EventType.NextItem, OnNextHandler);
            eventManager.Unsubscribe(EventType.PrevItem, OnPrevHandler);
            eventManager.Unsubscribe(EventType.ItemAdded, OnItemAddedHandler);
            eventManager.Unsubscribe(EventType.ItemUsed, OnItemUsedHandler);
            eventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler);
            eventManager.Unsubscribe(EventType.PlayerDeath, OnGameOverHandler);
            eventManager.Unsubscribe(EventType.ResetPlayer, OnResetPlayerHandler);
        }
    }
    #endregion

    #region Private Methods
    // Decide what inventory to use based on inventory type
    private void GetInventory()
    {
        inventory = GameController.instance.ItemInventory;
    }

    /// <summary>
    /// Updates the currently displayed item to the currently selected item in the inventory.
    /// </summary>
    private void UpdateItems()
    {
        if (inventory == null)
        {
            GetInventory();
        }
        // Gets the current item and the current amount
        Item currentItem = inventory?.GetCurrentItem();
        int currentAmount = inventory.GetCurrentAmount();
        int itemCount = inventory.ItemCount;
        int currentIndex = inventory.CurrentIndex;

        // Get the next and previous items
        Item nextItem = inventory?.GetNextItem();
        Item prevItem = inventory?.GetPrevItem();

        //Initialize sprites
        //  Only assign nextSprite and prevSprite if not duplicates
        Sprite currentSprite = currentItem?.itemSprite;
        Sprite nextSprite = null;
        Sprite prevSprite = null;

        //If inventory has more than 3 entries, can show the same item on left or right without repeating
        if (itemCount > 2)
        {
            nextSprite = nextItem?.itemSprite;
            prevSprite = prevItem?.itemSprite;
        }
        // Else if inventory has 2 entries, then only display 2 depending on their position in inventory
        else if (itemCount == 2)
        { 
            //if current item is the first item, show second item
            if (currentIndex == 0)
            {
                nextSprite = nextItem?.itemSprite;
            }
            else //current item is the second item show first item
            {
                prevSprite = prevItem?.itemSprite;
            }
        }

        // Assign the images sprites to the item sprites
        childImage.sprite = currentSprite;
        childNextImage.sprite = nextSprite;
        childPrevImage.sprite = prevSprite;

        if (currentAmount != -1)
        {
            childText.SetText(currentAmount.ToString());
        }
        else
        {
            childText.SetText("");
        }
    }

    /// <summary>
    /// Gets the child images and text objects
    /// </summary>
    private void GetChildren()
    {
        // Gets the child image for the current item
        GameObject child = this.transform.GetChild(0).gameObject;
        childImage = child.GetComponent<Image>();

        // Gets the child image for the next item
        child = this.transform.GetChild(1).gameObject;
        childNextImage = child.GetComponent<Image>();

        // Gets the child image for the previous item
        child = this.transform.GetChild(2).gameObject;
        childPrevImage = child.GetComponent<Image>();

        // Gets the child text
        child = this.transform.GetChild(3).gameObject;
        childText = child.GetComponent<TextMeshProUGUI>();
    }
    #endregion

    #region Event Methods
    /// <summary>
    /// Triggers upon Next event
    /// </summary>
    /// <param name="target"></param>
    public void OnNextHandler(object target = null)
    {
        anim.SetTrigger("NextTrig");
    }

    /// <summary>
    /// Triggers upon the end of the next animation
    /// </summary>
    public void OnNextEnd()
    {
        AudioManager.Play(AudioClipName.sfx_Ui_switch, loop: false);
        UpdateItems();
    }

    /// <summary>
    /// Triggers upon on Prev event
    /// </summary>
    /// <param name="target"></param>
    public void OnPrevHandler(object target = null)
    {
        anim.SetTrigger("PrevTrig");
    }
    
    /// <summary>
    /// Triggers upon the end of the Prev animation
    /// </summary>
    public void OnPrevEnd()
    {
        AudioManager.Play(AudioClipName.sfx_Ui_switch, loop: false);
        UpdateItems();
    }

    /// <summary>
    /// Triggers when an item is picked up by the player
    /// </summary>
    public void OnItemAddedHandler(object target = null)
    {
        AudioManager.Play(AudioClipName.sfx_Ui_grab, loop: false);
        UpdateItems();
    }

    /// <summary>
    /// Triggers when an item is used by the player
    /// </summary>
    /// <param name="layerIndex"></param>
    public void OnItemUsedHandler(object target)
    {
        UpdateItems();
    }
    public void OnResetPlayerHandler(object target)
    {
        UpdateItems();
    }

    /// <summary>
    /// Triggers when scene changes
    /// </summary>
    /// <param name="target"></param>
    public void OnEnterSceneHandler(object target = null)
    {
        UpdateItems();
    }

    public void OnGameOverHandler(object value = null)
    {
        // Update the inventory to new HUD status
        UpdateItems();
    }
    #endregion

}