using UnityEngine;

public class DeathAnimation : MonoBehaviour
{
    [SerializeField]
    public string firstAnimationStateName;    
    private Animator anim;
    private EventManager eventManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();

        if (anim == null)
        {
            Debug.LogError("Error: No Animator Component found on this GameObject!");

            PublishEvent();
        }

        if (!string.IsNullOrEmpty(firstAnimationStateName))
        {
            anim.Play(firstAnimationStateName);
        }
    }

    public void PublishEvent()
    {
        eventManager = GameController.instance.EventManager;

        eventManager.Publish(EventType.PlayerDeath);

        Destroy(gameObject);
    }
}
