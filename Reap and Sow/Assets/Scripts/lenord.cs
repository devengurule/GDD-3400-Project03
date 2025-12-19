using UnityEngine;

public class lenord : MonoBehaviour
{

    private Animator animator;
    private GameObject player;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("PlayerTag");
        animator.speed = 0.2f;
        animator.SetTrigger("Right");
    }

    // Update is called once per frame
    void Update()
    {
        animator.speed = 0.2f;
        if (player.transform.position.x < transform.position.x) animator.SetTrigger("Right");
        else animator.SetTrigger("Left");
    }
}
