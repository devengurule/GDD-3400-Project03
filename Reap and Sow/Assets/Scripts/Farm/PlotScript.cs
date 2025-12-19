using UnityEngine;

/// <summary>
/// PlotScript handles behaviors and plant growth for a single plot on players farm
/// </summary>
public class PlotScript : MonoBehaviour
{
    #region Fields
    [SerializeField]
    private Plant currentPlant = null;

    [Header("Plot Timers")]
    [SerializeField]
    // This is the time before transitioning to Young stage
    private float seedlingTimer = 5f;

    [SerializeField]
    // This is the time before transitioning to Nourished stage
    private float youngTimer = 10f;

    [SerializeField]
    // This is the max growth time
    private float adultTimer = 15f;

    // This is setting the current growth stage of the plant
    private PlotStages currentStage = PlotStages.Unplanted;

    // This is the sprite renderer to display plot sprite
    [Header("Plot Sprites")]
    private SpriteRenderer plotSpriteRenderer;

    [SerializeField]
    // This is the sprite for unplanted state
    private Sprite emptyPlotSprite;

    [SerializeField]
    // This is the sprite for seedling stage
    private Sprite seedlingSprite;

    [SerializeField]
    // This is the Sprite for young stage
    private Sprite youngSprite;

    [SerializeField]
    // This is the Sprite for Adult stage
    private Sprite adultSprite;

    [Header("Harvested Item")]
    [SerializeField]
    // This is an item that is harvested based on plant type
    private Item harvestableItem;

    private EventManager eventManager;

    [SerializeField]
    // This is defining a range for the harvest items to drop (for example, a radius of 1 to 3 units)
    float harvestSpreadRadius = 1f;

    // Individual plant timer
    private Timer plantTimer;

    string earlyHarvest = "earlyharvest";

    private GameObject harvestParticles;
    private SpriteRenderer harvestIndicator;

    [SerializeField] private GameObject Harvestparticle;

    #endregion

    #region Unity Methods
    private void Start()
    {
        // Get plot's sprite renderer
        plotSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        // Get harvest indicator game object
        harvestParticles = this.transform.GetChild(0).gameObject;
        harvestIndicator = this.transform.GetChild(1).GetComponent<SpriteRenderer>();

        // Get reference to event manager
        eventManager = GameController.instance.EventManager;

        // This subscribes to the farm events
        eventManager.Subscribe(EventType.EnterScene, OnEnterSceneHandler);
        eventManager.Subscribe(EventType.LeaveFarm, OnExitFarmHandler);

        // This adds a listener to the global clock event
        //GlobalClockScript.Instance.AddTimerFinishedListener(OnTimerFinishedHandler);

        // Give the plot a plant timer
        plantTimer = gameObject.AddComponent<Timer>();

        // Give the plant timer a listener to when it finishes
        plantTimer.AddTimerFinishedListener(UpdateGrowthStage);

        // If nothing is planted, then set plant accordingly
        if (currentPlant == null)
        {
            // This initializes the plot as unplanted
            currentStage = PlotStages.Unplanted;
            plotSpriteRenderer.sprite = emptyPlotSprite;
        }
        // Initialize plant depending on what planttype is planted
        else
        {
            PlantSeed(currentPlant);
        }
    }

    private void Update()
    {
        
        // Update() is not needed but it can stay here
        // As long as it pays rent :D

    }

