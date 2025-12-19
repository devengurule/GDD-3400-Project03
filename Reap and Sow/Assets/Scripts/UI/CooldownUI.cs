using UnityEngine;

public class CooldownUI : MonoBehaviour
{
    #region Variables
    [SerializeField] private string playerTag;
    private GameObject playerObject;
    private Animator anim;
    private SpriteRenderer sr;
    private EventManager eventManager;
    private AnimatorClipInfo[] clip;
    private float playerCooldown;
    private float playerAttackDurration;
    private float cooldown;
    private float numOfFrames;
    private float frameRate;
    #endregion

    #region Unity Methods
    void Start()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        eventManager = GameController.instance.EventManager;

        // Grab Attacking Event
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.AttackOn, StartCooldown);
            eventManager.Subscribe(EventType.AttackOff, TurnOffCooldownIcon);
            eventManager.Subscribe(EventType.Pause, OnPause);
        }
    }

    void Update()
    {
        // Get array of all animation clips in the animation controller at layer 0
        clip = anim.GetCurrentAnimatorClipInfo(0);

        // Loop though all animation clips in array (should only be 1)
        if (clip.Length > 0)
        {
            foreach (AnimatorClipInfo info in clip)
            {

                if (info.clip.length > 0 && info.clip.frameRate > 0)
                {
                    // Get the framerate and number of frames in the singular clip
                    numOfFrames = info.clip.length * info.clip.frameRate;
                    frameRate = info.clip.frameRate;
                }
            }
        }

        playerObject = transform.parent.gameObject;
        /*// Find player object in scene
        if (playerObject == null)
        {
            // Check all objects in scene for the one with the players tag
            playerObject = GameObject.FindGameObjectWithTag(playerTag);
        }
        else if (playerObject.gameObject.tag != playerTag)
        {
            // Set back to null if intial results are not the correct object
            playerObject = null;
        }*/
    }

    private void OnDestroy()
    {
        // Stop listening to event
        eventManager.Unsubscribe(EventType.AttackOn, StartCooldown);
        eventManager.Unsubscribe(EventType.AttackOff, TurnOffCooldownIcon);
        eventManager.Unsubscribe(EventType.Pause, OnPause);
    }
    #endregion

    #region Events
    /// <summary>
    /// Plays the cooldown animation
    /// </summary>
    /// <param name="target"></param>
    public void StartCooldown(object target)
    {
        if (!sr.enabled && sr != null)
        {
            sr.enabled = true;
        }
        // Grab melee cooldown from attack player
        playerCooldown = playerObject.GetComponent<PlayerAttack>().GetMeleeCooldown();

        // Grab attack duration from attack player
        playerAttackDurration = playerObject.GetComponent<PlayerAttack>().GetAttackDurration();

        // calculate total cooldown time
        cooldown = playerCooldown + playerAttackDurration;

        // Calculate animation speed based on how long the attack is
        float newFrameRate = numOfFrames / cooldown;
        float newAnimationSpeed = newFrameRate / frameRate;

        // Set new speed
        anim.speed = newAnimationSpeed;

        // Play animation
        anim.SetTrigger("cdTrigger");
    }

    void TurnOffCooldownIcon(object target)
    {
        if (sr.enabled && sr != null)
        {
            sr.enabled = false;
        }
    }

    void OnPause(object target)
    {
        if (sr.enabled)
        {
            sr.enabled = false;
        }
        else
        {
            sr.enabled = true;
        }
    }
    #endregion
}