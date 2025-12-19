using UnityEngine;

public class CollisionTracker : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
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
    void OnCollisionExit2D(Collision2D collision)
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

