using UnityEngine;
using UnityEngine.EventSystems;
using TargetUtils;

public class ChaseTarget : MonoBehaviour
{
    #region Fields
    [SerializeField] private float reactionDelay = .5f; //how often the targets position gets checked for movement
    [SerializeField] private float enemySpeed = 1;
    [SerializeField] private float minDistance = 0;
    [SerializeField] private float maxDistance = 0;
    [SerializeField] private string targetTag = "PlayerTag";
    [SerializeField] private string decoyTag = "Teno";
    [SerializeField] private AudioClipName currentclip;
    [SerializeField] private bool audio3d = true;
    [SerializeField, Range(0f, 90f)] private float variationAngle = 15f;
    [SerializeField, Range(0f, 1f)] private float idleProbability = 0.5f;
    [SerializeField] private NPCState moveState = NPCState.Idle;
    [SerializeField] bool coward = false;
    [SerializeField] float stunDuration = .1f;

    private float moveDistance = 1f;
    Transform target;
    private AudioSource audioSource;

    Vector2 targetPosition = Vector2.zero;

    Timer targetPositionTimer;
    bool playWalkAudio = false;

    //Stun timer (for knockback and stun effects)
    private bool stunned = false;
    Timer stunTimer;

    private float reactionDelayRandomization = .2f;

    public NPCState MoveState { get => moveState; set => moveState = value; }
    public bool IsWalking => (MoveState != NPCState.Idle && !stunned);
    private Rigidbody2D rb;
    #endregion

    private void OnEnable()
    {
        StartChase();
    }

    private void StartChase()
    {
        //Setup Rigidbody for Dynamic movement
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;

        audioSource = GetComponent<AudioSource>();

        //Add a timer component, listener for timer finished event, and set duration to fire cooldown
        targetPositionTimer = gameObject.AddComponent<Timer>();

        // Create a timer to update the targets location (and where to move to)
        targetPositionTimer.AddTimerFinishedListener(UpdateTargetPosition);

        //add slight randomization to reactiontime
        float nextReactionDelay = reactionDelay + (reactionDelay * Random.Range(-reactionDelayRandomization, reactionDelayRandomization));

        targetPositionTimer.Duration = nextReactionDelay;
        targetPositionTimer.Run();
        stunTimer = gameObject.AddComponent<Timer>();
    }

    // FixedUpdate is called 50 times per second
    private void FixedUpdate()
    {
        Vector2 currentPosition = rb.position; // Use Rigidbody2D position

        //Calculate distance to target
        float distance = Vector2.Distance(targetPosition, transform.position);

        //If targetposition requires moving towards, them play walking sound and move towards it.
        //  0.01f is used to avoid tiny movements which just cost processing, and cause audio to play with barely any movement
        if (distance > 0.05f && !stunned)
        {
            // If there is a valid target
            PlayWalking();

            // Compute direction toward target
            Vector2 direction = (targetPosition - currentPosition).normalized;

            // Apply velocity
            rb.linearVelocity = direction * enemySpeed;
        }
        else
        {
            StopWalking();
        }

    }

    /// <summary>
    /// Apply a "knockback" effect to the player preventing them from moving for a set period.
    /// </summary>
    /// <param name="duration"></param>
    public void Stun()
    {
        //Stun player for default time of .1 frames
        Stun(stunDuration);
    }

    public void Stun(float duration)
    {
        //set knockback to true
        stunned = true;

        //set timer
        stunTimer.Duration = duration;

        //tell timer what to do when finished
        stunTimer.AddTimerFinishedListener(EndStun);

        //start a timer until player can move again
        stunTimer.Run();
    }

    // after Knockback cooldown set knockback to false again
    public void EndStun()
    {
        //set knockback to true
        stunned = false;

        stunTimer.Duration = 0;
    }


