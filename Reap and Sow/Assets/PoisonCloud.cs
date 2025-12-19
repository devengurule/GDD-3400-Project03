using UnityEngine;
using UnityEngine.UIElements;

public class PoisonCloud : MonoBehaviour
{
    [SerializeField] private int damage;
    [SerializeField] private float duration;
    private ParticleSystem ps;

    private Timer timer;

    private void Start()
    {
        ps = GetComponent<ParticleSystem>();

        timer = gameObject.AddComponent<Timer>();
        timer.Duration = duration;
        timer.AddTimerFinishedListener(FadeAway);
        timer.Run();
    }

    private void Update()
    {
        if (!timer.Running)
        {
            if (ps == null || ps.particleCount <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public int GetDamage()
    {
        return damage;
    }
    
    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    private void FadeAway() 
    {
        if (ps != null)
        {
            var emission = ps.emission;
            emission.enabled = false;
        }
    }
}
