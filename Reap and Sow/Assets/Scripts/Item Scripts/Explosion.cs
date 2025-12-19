using UnityEngine;

public class Explosion : MonoBehaviour
{

    [SerializeField] int explosiveDamage = 2;
    [SerializeField] float explosionDuration = 0.5f;

    Timer explosionDurationTimer;

    [Tooltip("Setting attack audio clip ")]
    [SerializeField] private AudioClipName currentExplodeClip;
    [SerializeField] private AudioClipName currentGasClip;
    [SerializeField] private bool audio3d = true;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //play bullet sound (if able)
        if (currentExplodeClip != AudioClipName.None)
        {
            AudioManager.Play(currentExplodeClip, false, AudioType.SFX, gameObject, audio3d);
            AudioManager.Play(currentGasClip, true, AudioType.SFX, gameObject, audio3d);
        }

        // Explosion duration timer
        explosionDurationTimer = gameObject.AddComponent<Timer>();
        explosionDurationTimer.Duration = explosionDuration;
        explosionDurationTimer.AddTimerFinishedListener(KillExplosionHitbox);
        explosionDurationTimer.Run();
    }

    void KillExplosionHitbox()
    {
        Destroy(gameObject);
    }

    // Getter and setter for damage
    public int getDamage() { return explosiveDamage; }
    public void setDamage(int damage) { explosiveDamage = damage; }
}
