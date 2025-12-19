using System.Collections.Generic;
using UnityEngine;

public class DeathDropPickup : MonoBehaviour
{
    // List of items enemy can drop
    [SerializeField] private List<Item> itemDrops = new List<Item>();
    [SerializeField] private List<GameObject> dropPrefabs = new List<GameObject>();

    // Enemy drop chance (set to 100% by default)
    [SerializeField] private float dropChancePercent = 100f;
    [SerializeField] private bool removeProjectiles = false;

    //can adjust where you drop the item
    [SerializeField] private Vector2 relativeDropCoordinates = Vector2.zero;

    /// <summary>
    /// Create pickup at current position.
    /// </summary>
    private void Drop()
    {
        //modify drop location using relativeDropCoordinates
        Vector3 dropPosition = transform.position + (Vector3)relativeDropCoordinates;

        // If droppable items are stored, drop them
        if (itemDrops.Count > 0)
        {
            // Get a random int between 0 and the size of the itemDrops list minus 1
            int randIndex = UnityEngine.Random.Range(0, itemDrops.Count);

            // Get the matching random item from the list and instantiate
            Item drop = itemDrops[randIndex];
            Pickup.CreatePickup(drop, dropPosition);
        }

        //if droppable non items are stored (such as corpses and death animations)
        if (dropPrefabs.Count > 0)
        {
            // Get a random int between 0 and the size of the dropPrefabs list minus 1
            int randIndex = UnityEngine.Random.Range(0, dropPrefabs.Count);

            // Get the matching random object from the list and instantiate
            GameObject drop = dropPrefabs[randIndex];
            GameObject playerDeathAnim = Instantiate(drop, dropPosition, Quaternion.identity);
        }
    }

    
    // Rolls whether or not item is dropped
    private void RollDropChance()
    {
            // Roll a random number between 0-100
            float dropChance = UnityEngine.Random.Range(0, 100);

            // If the randomly rolled number is below the drop chance, drop the item
            if (dropChance < dropChancePercent)
            {
                Drop();
            }

            //if delete all projectiles on death (for boss)
            if (removeProjectiles)
            {
                DeleteProjectiles();
            }
    }


    public void Die()
    {
            RollDropChance();
    }

    public void DeleteProjectiles()
    {
        GameObject[] projectiles = GameObject.FindGameObjectsWithTag("BulletTag");
            
        //get all projectiles in scene and delete
        foreach (GameObject proj in projectiles)
        {
            Destroy(proj);
        }
    }
}