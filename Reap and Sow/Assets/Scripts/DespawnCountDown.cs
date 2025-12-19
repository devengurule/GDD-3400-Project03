using UnityEngine;

public class DespawnCountDown : MonoBehaviour
{
    private Timer despawnTimer;
    [SerializeField] private float despawnTime = 2f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (despawnTime > 0)
        {
            despawnTimer = gameObject.AddComponent<Timer>();
            despawnTimer.AddTimerFinishedListener(Despawn);
            despawnTimer.Duration = despawnTime;
            despawnTimer.Run();
        }
    }
    
    void Despawn()
    {
        Destroy(gameObject);
    }
}