    /// <summary>
    /// OnDestroy is called when the GameObject or component it is attached to is destroyed.
    /// </summary>
    void OnDestroy()
    {
        if (eventManager)
        {
            eventManager.Unsubscribe(EventType.EnterScene, OnEnterSceneHandler);
            eventManager.Unsubscribe(EventType.LeaveFarm, OnExitFarmHandler);
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// This method updates the growth stage
    /// </summary>
    private void UpdateGrowthStage()
    {
        // This is handling growth stage transitions based on the global timer
        if (currentStage == PlotStages.Seedling)
        {
            currentStage = PlotStages.Young;
            plotSpriteRenderer.sprite = currentPlant.youngSprite;

            // Set new duration of plant timer to youngTimer and start it
            plantTimer.Duration = youngTimer;
            plantTimer.Run();
        }
        else if (currentStage == PlotStages.Young)
        {
            currentStage = PlotStages.Adult;
            plotSpriteRenderer.sprite = currentPlant.adultSprite;

            // Turn on harvest indicator
            harvestParticles.SetActive(true);
            harvestIndicator.enabled = true;

            // Set new duration of plant timer to adultTimer and start it
            plantTimer.Duration = adultTimer;
            plantTimer.Run();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// This method plants a seed in the plot
    /// </summary>
    /// <param name="plantType">Plant type Enum</param>
    public void PlantSeed(Plant plant)
    {
        AudioManager.Play(AudioClipName.sfx_player_shoveldigging, loop: false);
        this.currentPlant = plant;

        // Sets the plant type, starts the seedling stage and updates the sprite renderer
        currentStage = PlotStages.Seedling;
        plotSpriteRenderer.sprite = currentPlant.seedlingSprite;

        // Updates the harvestable item with the item to drop on harvest
        harvestableItem = plant.drop;

        // Initializing the plant timer with the seedling timer duration and starting it
        plantTimer.Duration = seedlingTimer;
        plantTimer.Run();
    }

    /// <summary>
    /// This public method when called harvests an adult/young plant
    /// </summary>
    /// <returns>item, null</returns>
    public Item HarvestPlant()
    {
        // This is checking the current stage and return the corresponding harvestable item
        if (currentStage == PlotStages.Adult)
        {
            AudioManager.Play(AudioClipName.sfx_player_harvestplant, loop: false);
            // Disable harvest indicator
            harvestIndicator.enabled = false;
            harvestParticles.SetActive(false);

            // This is returning 3 harvestable items if the plant is Adult
            SpawnHarvestedItem(3);
            Instantiate(Harvestparticle, transform.position, Quaternion.identity);
            return harvestableItem;
        }
        else if (currentStage == PlotStages.Young)
        {
            AudioManager.Play(AudioClipName.sfx_player_harvestplant, loop: false);
            // This is returning 1 harvestable item if the plant is still young
            SpawnHarvestedItem(1);
            HUDTextManager.Instance.StartDialogue(earlyHarvest);
            Instantiate(Harvestparticle, transform.position, Quaternion.identity);
            return harvestableItem;
        }

        // This is when there is no harvest if the plant is not ready
        return null;
    }

    /// <summary>
    /// Gets the current plant of the plot
    /// </summary>
    /// <returns>Plant</returns>
    public Plant GetCurrentPlant()
    {
        return currentPlant;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// This method spawns the harvested item and its quantity
    /// </summary>
    /// <param name="quantity">int quantity of item to spawn</param>
    private void SpawnHarvestedItem(int quantity)
    {

        // If there is a harvestable item, drop it in a random area within a circle and reset the plot
        if (harvestableItem != null)
        {
            
            for (int i = 0; i < quantity; i++)
            {
                // This is getting a random direction in 2D space (within a circle)
                Vector2 randomDirection = Random.insideUnitCircle.normalized;

                // This is getting a random distance within the defined radius
                float randomDistance = Random.Range(0.5f, harvestSpreadRadius);

                // This is calculating the offset position based on random direction and distance
                Vector3 spawnPosition = (Vector3)(randomDirection * randomDistance) + transform.position;

                // This is instantiating the itemPrefab at the randomized position
                Pickup.CreatePickup(harvestableItem, spawnPosition);
            }

            if (harvestableItem is not BossItem)
            {
                ResetPlot();
            }
            else
            {
                PlantSeed(currentPlant);
            }
        }
    }

    /// <summary>
    /// This method resets the plot to unplanted stage
    /// </summary>
    private void ResetPlot()
    {
        // This is resetting the current plant type and stage
        currentPlant = null;
        currentStage = PlotStages.Unplanted;

        // This is resetting the plot sprite to the empty state
        plotSpriteRenderer.sprite = emptyPlotSprite;
    }
    #endregion

    #region Events
    /// <summary>
    /// This method starts or resumes global timers as necessary for scene
    /// </summary>
    /// <param name="obj"></param>
    public void OnEnterSceneHandler(object obj)
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Farm")
        {
            // This is the start or resume the global timer when the player enters the farm
            if (currentPlant != null && currentStage != PlotStages.Unplanted)
            {
                // This is starting the global timer for seedling stage
                GlobalClockScript.Instance.StartTimer(seedlingTimer);
            }
        }
    }

    public void OnExitFarmHandler(object obj)
    {
        // This is the pause the global timer when the player leaves the farm
        if (currentPlant != null && currentStage != PlotStages.Unplanted)
        {
            // This is stopping the global timer
            GlobalClockScript.Instance.StopTimer();
        }
    }

    /// <summary>
    /// This delegate calls the update growth stage method
    /// </summary>
    private void OnTimerFinishedHandler()
    {
        // This is updating the growth stage after the timer finishes
        UpdateGrowthStage();
    }
    #endregion
}