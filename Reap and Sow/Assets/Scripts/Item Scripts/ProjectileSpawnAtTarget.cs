using UnityEngine;
using System.Collections;
using UnityEngine.Assertions.Must;

public class ProjectileSpawnAtTarget : ProjectileScript
{

    // Info about explosion
    [Tooltip("Max Range limits how far the item will travel before spawning")]
    [SerializeField]
    public float maxRange = .5f;
    public float MaxRange { get => maxRange; set => maxRange = value; }
    float range = .5f;

    [SerializeField]
    GameObject spawnObject;

    [SerializeField] bool rotateProjectile = false;

    [SerializeField] private bool delayDespawn;
    [SerializeField] private float delayDespawnTimerDurration;

    [SerializeField] private float angleOffset;

    private ParticleSystem ps;

    private Timer delayDespawnTimer;

    //store start location for tracking distance
    private Vector2 startPosition;
    private bool activated = false;

    [SerializeField] private AudioClipName currentclip;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();

        delayDespawnTimer = gameObject.AddComponent<Timer>();
        delayDespawnTimer.Duration = delayDespawnTimerDurration;
        delayDespawnTimer.AddTimerFinishedListener(Despawn);
    }


    /// <summary>
    /// Fires a projectile in a given direction
    /// </summary>
    /// <param name="targetPosition">destination of bullet</param>
    /// <param name="ammo">Optional: an AmmoItem to designate specific ammo values</param>
    override public void Fire(Vector2 targetPosition, AmmoItem ammo = null)
    {
        startPosition = transform.position;


        // get data from player item if
        if (ammo != null)
        {
            this.bulletSpeed = ammo.speed;
        }

        //Update range if target is closer than maxRange
        float rawDistance = Vector2.Distance(startPosition, targetPosition);
        range = Mathf.Clamp(rawDistance, 0f, maxRange);


        // Calculate attack direction and rotation
        Vector2 attackDir = PlayerAttack.CalculateDirection(startPosition, targetPosition).normalized;
        float angle = (Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg) - 90f;
        if (rotateProjectile) this.transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);

        //add velocity
        GetComponent<Rigidbody2D>().linearVelocity = attackDir * bulletSpeed;
    }

    /// <summary>
    /// Updated more often than Update for physics checks
    ///     Used to determine if bullet has reached it's target location
    /// </summary>
    private void FixedUpdate()
    {
        // Check how far projectile has traveled
        float distance = Vector2.Distance(startPosition, transform.position);

        // activate once reached target distance
        if (distance >= range)
        {
            Activate();
        }
    }

    /// <summary>
    /// Handles the explosion of an explosive projectile
    /// </summary>
    public void Activate()
    {

        if (!activated)
        {
            activated = true;
            GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            // Ensure a valid projectile is stored first
            if (spawnObject != null)
            {
                // Instantiates a projectile object and fires the projectile

                if (spawnObject.gameObject.tag == "ExplosiveTag")
                {
                    GameObject explosion = Instantiate(spawnObject, transform.position, transform.rotation);
                    explosion.GetComponent<Explosion>().setDamage(GetDamage());
                }
                else 
                {
                    Instantiate(spawnObject, transform.position, transform.rotation);
                }
            }
            else
            {
                Debug.Log($"{gameObject.name}: No valid spawnObject prefab found! ");
            }

            if (!delayDespawnTimer.Running && delayDespawn)
            {
                if (ps != null)
                {
                    // Turns off particle emmision
                    var em = ps.emission;
                    em.enabled = false;
                }

                delayDespawnTimer.Run();
            }
            else Destroy(gameObject);
        }
    }

    /// <summary>
    /// Override OnCollisionEnter2D so that we can spawn the spawnobject on collision
    /// </summary>
    /// <param name="collision"></param>
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        // Spawn first
        Activate();
    }

    private void Despawn()
    {
        Destroy(gameObject);
    }
}
