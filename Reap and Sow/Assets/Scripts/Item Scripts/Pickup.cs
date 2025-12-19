using System.Collections;
using UnityEngine;

/// <summary>
/// Represents item that can be picked up by the player.
/// </summary>
public class Pickup : MonoBehaviour
{
    public Item itemInfo; // Info for the item the pickup represents.

    /// <summary>
    /// Start is called on start
    /// </summary>
    private void Start()
    {
        //This is needed if manually placing a pickup item
        if (itemInfo != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = itemInfo.itemSprite;
        }
    }

    public void Initialize(Item newInfo)
    {
        this.itemInfo = newInfo;

        //this is needed if generating a pickup from an item script
        if (itemInfo != null)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            sr.sprite = itemInfo.itemSprite;
        }
        else
        {
            //Pickup is broken
            Debug.Log("Pickup object spawned with no iteminfo");
            Destroy(gameObject);
        }
    }
    
    // Method to execute when pickup is picked up
    public void PickedUp(Transform player)
    {
        // Disable pickup collisions
        this.GetComponent<CircleCollider2D>().enabled = false;

        // Make the player the pickup's parent 
        // This makes the pickup's localPosition be relative to the player
        this.transform.SetParent(player);

        // Move pickup to player
        this.transform.position = player.transform.position;

        // Start picked up animation
        StartCoroutine(CollectionAnim());
    }

    // Coroutine for collection animation 
    IEnumerator CollectionAnim()
    {
        const float ENDPOINT = 0.25f; // The point the pickup should stop at
        const float FLOAT_SPEED = 2f; // The speed at which the pickup should move
        const float WAIT_TIME = 0.0001f; // The time between yields in the loop
        const float END_TIME = 0.1f; // The time to wait before destroying the pickup

        while (this.transform.localPosition.y < ENDPOINT)
        {
            this.transform.Translate(0f, FLOAT_SPEED * Time.deltaTime, 0f);
            yield return new WaitForSeconds(WAIT_TIME);
        }

        yield return new WaitForSeconds(END_TIME);
        
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Creates a pickup of type at the specified posistion
    /// </summary>
    /// <param name="newInfo">Info for item to spawn</param>
    /// <param name="spawnPosition">Posistion to spawn pickup</param>
    /// <returns>The newly created item as a GameObject.</returns>
    public static GameObject CreatePickup(Item newInfo, Vector3 spawnPosition)
    {
        GameObject pickupPrefab = Resources.Load<GameObject>("Prefabs/Pickups/Pickup");
        GameObject newItem =null;
        if(pickupPrefab)
        {
            newItem = Instantiate(pickupPrefab, spawnPosition, Quaternion.identity);
            Pickup itemScript = newItem.GetComponent<Pickup>();
            itemScript.Initialize(newInfo);
        }
        return newItem;
    }
}