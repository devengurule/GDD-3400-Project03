using UnityEngine;

public class MoveAnim : MonoBehaviour
{
    enum Direction
    {
        Left = 1,
        Right = 3,
        Up = 2,
        Down = 0
    }

    [SerializeField] bool logAnimationErrors = false;
    [SerializeField] bool mirrorLeftAnimations = false; // old behavior for left animation (not used unless original programmer wants to use it)
    [SerializeField] bool flipBasedOnMovement = true;   // preferred movement vector
    [Tooltip("If true the sprite's 'front' points to the right when scale.x is positive. If false front points left.")]
    [SerializeField] bool initiallyFacesRight = true;
    [SerializeField] private AudioClipName currentclip;

    // Lunge animation names (can be adjusted per prefab in inspector)
    [SerializeField] private string leftLungeAnim = "Left_Walk";
    [SerializeField] private string rightLungeAnim = "Right_Walk";

    Animator anim;
    Rigidbody2D rb;
    ChaseTarget moveScript;

    Direction direction = Direction.Down;
    float baseScaleX;

    string downWalkAnim = "Down_Walk";
    string upWalkAnim = "Up_Walk";
    string leftWalkAnim = "Left_Walk";
    string rightWalkAnim = "Right_Walk";
    string downIdleAnim = "Down_Idle";
    string upIdleAnim = "Up_Idle";
    string leftIdleAnim = "Left_Idle";
    string rightIdleAnim = "Right_Idle";

    bool isWalking = true;

    private float velocityDeadZone = 0.05f; //If velocity is below this then treat as staying still

    // When true, MoveAnim will not update/play normal movement animations (used for lunges / attacks)
    private bool actionLocked = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        moveScript = GetComponent<ChaseTarget>();

        anim.SetInteger("Direction", (int)direction);