    /// <summary>
    /// Updates the current targets location. Called at the end of a timer, and resets the timer after being called
    /// </summary>
    public void UpdateTargetPosition()
    {
        // Finding a target to track for enemy pathing
        target = EnemyTarget.GetTarget(transform.position, maxDistance);

        if (target != null)
        {
            //temporarily store the actual position
            Vector2 actualPosition = target.position;
            Vector2 currentPosition = transform.position;

            //Calculate the target position depending on min/max distance values// Find the Targets current position and distance
            float distance = Vector2.Distance(actualPosition, currentPosition);
            
            // Ensure maxDistance is not less than minDistance
            maxDistance = Mathf.Max(maxDistance, minDistance);

            //Get direction towards target
            Vector2 direction = (actualPosition - currentPosition).normalized; 
            direction = RandomizeMoveDirection(direction);

            //if outside of max distance, find a location within range of the target
            if (distance < minDistance || coward)
            {
                // Move away to be at least minDistance
                MoveState = NPCState.Flee;
                targetPosition = currentPosition - (direction * moveDistance);

                //Debug.Log("Flee distance to targetPosition: " + Vector2.Distance(currentPosition, targetPosition));
            }
            else if (distance > maxDistance)
            {
                // Move closer within maxDistance
                MoveState = NPCState.Chase;
                //targetPosition = actualPosition - (direction * (maxDistance - distance));
                targetPosition = currentPosition + (direction * moveDistance);
            }
            else
            {
                bool wanderCheck = (Random.value > idleProbability);

                //Randomly determine whether to stay still or move
                if (wanderCheck)
                {
                    //  randomly select a target location
                    float wanderRadius = .5f; // Adjust for how far they can wander
                    Vector2 randomOffset = Random.insideUnitCircle * wanderRadius;
                    targetPosition = currentPosition + randomOffset;
                    MoveState = NPCState.Wander;
                }
                else
                {
                    targetPosition = currentPosition;
                    MoveState = NPCState.Idle;
                }
            }
        }

        //add slight randomization to reactiontime
        float nextReactionDelay = reactionDelay + (reactionDelay * Random.Range(-reactionDelayRandomization, reactionDelayRandomization));

        //restart timer (if something prevented an update, then it will check again)
        targetPositionTimer.Duration = nextReactionDelay;
        targetPositionTimer.Run();
    }

    /// <summary>
    /// Rotates a movement direction randomly up to variationAngle degrees away from the target
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector2 RandomizeMoveDirection(Vector2 direction)
    {
        Vector2 newVector = direction;
        if (variationAngle > 0)
        {
            float rotateAngle = Random.Range(-variationAngle, variationAngle);
            newVector = RotateVector(direction, rotateAngle);
        }
        return newVector;
    }

    /// <summary>
    /// Rotates a Vector2 by the given angle in degrees.
    /// </summary>
    private Vector2 RotateVector(Vector2 v, float angleDegrees)
    {
        float radians = angleDegrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            v.x * cos - v.y * sin,
            v.x * sin + v.y * cos
        );
    }
    /// <summary>
    /// this is called when enemy is moving and starts sounds
    /// </summary>
    public void PlayWalking()
    {
        if (!playWalkAudio)
        {
            AudioManager.Play(currentclip, true, AudioType.SFX, gameObject, audio3d);
            playWalkAudio = true;
        }
    }

    /// <summary>
    /// this is called when enemy is not moving and this ends the looping with the sounds
    /// </summary>
    public void StopWalking()
    {
        if (playWalkAudio) // Only stop the sound if it's currently playing
        {
            AudioManager.StopLoopedSoundByName(currentclip);  // A method that stops the specific sound
            playWalkAudio = false;
        }
    }

    void OnDestroy()
    {
        //Stop audio
        AudioManager.StopLoopedSoundByName(currentclip);

        //remove timers
        if (targetPositionTimer != null)
            targetPositionTimer.ClearTimerFinishedListener();
        if (stunTimer != null)
            stunTimer.ClearTimerFinishedListener();
    }

    // Expose targetPosition so animation/movement scripts can query intended movement direction
    public Vector2 TargetPosition => targetPosition;
}
