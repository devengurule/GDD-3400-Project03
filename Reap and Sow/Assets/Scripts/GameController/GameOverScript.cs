using UnityEngine;

public class GameOverScript : MonoBehaviour
{
    private bool isGameOver = false; //set to true as soon as player dies
    private bool canRespawn = false; // set to true after death + a small delay (respawnDelay)
    private float respawnDelay = 1f;
    private Timer respawnDelayTimer;
    [SerializeField] private GameObject gameOverText;
    [SerializeField] private GameObject gameOverScreen;
    EventManager eventManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        eventManager = GameController.instance.EventManager;
        

        gameOverScreen.SetActive(false);
        if (eventManager)
        {
            eventManager.Subscribe(EventType.PlayerDeath, OnGameOverHandler);
            eventManager.Subscribe(EventType.AutoTestStart, OnAutoTestStartHandler);
            eventManager.Subscribe(EventType.AutoTestStop, OnAutoTestStopHandler);
        }
    }

    void Update()
    {

        // Checks if currently on game over screen
        if (isGameOver && canRespawn)
        {
            // If any key is pressed while the game over text is up, respawn the player
            if (Input.anyKeyDown)
            {   
                isGameOver = false;

                // Disable the game over text and respawn the player
                gameOverText.SetActive(false);    
                gameOverScreen.SetActive(false);
                Respawn();
            }
        }
    }

    void OnAutoTestStartHandler(object value = null)
    {
        //Temporarily unsubscribe from GameOver 
        eventManager.Unsubscribe(EventType.PlayerDeath, OnGameOverHandler);
    }
    void OnAutoTestStopHandler(object value = null)
    {
        //Resubscribe to GameOver 
        eventManager.Subscribe(EventType.PlayerDeath, OnGameOverHandler);
    }
    void OnGameOverHandler(object value)
    {
        //set reference booleans
        canRespawn = false;
        isGameOver = true;

        //show game over screen if able
        if(gameOverScreen)
        {
            gameOverScreen.SetActive(true);
        }

        //start respawn delay timer so player doesnt accidentally skip splash screen
        if (respawnDelayTimer == null)
        {
            respawnDelayTimer = gameObject.AddComponent<Timer>();
            respawnDelayTimer.AddTimerFinishedListener(EnableRespawn);
            respawnDelayTimer.Duration = respawnDelay;
        }
        respawnDelayTimer.Run();
    }

    void EnableRespawn()
    {
        if (isGameOver)
        {
            canRespawn = true;

            //show "press any key to continue" text     
            if (gameOverText)
            {
                gameOverText.SetActive(true);            
            }
        }
    }

    // Method to respawn the player
    void Respawn()
    {
        //load event manager to publish events
        EventManager eventManager = GameController.instance.EventManager;

        //create SceneData object (Using zero for player position because the wont be moved)
        SceneData data = new SceneData("Farm",Vector2.zero);

        if(eventManager)
        {
            //Raise event to change scene to Farm
            eventManager.Publish(EventType.ChangeScene, data);
        }
    }
}
