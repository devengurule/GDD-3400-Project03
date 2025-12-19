using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class GlobalClockScript : MonoBehaviour
{
    #region Fields 
    private static GlobalClockScript instance;
    private EventManager eventManager;

    //Declare the string for day or night
    public string dayNight = "Day";

    //declare the amount of time it takes for the day to night transition
    [SerializeField, Range(1f, 20f)] 
    private float transitionDuration = 5f;

    //set the tracker that checks the duration of the change to zero
    private float transitionTimer = 0f;
    private bool isTransitioning = false;

    // PostProcessVolume and colorgrading of current camera
    private PostProcessVolume volume;
    private ColorGrading colorGrading;

    [SerializeField] private float dayTemp = 10f, nightTemp = -25f;
    [SerializeField] private float dayExposure = 0f, nightExposure = -3f;

    // The starting temp/exposure, the temp/exposure to target, and the current temp/exposure
    private float startTemp, targetTemp, currentTemp;
    private float startExposure, targetExposure, currentExposure;

    // This is the total seconds that gets set by the script its applied too.
    private float totalSeconds;
    // This is just the amount of time that has passed since we started the timer
    private float elapsedSeconds;
    // This is just a way of checking if the base timer is running
    private bool running;
    [SerializeField]
    // This is just a way to add sprites into an array that we apply later by an index
    private Sprite[] clockSprites;
    [SerializeField]
    // This is just an way to set what image we are applying too
    private Image clockImage;
    // This is just an index for the images 
    private int imageIndex = 0;
    [SerializeField, Range(10f, 400f)]
    // This is just a total timer for how long the day will be
    private float totalDurationForDay;
    // This is just a timer finished event
    private TimerFinishedEvent finishedEvent = new TimerFinishedEvent();
    // this is for the constant game timer
    private float gameTimer = 0f;

    // this is just a Singleton that you can access

    public static GlobalClockScript Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Object.FindAnyObjectByType<GlobalClockScript>();
                if (instance == null)
                {
                    GameObject obj = new GameObject("GlobalClockScript");
                    instance = obj.AddComponent<GlobalClockScript>();
                }
            }
            return instance;
        }
    }

    #endregion

    #region Properties
    // This is a duration float and this is get setting the total seconds based on the value that is given when it is called in.
    public float Duration
    {
        get { return totalSeconds; }
        set
        {
            
            if (!running)
            {
                totalSeconds = value;
            }
        }
    }

    public bool Running => running;
    public bool Finished => !running && elapsedSeconds >= totalSeconds;
    #endregion

    #region Unity Methods

    // This is the start method and during this we will be making sure that it makes it not possible to destroy the clock instance.
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        // Load the current camera's volume 
        loadVolume(dayTemp, dayExposure);

        // Get event manager and subscribe to EnterScene event
        eventManager = GameController.instance.EventManager;

        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.EnterScene, OnEnterScene);
        }
    }


    // This is the update method and during this we are starting the elapsed time and making sure that when the clock is running elapsed seconds and clock timer ends when elapsed is greater than total seconds.
    public void Update()
    {
        // This is the gameTimer
        gameTimer += Time.deltaTime;

        clockmovement();

        // This is resetting the current day timer so that it can repeat
        if (gameTimer >= totalDurationForDay)
        {
            gameTimer = 0f;
            imageIndex = 0;
        }
       
        // This is the timer that is being set and checking if it is finished
        if (running)
        {
            elapsedSeconds += Time.deltaTime;
            
            if (elapsedSeconds >= totalSeconds)
            {
                running = false;
                finishedEvent.Invoke();
            }
        }

        //if the current phase does not sit within the current definitions of the day and night periods
        string newPhase = (imageIndex < 15 || imageIndex > 31) ? "Day" : "Night";


        //set the time of day to nighttime or daytime depending on which it currently isnt
        if (newPhase != dayNight)
        {
            dayNight = newPhase;
            //call the start color transition method
            StartColorTransition(newPhase);
        }

        //if the transition between day and night is still happening.
        if (isTransitioning)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);
            
            currentTemp = Mathf.Lerp(startTemp, targetTemp, t);
            currentExposure = Mathf.Lerp(startExposure, targetExposure, t);

            colorGrading.temperature.value = currentTemp;
            colorGrading.postExposure.value = currentExposure;

            if (t >= 1f)
            {
                isTransitioning = false;
                //Debug.Log("Day/Night Transition Ended");
            }
        }
    }

    public void OnDestroy()
    {
        if (eventManager != null)
        {
            // Unsubscribe from subscribed events
            eventManager.Unsubscribe(EventType.EnterScene, OnEnterScene);
        }
    }
    #endregion

    #region Private Methods



    private void clockmovement()
    {
        // This is to calculate the time per sprite 
        float timePerSprite = totalDurationForDay / clockSprites.Length;

        // This is to calculate which sprite index should be active
        int newImageIndex = Mathf.FloorToInt(gameTimer / timePerSprite);
        newImageIndex = Mathf.Clamp(newImageIndex, 0, clockSprites.Length - 1);
        // This is to only update sprite if the image index has changed
        if (newImageIndex != imageIndex)
        {
            imageIndex = newImageIndex;
            if (clockImage != null)
            {
                clockImage.sprite = clockSprites[imageIndex];
            }
        }
    }

    /// <summary>
    /// Load the post process volume and set color temp and exposure
    /// </summary>
    /// <param name="temp">The temperature to set the color temp to</param>
    /// <param name="exposure">The exposure to set the color exposure to</param>
    private void loadVolume(float temp, float exposure)
    {
        // Obtain the current camera's PostProcessVolume 
        volume = Camera.main.GetComponent<PostProcessVolume>();

        if (volume != null && volume.profile != null)
        {
            // Obtain the volume's colorGrading
            volume.profile.TryGetSettings(out colorGrading);
            if (colorGrading != null)
            {
                // Set colorgrading temperature and exposure to provided temp and exposure
                colorGrading.temperature.value = temp;
                colorGrading.postExposure.value = exposure;
            }
        }
    }
    #endregion

    #region Public Methods
    // This is adding a listener to check if the timer is finished.
    public void AddTimerFinishedListener(UnityAction listener)
    {
        finishedEvent.AddListener(listener);
    }



    private void StartColorTransition(string phase)
    {
        if (colorGrading == null) return;

        startTemp = colorGrading.temperature.value;
        startExposure = colorGrading.postExposure.value;

        targetTemp = (phase == "Day") ? dayTemp : nightTemp;
        targetExposure = (phase == "Day") ? dayExposure : nightExposure;

        transitionTimer = 0f;
        isTransitioning = true;
        //Debug.Log("Day/Night transition started");
    }

    // This is starting the timer and also reseting the elapsed seconds and setting total by what it is applied too.
    public void StartTimer(float duration)
    {
        totalSeconds = duration;
        elapsedSeconds = 0f;
        running = true;
    }

    // This is stoping the timer and reseting the elapsed seconds
    public void StopTimer()
    {
        running = false;
        elapsedSeconds = 0;
    }

    #endregion

    #region TimerFinishedEvent
    [System.Serializable]
    public class TimerFinishedEvent : UnityEvent { }
    #endregion

    #region Event Methods
    // Load the PostProcessVolume of new scene and set its color grading
    public void OnEnterScene(object target = null)
    {
        loadVolume(currentTemp, currentExposure);
    }
    #endregion
}