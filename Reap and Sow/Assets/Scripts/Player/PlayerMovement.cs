using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    #region Variables
    private AudioSource[] audioSources;
    EventManager eventManager;

    // Movement
    Rigidbody2D rigidBody;
    [SerializeField] int speed = 1;
    Vector2 moveVector = Vector2.zero;

    private bool stunned = false;
    private bool canMove = true;

    //Stun timer (for knockback and stun effects)
    Timer stunTimer;
    #endregion

    #region Unity Methods
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSources = GetComponents<AudioSource>();

        // Get rigid body reference
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.freezeRotation = true;

        // Get reference to event manager and animator
        eventManager = GameController.instance.EventManager;

        // Sub to appropriate events within event manager
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.Move, OnMoveHandler);
        }

        //setup timer component

        if (stunTimer == null)
            stunTimer = gameObject.AddComponent<Timer>();
        stunTimer.TimerName = "Stun Timer";
        stunTimer.AddTimerFinishedListener(EndStun);
    }

    // Update is called once per frame
    void Update()
    {
        //anim.Animate();
        //check if player is able to move
        if (!stunned && canMove)
        {
            // Move as needed
            rigidBody.linearVelocity = moveVector * speed;
        }

        //This code is what stops player movement for codes using canMove (the player attack animation for example).
        else if (!canMove)
        {
            //stop player movement
            rigidBody.linearVelocity = Vector2.zero;
        }

    }

    /// <summary>
    /// OnDestroy is called when the GameObject or component it is attached to is destroyed.
    /// </summary>
    void OnDestroy()
    {
        AudioManager.StopLoopedSoundByName(AudioClipName.sfx_player_Walking);
        eventManager.Unsubscribe(EventType.Move, OnMoveHandler);
    }
    #endregion




    /// <summary>
    /// Called when Move action is performed
    /// Move event should be raise any time movement input vector changes
    /// Should also be raised when no input is provided to set moveVector to zero.
    /// </summary>
    /// <param name="target"></param>
    public void OnMoveHandler(object target)
    {
        // Update stored vector (if a valid vector is provided). Movement will be applied on Update
        if (target is Vector2 moveVector)
        {
            this.moveVector = moveVector;
        }

    }

    /// <summary>
    /// Apply a "knockback" effect to the player preventing them from moving for a set period.
    /// </summary>
    /// <param name="duration"></param>
    public void Stun()
    {
        //Stun player for default time of .1 frames
        Stun(0.1f);
    }

    public void Stun(float duration)
    {
        //start a timer until player can move again
        stunned = true;
        stunTimer.Duration = duration;
        stunTimer.Run();
    }

    // after Knockback cooldown set knockback to false again
    public void EndStun()
    {
        //set knockback to true
        stunned = false;

        stunTimer.Duration = 0;
    }

    //setter for canMove
    public void SetCanMove(bool canMove)
    {
        this.canMove = canMove;
    }

    //getter for canMove
    public bool GetCanMove()
    {
        return canMove;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("TrapTag"))
        {
            Stun(other.GetComponent<BearTrap>().trapTime);
            rigidBody.linearVelocity = Vector2.zero;
            this.transform.position = other.transform.position;
        }
    }
}