        // store absolute base scale (use magnitude so flipping uses +/− of this)
        baseScaleX = Mathf.Abs(transform.localScale.x);
    }


    // Changing to FixedUpdate to match the FixedUpdate function in ChaseTarget.cs
    // (Unsure if this will fix the animations or not, but it also needs to be done to ensure that
    // the calculations and animations match the same speed, update calls once a frame, but fixedupdate
    // calls 50 times per second)
    void FixedUpdate()
    {
        // If an action (like a lunge) is playing, do not override the animation
        if (actionLocked) return;

        if (ShouldWalk())
        {
            GetDirection();
            PlayWalking();
        }
        else
        {
            StopWalking();
        }
    }

    public bool ShouldWalk()
    {
        // Use moveScript only when it exists and is enabled; otherwise fall back to Rigidbody velocity
        if (moveScript != null && moveScript.isActiveAndEnabled)
            return moveScript.IsWalking;
        return rb != null && rb.linearVelocity.magnitude > velocityDeadZone;
    }

    private void GetDirection()
    {
        Vector2 movementVec = Vector2.zero;

        // Prefer the chase script's intended movement vector only when it's active
        if (moveScript != null && moveScript.isActiveAndEnabled)
        {
            movementVec = moveScript.TargetPosition - (Vector2)transform.position;

            // If very small, fall back to rb velocity
            if (movementVec.sqrMagnitude < 0.0001f && rb != null)
                movementVec = rb.linearVelocity;
        }
        else if (rb != null)
        {
            movementVec = rb.linearVelocity;
        }

        float velocityX = movementVec.x;
        float velocityY = movementVec.y;

        // Choose primary axis
        if (Mathf.Abs(velocityY) < Mathf.Abs(velocityX))
        {
            if (velocityX < 0) direction = Direction.Left;
            else if (velocityX > 0) direction = Direction.Right;
        }
        else
        {
            if (velocityY > 0) direction = Direction.Up;
            else if (velocityY < 0) direction = Direction.Down;
        }

        anim.SetInteger("Direction", (int)direction);

        // Apply consistent flipping based on movement vector when enabled
        if (flipBasedOnMovement && Mathf.Abs(velocityX) > 0.01f)
        {
            float sign = Mathf.Sign(velocityX); // +1 = right, -1 = left
            int faceDir = initiallyFacesRight ? 1 : -1;
            transform.localScale = new Vector3(baseScaleX * faceDir * (int)sign, transform.localScale.y, transform.localScale.z);
        }
        else if (mirrorLeftAnimations)
        {
            // fallback for old behavior
            CheckFlipAnimationtoLeft();
        }
    }

    // start lunge animation (locks movement animation updates)
    public void StartLungeAnim(Vector2 lungeDirection)
    {
        actionLocked = true;

        // determine left/right from horizontal component
        float vx = lungeDirection.x;
        if (Mathf.Abs(vx) < 0.01f)
        {
            // fallback to current facing sign if horizontal is tiny
            vx = transform.localScale.x;
        }

        if (vx < 0)
            SafePlay(leftLungeAnim);
        else
            SafePlay(rightLungeAnim);
    }

    // end lunge animation and resume normal animations
    public void EndLungeAnim()
    {
        actionLocked = false;

        // reset to idle/walk frame for current direction to avoid snapping to wrong anim
        PlayDirectionAnimation(direction, downIdleAnim, leftIdleAnim, upIdleAnim, rightIdleAnim);
    }

    public void PlayWalking()
    {
        if (!isWalking)
        {
            isWalking = true;
            AudioManager.Play(currentclip, true, AudioType.SFX, gameObject, false);
            this.anim.SetBool("IsWalking", isWalking);

            //Should we move the playAnimation method to here like the stopWalking method?
        }

        PlayDirectionAnimation(direction, downWalkAnim, leftWalkAnim, upWalkAnim, rightWalkAnim);
    }

    public void StopWalking()
    {
        if (isWalking)
        {
            isWalking = false;
            AudioManager.StopLoopedSoundByName(currentclip);
            anim.SetBool("IsWalking", isWalking);

            PlayDirectionAnimation(direction, downIdleAnim, leftIdleAnim, upIdleAnim, rightIdleAnim);
        }
    }

    private void PlayDirectionAnimation(Direction dir, string downAnim, string leftAnim, string upAnim, string rightAnim, int layer = 0)
    {
        string animToPlay;
        switch (dir)
        {
            case Direction.Left: animToPlay = leftAnim; break;
            case Direction.Up: animToPlay = upAnim; break;
            case Direction.Right: animToPlay = rightAnim; break;
            case Direction.Down:
            default: animToPlay = downAnim; break;
        }
        SafePlay(animToPlay, layer);
    }

    public void SafePlay(string stateName, int layer = 0)
    {
        if (anim.HasState(layer, Animator.StringToHash(stateName)))
        {
            anim.Play(stateName, layer);

            // Only use legacy/old flip if flipBasedOnMovement is disabled
            if (!flipBasedOnMovement)
                CheckFlipAnimationtoLeft();

            if (logAnimationErrors)
            {
                var clips = anim.GetCurrentAnimatorClipInfo(layer);
                string clipName = clips.Length > 0 ? clips[0].clip.name : "None";
                var currentStateInfo = anim.GetCurrentAnimatorStateInfo(layer);
                Direction dir = (Direction)anim.GetInteger("Direction");
                Debug.Log($"[{gameObject.name}] Playing animation. Direction: {dir} Requested State: {stateName} Current Clip: {clipName}");
            }
        }
        else if (logAnimationErrors)
        {
            Debug.LogWarning($"{gameObject.name} Animation: '{stateName}' does not exist on layer {layer}!");
        }
    }

    private void CheckFlipAnimationtoLeft()
    {
        float baseX = Mathf.Abs(baseScaleX);
        if (mirrorLeftAnimations && direction == Direction.Left)
            transform.localScale = new Vector3(-baseX, transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(baseX, transform.localScale.y, transform.localScale.z);
    }

    private void FlipTowardsTarget(Transform target)
    {
        if (target == null) return;
        float dx = target.position.x - transform.position.x;
        const float deadzone = 0.01f;
        if (Mathf.Abs(dx) < deadzone) return;
        float sign = Mathf.Sign(dx);
        transform.localScale = new Vector3(Mathf.Abs(baseScaleX) * sign, transform.localScale.y, transform.localScale.z);
    }
}
