using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    #region Variables
    EventManager eventManager;
    GameController gameController;
    private Inventory itemInventory;
    public Inventory PlayerInventory { get => itemInventory; }
    
    private bool isFirstSeed = true;
    private bool isFirstHealthPickup = true;
    private bool isFirstThornPickup = true;
    string healingTutorial = "tutorialpickup_health";
    string seedTutorial = "tutorialpickup_seed";
    string thornTutorial = "tutorialpickup_thorn";
    //string invalidPlantLocation = "invalidplantspace";
    #endregion

    #region Unity Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get reference to event manager
        eventManager = GameController.instance.EventManager;

        // Get reference to gamecontroller
        gameController = GameController.instance;

        // Get reference to inventories
        itemInventory = GameController.instance.ItemInventory;

        //Add a flag for tutorials if not already added
        if (!gameController.tutorialDict.ContainsKey(seedTutorial))
        {
            gameController.tutorialDict.Add(seedTutorial, isFirstSeed);
        }
        if (!gameController.tutorialDict.ContainsKey(healingTutorial))
        {
            gameController.tutorialDict.Add(healingTutorial, isFirstHealthPickup);
        }
        if (!gameController.tutorialDict.ContainsKey(thornTutorial))
        {
            gameController.tutorialDict.Add(thornTutorial, isFirstThornPickup);
        }

        // Subscribe to input events
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.NextItem, OnNextItemHandler);
            eventManager.Subscribe(EventType.PrevItem, OnPrevItemHandler);
            eventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
        }

        //publish playerspawned event
        eventManager.Publish(EventType.PlayerStart);
        
        // Make object persistent between rooms
        DontDestroyOnLoad(gameObject);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// OnDestroy is called when the GameObject or component it is attached to is destroyed.
    /// </summary>
    void OnDestroy()
    {
        eventManager.Unsubscribe(EventType.NextItem, OnNextItemHandler);
        eventManager.Unsubscribe(EventType.PrevItem, OnPrevItemHandler);
        eventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler);
    }

    #endregion

    // Method to add an item to the inventory
    public void AddItem(Item itemToAdd)
    {   
        // If the seed pickup tutorial dialogue hasn't played yet and the player picks up a seed
        if (gameController.tutorialDict.ContainsKey(seedTutorial) && gameController.tutorialDict[seedTutorial] && (itemToAdd.itemType == ItemType.Seed))
        {
            // Run the seed tutorial dialogue
            gameController.tutorialDict[seedTutorial] = false;
            HUDTextManager.Instance.StartDialogue(seedTutorial);
        }
        // Otherwise if the healing item pickup tutorial dialogue hasn't played yet and the player picks up a healing item
        else if (gameController.tutorialDict.ContainsKey(healingTutorial) && gameController.tutorialDict[healingTutorial] && (itemToAdd.itemType == ItemType.Healing))
        {
            // Run the healing item tutorial dialogue
            gameController.tutorialDict[healingTutorial] = false;
            HUDTextManager.Instance.StartDialogue(healingTutorial);
        }
        else if (gameController.tutorialDict.ContainsKey(thornTutorial) && gameController.tutorialDict[thornTutorial] && (itemToAdd.itemType == ItemType.Projectile))
        {
            gameController.tutorialDict[thornTutorial] = false;
            HUDTextManager.Instance.StartDialogue(thornTutorial);
        }

        // If the item picked up is a seed, add it to the seed inventory. Otherwise, add it to the item inventory
        itemInventory.AddItem(itemToAdd);
    }

    #region Events
    // Called when nextItem action is performed
    public void OnNextItemHandler(object action)
    {
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        itemInventory.NextItem();
        ItemType currentItemType = PlayerInventory.GetCurrentItemType();
    }

    // Called when prevItem action is performed
    public void OnPrevItemHandler(object action)
    {
        SpriteRenderer playerSprite = GetComponent<SpriteRenderer>();
        itemInventory.PrevItem();
        ItemType currentItemType = PlayerInventory.GetCurrentItemType();
    }

    /// <summary>
    /// Moves Player to the spawn position of new scene when entering
    /// </summary>
    /// <param name="value"></param>
    public void OnEnterSceneHandler(object value)
    {
        if (value is Vector2 targetPosition)
        {
            transform.position = targetPosition;
        }
    }
    #endregion
}
