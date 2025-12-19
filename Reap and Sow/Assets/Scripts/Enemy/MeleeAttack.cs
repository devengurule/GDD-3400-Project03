using UnityEngine;
using TargetUtils;

public class MeleeAttack : MonoBehaviour
{

    [SerializeField] GameObject meleeHitbox;
    [SerializeField] float meleeAttackDuration = 2f;
    [SerializeField] float meleeAttackDelay = 1f;
    [SerializeField] float meleeAttackRange = 1f;
    //[SerializeField] float meleeHitboxSize = 2f;
    [SerializeField] int meleeAttackDamage = 25;
    [SerializeField] private string targetTag = "PlayerTag";
    private Transform target;
    private GameObject hitbox;
    private Timer attackTimer;
    private Timer attackTimerDelay;
    private bool canAttack = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Attack timer for actual attack
        attackTimer = gameObject.AddComponent<Timer>();
        attackTimer.AddTimerFinishedListener(MeleeAttackEnd);
        attackTimer.Duration = meleeAttackDuration;

        // Timer for attack delay
        attackTimerDelay = gameObject.AddComponent<Timer>();
        attackTimerDelay.AddTimerFinishedListener(MeleeAttackStart);
        attackTimerDelay.Duration = meleeAttackDelay;

    }

    // Update is called once per frame
    void Update()
    {

        // If no target, get target
        if (target == null)
        {

            target = EnemyTarget.GetTarget(transform.position);

        }

    }

    void FixedUpdate()
    {

        if (target != null)
        {

            Vector3 targetPosition = target.position;
            float distance = Vector2.Distance(targetPosition, transform.position);

            // If the enemy is within range of the player and is able to attack
            if (distance <= meleeAttackRange && canAttack)
            {

                // Set canAttack to false so that this doesn't run a million times
                canAttack = false;

                // Start the attack delay
                attackTimerDelay.Run();

            }

        }

    }

    // Melee damage setter
    public void SetMeleeDamage(int damage)
    {

        meleeAttackDamage = damage;

    }

    // Melee damage getter
    public int GetMeleeDamage()
    {

        return meleeAttackDamage;

    }

    // Gets the player as the target
    private void GetTarget()
    {

        if (GameObject.FindGameObjectWithTag(targetTag))
        {

            target = GameObject.FindGameObjectWithTag(targetTag).transform;

        }

    }

    private void MeleeAttackStart()
    {

        // Instantiate the hitbox for the melee attack
        hitbox = Instantiate(meleeHitbox, this.transform);
        Vector2 attackDirection = CalculateDirection();

        hitbox.transform.localPosition = attackDirection;
        attackTimer.Run();


    }

    private void MeleeAttackEnd()
    {

        Destroy(hitbox);
        canAttack = true;

    }

    private Vector2 CalculateDirection()
    {

        Vector2 targetPosition = target.position;

        // Find the direction of the target in reference to the enemy
        Vector2 targetDirection = new Vector2(
            targetPosition.x - transform.position.x,
            targetPosition.y - transform.position.y);
        targetDirection.Normalize();

        return targetDirection;

    }
}
