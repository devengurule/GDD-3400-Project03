using System;
using UnityEngine;

public class SpawnAnimation : MonoBehaviour
{
    [SerializeField]
    public string firstAnimationStateName;
    private Animator anim;
    [SerializeField] public GameObject playerPrefab;
    private Action<Vector2> onSpawnComplete;  // Called when animation finishes

    public void OnSpawnComplete (Action<Vector2> onSpawnComplete)
    { 
        this.onSpawnComplete = onSpawnComplete; 
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        anim = GetComponent<Animator>();

        if (anim != null && !string.IsNullOrEmpty(firstAnimationStateName))
        {
            anim.Play(firstAnimationStateName);
        }
        else
        {
            SpawnPlayer();
        }
    }

    /// <summary>
    /// Called by animation event at end of animation clip
    /// </summary>
    public void SpawnPlayer()
    {
 
        onSpawnComplete.Invoke(transform.position); // tell gamecontroller to spawn player at current position
        Destroy(gameObject); // remove the animation object
    }
}
