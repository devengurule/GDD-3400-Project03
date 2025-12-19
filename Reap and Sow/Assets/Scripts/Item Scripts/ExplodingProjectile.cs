using UnityEngine;

public class ExplodingProjectile : ProjectileScript
{

    // Explosion hitbox and explosion delay
    [SerializeField] public GameObject explosionPrefab;
    [SerializeField] float explosionDelay = 2f;

    Timer explosionTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        explosionTimer = gameObject.AddComponent<Timer>();
        explosionTimer.Duration = explosionDelay;
        explosionTimer.AddTimerFinishedListener(Explode);
        explosionTimer.Run();
    }

    // Instantiate the explosion hitbox
    void Explode()
    {

        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);

    }

}
