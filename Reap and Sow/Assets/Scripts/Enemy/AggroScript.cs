using System.Collections;
using UnityEngine;

public class AggroScript : MonoBehaviour
{
    [Header("Aggro")]
    [SerializeField] private string targetTag = "PlayerTag";
    [SerializeField] private float aggroRange = 8f;
    [SerializeField] private float aggroCheckInterval = 0.25f;

    [Header("Wandering (when not aggroed)")]
    [SerializeField] private float wanderRadius = 3f;
    [SerializeField] private float wanderInterval = 2f;
    [SerializeField] private float wanderMoveSpeed = 1f;
    [SerializeField] private float stopDistance = 0.1f;

    private Transform player;
    private Rigidbody2D rb;
    private bool isAggroed = false;
    private Coroutine wanderRoutine;
    [SerializeField] private AudioClipName currentIdleClip;
    [SerializeField] private bool audio3d = true;


    // save component references so we can enable/disable them
    private ChaseTarget chaseComp;
    private LungeAttack lungeComp;
    private RangeAttack rangeComp;

    void Awake()
    {
        // save rigidbody and behaviour components
        rb = GetComponent<Rigidbody2D>();
        chaseComp = GetComponent<ChaseTarget>();
        lungeComp = GetComponent<LungeAttack>();
        rangeComp = GetComponent<RangeAttack>();

        // Disable behavior components early to prevent them running in their Start/OnEnable
        if (chaseComp != null) chaseComp.enabled = false;
        if (lungeComp != null) lungeComp.enabled = false;
        if (rangeComp != null) rangeComp.enabled = false;

        // Start wandering immediately (will be stopped if aggro is detected)
        wanderRoutine = StartCoroutine(WanderRoutine());
    }

    void Start()
    {
        AudioManager.Play(currentIdleClip, true, AudioType.SFX, gameObject, audio3d);
        // Aggro checking can start in Start
        StartCoroutine(AggroCheckLoop());
    }

    private IEnumerator AggroCheckLoop()
    {
        while (true)
        {
            UpdatePlayerReference();

            bool shouldBeAggro = false;
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.position);
                shouldBeAggro = dist <= aggroRange;
            }

            if (shouldBeAggro && !isAggroed)
                SetAggro(true);
            else if (!shouldBeAggro && isAggroed)
                SetAggro(false);

            yield return new WaitForSeconds(aggroCheckInterval);
        }
    }

    private void UpdatePlayerReference()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag(targetTag);
            if (p != null) player = p.transform;
        }
    }

    private void SetAggro(bool on)
    {
        isAggroed = on;

        // Enable/disable saved behaviour components if present
        if (chaseComp != null) chaseComp.enabled = on;
        if (lungeComp != null) lungeComp.enabled = on;
        if (rangeComp != null) rangeComp.enabled = on;

        // Stop/start wandering
        if (on)
        {
            if (wanderRoutine != null)
            {
                StopCoroutine(wanderRoutine);
                wanderRoutine = null;
            }

            // stop physics-based wandering motion
            if (rb != null) rb.linearVelocity = Vector2.zero;
        }
        else
        {
            if (wanderRoutine == null)
                wanderRoutine = StartCoroutine(WanderRoutine());
        }
    }

    private IEnumerator WanderRoutine()
    {
        // Small initial yield so other Awake/Start logic/scritps settles
        yield return null;

        while (!isAggroed)
        {
            Vector2 origin = transform.position;
            Vector2 target = origin + Random.insideUnitCircle * wanderRadius;

            // calculating a reasonable timeout to detect if stuck (based on distance/speed)
            float distanceToTarget = Vector2.Distance(origin, target);
            float estimatedMoveTime = distanceToTarget / Mathf.Max(0.01f, wanderMoveSpeed);
            float maxMoveTime = Mathf.Max(1f, estimatedMoveTime * 3f); // safety multiplier
            float startTime = Time.time;

            // Move toward the target until reached, aggroed, or timeout
            Vector2 dir = Vector2.zero;
            while (!isAggroed && Vector2.Distance(transform.position, target) > stopDistance)
            {
                dir = (target - (Vector2)transform.position).normalized;

                if (rb != null)
                {
                    // timing that is independent of framerate
                    rb.linearVelocity = dir * wanderMoveSpeed;
                    yield return new WaitForFixedUpdate();
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, target, wanderMoveSpeed * Time.deltaTime);
                    yield return null;
                }

                // If stuck (taking too long), break to pick a new random target
                if (Time.time - startTime > maxMoveTime)
                {
                    break;
                }
            }

            // Ensure we stop moving
            if (rb != null)
                rb.linearVelocity = Vector2.zero;

            // Wait a short interval before picking a new wander point (still stop early if aggroed)
            float wait = wanderInterval;
            while (wait > 0f && !isAggroed)
            {
                wait -= Time.deltaTime;
                yield return null;
            }
        }
    }

    // public accessor
    public bool IsAggroed => isAggroed;
}
