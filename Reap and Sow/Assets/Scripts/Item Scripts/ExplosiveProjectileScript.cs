using UnityEngine;

public class ExplosiveProjectileScript : ProjectileScript
{

    // Info about explosion
    [SerializeField] int timeUntilExplode = 1;
    Timer explosionDelay;
    GameObject explosiveHitbox;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        // Initialize a timer between projectile instantiation and explosion
        explosionDelay = gameObject.AddComponent<Timer>();
        explosionDelay.AddTimerFinishedListener(Explode);
        explosionDelay.Duration = timeUntilExplode;
        explosionDelay.Run();

    }



    /// <summary>
    /// Handles the explosion of an explosive projectile
    /// </summary>
    public void Explode()
    {

        CircleCollider2D projectileCollider = gameObject.GetComponent<CircleCollider2D>();
        projectileCollider.radius = 0.5f;
        Destroy(gameObject);

    }
}
