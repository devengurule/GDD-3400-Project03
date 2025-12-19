using System;
using System.Collections;
using TargetUtils;
using UnityEngine;

public class RangeAttack : MonoBehaviour
{

    #region Serialized Fields
    //      Projectile Settings
    [Header("Projectile Settings")]
    [Tooltip("The prefab used as a projectile")]
    [SerializeField]
    protected GameObject projectilePrefab;

    [Tooltip("How much damage this bullet does")]
    [SerializeField]
    protected int projectileDamage = 1;

    [Tooltip("How fast projectiles move")]
    [SerializeField]
    protected int projectileSpeed = 1;

    [Tooltip("Number of projectiles to be fired (Can't be even)")]
    [SerializeField]
    protected int projectileQty = 1;

    [Tooltip("The angle projectiles will be spread (if more than one) in degrees")]
    [SerializeField]
    protected float scatterShotSpread = 30;

    [Tooltip("Variation in scattershot projectile distance (if more than one) (0 means no variation, 1 means it goes 2x further")]
    [SerializeField, Range (0f, 1f)]
    protected float scatterDistanceVariation = .25f;
    


    //      Firing Settings
    [Header("Firing Settings")]

    [Tooltip("How often shots are fired")]
    [SerializeField, Range(0.1f, 20f)]
    protected float reloadSpeed = 2f;

    [Tooltip("Slightly changes the reload speeds for slight variation (Not all enemies shooting at same time)")]
    [SerializeField, Range(0f, 1f)]
    protected float reloadSpeedRandomization = .2f;

    [Tooltip("Time before first shot (to allow player to load)")]
    [SerializeField, Range(0.1f, 10f)]
    protected float initialFireDelay = 2.5f;

    [Tooltip("How long the firing animation should play")]
    [SerializeField, Range(0.1f, 5f)]
    protected float animationDelayTime = 0.75f;

    //      Target Settings
    [Header("Target Settings")]
    [Tooltip("Maximum distance this unit can shoot at")]
    [SerializeField]
    protected float maxRange = 10f;

    [SerializeField]
    protected bool useMaxRange;

    [Header("Audio Settings")]
    [Tooltip("Setting attack audio clip ")]
    [SerializeField]
    protected AudioClipName currentclip;
    [SerializeField]
    protected bool audio3d = true;
    #endregion

    #region Fields
    protected Timer fireTimer;
    protected Timer animationDelayTimer;
    protected Transform targetTransform;
    protected AttackAnim attackAn;
    protected ChaseTarget moveScript;
    protected bool hasRun = false;
    protected bool randomFire = false;
    protected ScatterShot scatterShot; 
    private bool firing = false; //used to prevent duplicate Fire commands


    //lazyloaded Scattershot for multi-attack enemies
    public ScatterShot ScatterShot => scatterShot ??= gameObject.AddComponent<ScatterShot>();

    #endregion
  

    protected virtual void OnEnable()
    {
        StartAttack();
    }

    //OnValidate is called whenever a serialized value is changed
    protected virtual void OnValidate()
    {
        // Ensure projectileQty is odd
        if (projectileQty % 2 == 0)
        {
            projectileQty += 1; // round up to next odd number
        }
    }

    protected virtual void StartAttack()
    {
        attackAn = GetComponent<AttackAnim>();

        //Add a timer component, listener for timer finished event, and set duration to fire cooldown  
        if (fireTimer == null)
            fireTimer = gameObject.AddComponent<Timer>();

        // Listens for when the fire timer is finished, runs the Firable method when this happens  
        fireTimer.AddTimerFinishedListener(StartAttackAnimation);
        fireTimer.TimerName = "fire Timer";

        //animation timer
        if (animationDelayTimer == null)
            animationDelayTimer = gameObject.AddComponent<Timer>();
        animationDelayTimer.AddTimerFinishedListener(Fire);
        animationDelayTimer.Duration = animationDelayTime;
        animationDelayTimer.TimerName = "animation Delay Timer";

        // get movement script if able
        GetCheckMoveScript();

        // Start Firing Timer (slight randomization so not all enemies attack at same time)
        float modifiedFireDelay = initialFireDelay + 
            (initialFireDelay * UnityEngine.Random.Range(0, reloadSpeedRandomization));


        Reload(modifiedFireDelay);
    }
    
