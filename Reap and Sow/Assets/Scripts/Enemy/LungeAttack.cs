using System.Collections;
using UnityEngine;
using TargetUtils;

public class LungeAttack : MonoBehaviour
{
    [SerializeField] private float lungeSpeed = 5;  // Speed of the lunge
    [SerializeField] private float lungeCooldown = 2;  // Cooldown between lunges
    [SerializeField] private float maxDistance = 0; //maximum distance to lunge
    [SerializeField] private string targetTag = "PlayerTag";
    [SerializeField] float lungeDelay = 0.5f; // Delay before the lunge
    [SerializeField] float lungeDuration = 0.5f; // Duration of the lunge
    private Transform target; // The target to lunge towards
    private float lastLungeTime; // The time of the last lunge
    private bool isLunging = false; // Whether the enemy is currently lunging
    private MoveAnim moveAnim;

    private void OnEnable()
    {
        StartAttack();
    }

    private void StartAttack()
    {
        moveAnim = GetComponent<MoveAnim>();
    }

    void Update()
    {
        //finding a target to track for enemy 
        if (target != null)
        {
            target = EnemyTarget.GetTarget(transform.position);
        }
    }

    private void FixedUpdate()
    {
        if (!isLunging)
        { 
            // perform a lunge attack if the target is within range and the cooldown has passed
            if (Time.time >= lastLungeTime + lungeCooldown)
            {
                target = EnemyTarget.GetTarget(transform.position);

                if (target != null)
                {
                    // find the target's current position and distance
                    Vector3 targetCurrentLocation = target.position;
                    float distance = Vector2.Distance(target.position, transform.position);

                    if (distance <= maxDistance)
                    {
                        StartCoroutine(PerformLungeAttack(targetCurrentLocation));
                        lastLungeTime = Time.time;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Perform a lunging attack towards the target with a delay
    /// </summary>
    /// <param name="targetPosition">The position of the target</param>
    private IEnumerator PerformLungeAttack(Vector3 targetPosition)
    {
        isLunging = true;
        // wait for the delay before executing the lunge
        yield return new WaitForSeconds(lungeDelay);

        // compute direction for animation choice
        Vector2 lungeDir = (targetPosition - transform.position).normalized;
        if (moveAnim != null)
        {
            moveAnim.StartLungeAnim(lungeDir);
        }

        // reset the elapsed time
        float elapsedTime = 0;

        // move towards the target position (the execution of the lunge) for the duration of the lunge
        while (elapsedTime < lungeDuration)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, lungeSpeed * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (moveAnim != null)
        {
            moveAnim.EndLungeAnim();
        }

        isLunging = false;
    }

    // method to get the target
    private void GetTarget()
    {
        if (GameObject.FindGameObjectWithTag(targetTag))
        {
            target = GameObject.FindGameObjectWithTag(targetTag).transform;
        }
    }
}
