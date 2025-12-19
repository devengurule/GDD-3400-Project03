using UnityEngine;

//Spawns the item, and lasts for <Duration> Amount of time
public class TemporarySpawnObject : MonoBehaviour
{
    [SerializeField] float DespawnTimer = 2f;
    Timer spawnTimer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        spawnTimer = gameObject.AddComponent<Timer>();
        spawnTimer.Duration = DespawnTimer;
        spawnTimer.AddTimerFinishedListener(Despawn);
        spawnTimer.Run();
    }

    /// <summary>
    /// Dwspawn after despawntimer completes
    /// </summary>
    public void Despawn()
    {
        Destroy(spawnTimer);
        Destroy(gameObject);
    }
}