    /// <summary>
    ///  Call to ensure movement script is located
    /// </summary>
    protected virtual void GetCheckMoveScript()
    {
        if (moveScript == null)
        {
            moveScript = GetComponent<ChaseTarget>();
        }
    }

    protected virtual void Reload()
    {
        Reload(reloadSpeed);
    }
    protected virtual void Reload(float delay)
    {
        //Randomize delay before can fire again// Introduce a slight random delay to desynchronize attacks  
        //multiplies randomDelay x fireRateDelay to create the actual delay before next bullet.
        float randomDelay = (delay * UnityEngine.Random.Range(-reloadSpeedRandomization, reloadSpeedRandomization));
        fireTimer.Duration = delay + randomDelay;

        //start timer to fire again
        fireTimer.Run();
    }

    /// <summary>
    /// Plays the firing animation and then fires the bullet at the appropriate time
    /// </summary>
    protected virtual void StartAttackAnimation()
    {
        //find a viable target
        if(!randomFire) targetTransform = EnemyTarget.GetTarget(transform.position, maxRange);

        if (targetTransform != null)
        {
            //If there is an attack animation to play, then play it
            if (attackAn != null)
            {
                attackAn.StartAttack(targetTransform.position);
                animationDelayTimer.Stop(); // ensure timer is only running once
                animationDelayTimer.Run();
            }
            else //else we cant animate, but we can attack
            {
                Fire();
            }
        }
        else //we cant attack now, reload
        {
            Reload();
        }
    }

    /// <summary>
    /// Instantiates a projectile prefab and gives it a vector/velocity, initiates 'cooldown' and runs the timer
    /// </summary>
    protected virtual void Fire()
    {
        //avoid duplicate Fire() attempts
        if (!firing)
        {
            firing = true;
            StartCoroutine(SafeFireCoroutine());
        }
    }

    private IEnumerator SafeFireCoroutine()
    {

        //only fire if a viable target a valid projectile is stored first
        if (targetTransform != null && projectilePrefab != null)
        {
            // Find Targets current position and distance
            Vector2 targetLocation = targetTransform.position;
            float distance = Vector2.Distance(targetTransform.position, transform.position);

            //Check if target is in range
            if ((distance <= maxRange))
            {
                PlayProjectileAudio();
                StunTarget();

                if (projectileQty > 1)
                {
                    ScatterFire(targetLocation);
                }
                //else use standard projectile logic
                else
                {
                    //Generate a spawn position
                    SingleFire(targetLocation, this.transform.position);
                }
            }
        }
        yield return null;

        //allow firing again
        firing = false;

        //restart firing sequence
        Reload();
    }

    private void StunTarget()
    {
        //if movement script tell it to stun
        if (moveScript != null)
        {
            moveScript.Stun();
        }
    }

    private void PlayProjectileAudio()
    {
        //play bullet sound (if able)
        if (currentclip != AudioClipName.None)
        {
            AudioManager.Play(currentclip, false, AudioType.SFX, gameObject, audio3d);
        }
    }

    private void SingleFire(Vector2 targetLocation, Vector2 spawnPos)
    {
        // Instantiates a projectile object and fires the projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        //set projectile values
        ProjectileScript projectileScript = projectile.GetComponent<ProjectileScript>();
        projectileScript.SetDamage(projectileDamage);
        projectileScript.SetSpeed(projectileSpeed);


        //fire the projectile
        projectileScript.Fire(targetLocation);
    }

    private void ScatterFire(Vector2 targetLocation)
    {
        //Use Scattershot logic for projectiles
        ScatterShot.ProjectilePrefab = projectilePrefab;
        ScatterShot.Quantity = projectileQty;
        ScatterShot.Speed = projectileSpeed;
        ScatterShot.Spread = scatterShotSpread;
        ScatterShot.Damage = projectileDamage;
        ScatterShot.DistanceVariation = scatterDistanceVariation;
        ScatterShot.Fire(transform.position, targetLocation);
    }

    void OnDestroy()
    {
        //remove timers
        if (fireTimer != null)
            fireTimer.ClearTimerFinishedListener();
        if (animationDelayTimer != null)
            animationDelayTimer.ClearTimerFinishedListener();
    }
}