using UnityEngine;

public class AttackAnim : MonoBehaviour
{
    private PlayerAttack playerAttack;
    Animator anim;
    int dirInt;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerAttack = GetComponent<PlayerAttack>();
        anim = GetComponent<Animator>();
    }

    public void StartAttack(Vector2 dir)
    {
        // if the player is attacking to the right or left
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            // if the player is attacking to the right
            if (dir.x > 0)
            {
                dirInt = 3;
            }
            // if the player is attacking to the left
            else
            {
                dirInt = 1;
            }
        }
        // if the player is attacking up or down
        else
        {
            // if the player is attacking up
            if (dir.y > 0)
            {
                dirInt = 2;
            }
            // if the player is attacking down
            else
            {
                dirInt = 0;
            }
        }
        if (anim != null)
        {
            anim.SetInteger("Direction", dirInt);
            anim.SetTrigger("Attack");
        }
    }

    // getter for dirInt
    public int GetDirInt()
    {
        return dirInt;
    }
}
