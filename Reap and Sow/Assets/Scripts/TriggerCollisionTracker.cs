using UnityEngine;

public class TriggerCollisionTracker : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Get reference to collision manager
        CollisionManager collisionManager = GameController.instance.CollisionManager;

        if (collisionManager)
        {
            // This object
            GameObject thisObject = gameObject;

            // Other object involved in the collision
            GameObject otherObject = collision.gameObject;
            
            collisionManager.ManageCollision(thisObject, otherObject);
        }
    }
    void OnTriggerExit2D(Collider2D collision)
    {
        // Get reference to collision manager
        CollisionManager collisionManager = GameController.instance.CollisionManager;

        if (collisionManager)
        {
            // This object
            GameObject thisObject = gameObject;

            // Other object involved in the collision
            GameObject otherObject = collision.gameObject;

            collisionManager.ManageCollisionExit(thisObject, otherObject);
        }
    }

}

