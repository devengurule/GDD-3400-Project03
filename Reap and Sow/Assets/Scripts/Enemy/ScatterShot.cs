using UnityEngine;


public class ScatterShot : MonoBehaviour
{
    float distanceVariation = 0.2f;
    private Vector2 startVector;
    private Vector2 targetVector;

    public float DistanceVariation { get => distanceVariation; set => distanceVariation = value; }
    public GameObject ProjectilePrefab { get; set; }
    public int Quantity { get; set; }
    public int Speed { get; set; }
    public int Damage { get; set; }
    public float Spread { get; set; }

    private float range = int.MaxValue;

    /// <summary>
    /// Receives the information needed to instantiate a single projectile towards target vector
    ///     Determines how to spread out the number of projectiles into a scattershot pattern
    /// </summary>
    /// <param name="projectilePrefab"></param>
    /// <param name="quantity"></param>
    /// <param name="targetVector"></param>
    public void Fire(Vector2 startVector, Vector2 targetVector)
    {
        this.startVector = startVector;
        this.targetVector = targetVector;

        //get the vectors for all spawned bullets
        Vector2[] vectorList = CalculateProjectileSpreadTargets();

        foreach (Vector2 vector in vectorList)
        {
            //fire the projectile
            // Instantiates a projectile object and fires the projectile
            GameObject projectile = Instantiate(ProjectilePrefab, this.startVector, Quaternion.identity);

            //set projectile values
            ProjectileScript projectileScript = projectile.GetComponent<ProjectileScript>();
            projectileScript.SetDamage(Damage);
            projectileScript.SetSpeed(Speed);

            //fire the projectile
            projectileScript.Fire(vector);
        }
    }


    /// <summary>
    /// Returns a list of vectors spread out be spread value in the direction of the target.
    ///     Sets all target vectors to be around the same distance as the initial target vector.
    ///     
    /// </summary>
    /// <param name="initalAngle"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    private Vector2[] CalculateProjectileSpreadTargets()
    {
        //if quantity is somehow 0, then dont do anything
        if (Quantity <= 0) return new Vector2[0];
        if (Quantity == 1) return new Vector2[] { targetVector };

        //First get the initial angle and distance to target vector
        Vector2 toTarget = (targetVector - startVector).normalized;
        float distance = Mathf.Abs(Vector2.Distance(startVector, targetVector)); //determine scattershot distance

        //If projectile uses ProjectileSpawnAtTarget then we can update out distance to only be within range
        ProjectileSpawnAtTarget ps = ProjectilePrefab?.GetComponent<ProjectileSpawnAtTarget>();
        if (ps != null)
        {
            range = ps.MaxRange;
            distance = Mathf.Min(distance, range);
        }

        Vector2[] positions = new Vector2[Quantity];

        // Spread the rest of the projectiles
        // Divide the spread angle across remaining projectiles
        float angleStep = Spread / (Quantity - 1);
        float startAngle = -Spread / 2f; // start leftmost

        //Determine the other vectors, spread out across the target direction
        for (int i = 0; i < Quantity; i++)
        {
            //offset targetVector by spread amount
            float angle = startAngle + angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector2 rotatedDir = Quaternion.Euler(0, 0, angle) * toTarget;

            //add slight random variation to distance
            float adjustedDistance = distance * Random.Range(1 - DistanceVariation, 1 + DistanceVariation);
            adjustedDistance = Mathf.Min(adjustedDistance, range);

            //adjust the position so that it is within <distance> of the startVector
            positions[i] = startVector + rotatedDir * adjustedDistance;
        }

        return positions;
    }

    private void OnDrawGizmos()
    {
        // Only draw if Quantity is valid and StartVector/TargetVector are set
        if (Quantity <= 0 || ProjectilePrefab == null) return;

        Vector2[] positions = CalculateProjectileSpreadTargets();

        Gizmos.color = Color.red; // Color for the target positions
        foreach (var pos in positions)
        {
            Gizmos.DrawSphere(pos, 0.1f); // Draw a small sphere at each target
            Gizmos.DrawLine(startVector, pos); // Optional: draw line from start to target
        }

        // Draw start position as green sphere
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startVector, 0.15f);

        // Draw direct target position as blue sphere
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetVector, 0.15f);
    }

}
