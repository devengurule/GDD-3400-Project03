using UnityEngine;

public class BearTrap : MonoBehaviour
{
    [SerializeField]
    Sprite closedSprite; // The bear trap's closed sprite
    [SerializeField]
    public float trapTime; // The time to keep the player in the trap
    [SerializeField] private AudioClipName currentclip;

    // Triggers when a collider2D touches the bear trap
    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if other object is the player 
        if (other.CompareTag("PlayerTag"))
        {
            //triggers the closing animation
            this.GetComponent<Animator>().SetTrigger("Close");
            AudioManager.Play(currentclip, loop: false, AudioType.SFX, gameObject, false);
            // Disable itself and change to closed sprite
            this.GetComponent<BoxCollider2D>().enabled = false;
            this.GetComponent<SpriteRenderer>().sprite = closedSprite;
        }
    }
}
