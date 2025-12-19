using System.Collections;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{

    #region Collision Behaviors
    /// <summary>
    /// Deals damage to a target if that target has a healthScript attached
    /// </summary>
    /// <param name="target"></param>
    /// <param name="damage"></param>
    public void DealDamage(GameObject target, int damage)
    {
        // Get Health script of target
        HealthScript healthScript = target.GetComponent<HealthScript>();

        // Damage target if healthscript found

        if (healthScript)
        {
            int actualDamage = healthScript.TakeDamage(damage);
            //Debug.Log($"{target.name} was dealt {actualDamage} damage");
        }
    }


    /// <summary>
    /// attempts to get damage value off of an object with projectilescript attached. otherwise returns 0
    /// </summary>
    /// <param name="Source"> Source of the damage </param>
    /// <returns>Damage value as an int</returns>
    public int GetProjectileDamage(GameObject Source)
    {
        // Reference the projectile script of the bullet to get the damage value and enemyHealthScript to deal it
        ProjectileScript projScript = Source.GetComponent<ProjectileScript>();
        int damage = 0;

        // Damage player
        //PlayerScript playerScript = thisObject.GetComponent<PlayerScript>();
        //playerScript.TakeDamage(projScript.damage);

        if (projScript)
        {
            damage = projScript.GetDamage();
        }

        return damage;
    }

    /// <summary>
    /// attempts to get damage value off of an object with projectilescript attached. otherwise returns 0
    /// </summary>
    /// <param name="Source"> Source of the damage </param>
    /// <returns>Damage value as an int</returns>
    public int GetPushDamage(GameObject source)
    {
        int damage = 0;

        // Get push attack damage from the attacker
        PushAttack pushAttackScript = source.GetComponent<PushAttack>();

        if (pushAttackScript)
        {
            damage = pushAttackScript.GetPushDamage();
        }

        return damage;
    }

    /// <summary>
    /// Pushes target away from pusher
    /// </summary>
    /// <param name="pushingObject">the object pushing the other object</param>
    /// <param name="targetObject">the object being pushed</param>
    private void BouncePushApart(GameObject pushingObject, GameObject targetObject)
    {
        // Get push force value from the attacker
        PushAttack pushAttackScript = pushingObject.GetComponent<PushAttack>();
        if (pushAttackScript)
        {
            // Get rigid bodies from both objects
            Rigidbody2D targetRB = targetObject.GetComponent<Rigidbody2D>();
            Rigidbody2D pushingRB = pushingObject.GetComponent<Rigidbody2D>();

            // Calculate direction of push then Bounce the player away from enemy
            Vector2 collisionDirection = (targetRB.position - pushingRB.position).normalized;

            //Push target
            StartCoroutine (MoveObject(targetObject, collisionDirection, pushAttackScript.GetPushForce()));

            //equal opposite reaction
            StartCoroutine (MoveObject(pushingObject, -collisionDirection, pushAttackScript.GetPushForce()));
        }
    }

    /// <summary>
    /// This method is able to move an object in a target direction regardless of if it uses Kinematic or Dynamic rigidbody
    /// </summary>
    /// <param name="targetObject">object to be pushed</param>
    /// <param name="direction">direction to be pushed</param>
    /// <param name="force">how hard to push</param>
    public static IEnumerator MoveObject(GameObject targetObject, Vector2 direction, float force)
    {
        // Get the Rigidbody
        Rigidbody2D rigidBody = targetObject.GetComponent<Rigidbody2D>();
        Collider2D collider = targetObject.GetComponent<Collider2D>();

        //make sure there is a rigidbody and collider (to prevent pushing into another collider)
        if (rigidBody != null && collider != null)
        {
            // Calculate target position
            Vector2 movement = direction.normalized * force;
            Vector2 origin = collider.bounds.center  ;

            //Check for objects to collide with in target direction
            RaycastHit2D hit = CheckPushCollision(targetObject, ref direction, force, rigidBody, collider, origin);

            //Stun the enemy if needed (Otherwise their movement will override push)
            ChaseTarget chaseScript = targetObject.GetComponent<ChaseTarget>();
            if (chaseScript != null)
            {
                chaseScript.Stun();

                //Wait until the existing movement has been stalled
                yield return new WaitForFixedUpdate(); 
            }

            // Reset velocity to avoid drift or stacking forces
            if (rigidBody != null)
            {
                rigidBody.linearVelocity = Vector2.zero;
                rigidBody.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }
    }

    private static RaycastHit2D CheckPushCollision(GameObject targetObject, ref Vector2 direction, float force, Rigidbody2D rigidBody, Collider2D collider, Vector2 origin)
    {

        //Estimate distance of the push
        float estimatedDistance = EstimatePushDistance(targetObject, force);


        // Perform a cast to check for obstacles in the movement path
        //  This lets us check "would this collide with another object?"
        RaycastHit2D hit = Physics2D.BoxCast(
            rigidBody.position,
            collider.bounds.size,
            0f,
            direction,
            estimatedDistance,
            LayerMask.GetMask("Default"));
        Debug.DrawRay(origin, direction.normalized * estimatedDistance, Color.blue, 1f);
        return hit;
    }

    /// <summary>
    /// Estimates how far an object will be pushed based on force and mass. f=ma
    /// </summary>
    /// <param name="targetObject">The object to push.</param>
    /// <param name="force">The force applied to the object.</param>
    /// <param name="time">Time duration for which the force is applied (in seconds). Assuming 1</param>
    /// <returns>The estimated displacement in the direction of the force.</returns>
    public static float EstimatePushDistance(GameObject targetObject, float force, float time=.5f)
    {
        Rigidbody2D rigidBody = targetObject.GetComponent<Rigidbody2D>();

        // Ensure we have a valid Rigidbody2D
        if (rigidBody != null)
        {
            // Calculate acceleration: a = F / m
            float acceleration = force / rigidBody.mass;

            // Estimate displacement: d = 0.5 * a * t^2
            float displacement = 0.5f * acceleration * Mathf.Pow(time, 2);
            return displacement;
        }

        return 0f; // Return 0 if there's no Rigidbody2D
    }


    /// <summary>
    /// Returns a damage value from a players melee attack
    /// </summary>
    /// <param name="attackerObject"></param>
    /// <returns></returns>
    private static int GetMeleeDamage(GameObject attackerObject)
    {
        // Reference the player attack script on the player
        PlayerAttack attack = attackerObject.GetComponentInParent<PlayerAttack>();

        //Deal Damage to enemy
        int damage = attack.GetMeleeDamage();

        // Checking damage value :D
        return damage;
    }

    /// <summary>
    /// Handles picking up an item and adding to inventory
    ///     Currently only works with playerscript due to how inventory is implemented.
    /// </summary>
    /// <param name="item">item to be picked up</param>
    /// <param name="grabber">the object picking up item.</param>
    /// <param name="grabber">the object picking up item.</param>
    private static void PickupItem(GameObject item, GameObject grabber)
    {
        PlayerScript player = grabber.GetComponent<PlayerScript>();
        Pickup itemObject = item.GetComponent<Pickup>();

        //make sure item is added successfully
        player.AddItem(itemObject.itemInfo);

        //destroy item so it is removed from game
        itemObject.PickedUp(player.transform);
    }
    #endregion

    /// <summary>
    /// Master Collision handler takes all collisions, and responds to the collision depending on the tags of each object
    /// </summary>
    /// <param name="object1"> object being collided into </param>
    /// <param name="object2"> object colliding with thisObject</param>
    /// <param name="object2"> object colliding with thisObject</param>
    public void ManageCollision(GameObject object1, GameObject object2)
    {
        string tag1 = object1.tag;
        string tag2 = object2.tag;


        // Player collisions
        // If the player object is colliding with a wall set the speed of the player object to zero
        if ((tag1.Equals("PlayerTag") || tag1.Equals("EnemyTag")) && tag2.Equals("BarrierTag"))
        {
            Rigidbody2D rb = object1.GetComponent<Rigidbody2D>();

            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0;
        }

        // Player collisions
        // For handling cases where player and enemy collide, damage player and knock both objects away from each other
        if (tag1.Equals("PlayerTag"))
        {
            if (tag2.Equals("EnemyTag") || tag2.Equals("BossTurret"))
            {
                //rename objects for clarity
                GameObject enemy = object2;
                GameObject player = object1;


                //Stun player
                PlayerMovement movementScript = player.GetComponent<PlayerMovement>();
                movementScript.Stun();

                //Bounce player and enemy off each other
                BouncePushApart(enemy, player);

                // Damage player
                DealDamage(player, GetPushDamage(enemy));

                // Damage enemy     (if player has a pushdamage value)
                //  Note: Once getPushDamage actually gets a value from pusher uncomment this. In most cases this wont deal any damage, but this makes it easy to imnplement later if for instance an item achanges this
                //  DealDamage(otherObject, GetPushDamage(thisObject));
            }
        }

        // Shield Enemy Collisions
        if (tag1.Equals("ShieldTag") && tag2.Equals("EnemyTag"))
        {
            GameObject enemy = object2;
            GameObject shield = object1;

            DealDamage(shield, GetPushDamage(enemy));
        }

        // Shield bullet collisions
        if (tag1.Equals("ShieldTag") && tag2.Equals("BulletTag"))
        {
            int damage = GetProjectileDamage(object2);

            DealDamage(object1, damage);
        }

        // If the player collides with a basic bullet destory the bullet 
        if (tag2.Equals("BulletTag"))
        {
            //Get damage value from projectile
            int damage = GetProjectileDamage(object2);

            //Deal damage to player
            DealDamage(object1, damage);
        }

        // If a basic player bullet hits an enemy
        if (tag1.Equals("PlayerBulletTag"))
        {
            //Get damage value from projectile
            int damage = GetProjectileDamage(object1);

            //Deal damage to player
            DealDamage(object2, damage);
        }

        // Pickup collision
        if (tag1.Equals("PlayerTag") && tag2.Equals("PickupTag"))
        {
            //define objects for clarity
            GameObject item = object2;
            GameObject player = object1;

            //Pick up item
            PickupItem(item, player);
        }

        // Explosion collisions
        // If a collider is an explosion
        if (tag2.Equals("ExplosiveTag"))
        {
            Explosion explosion = object2.GetComponent<Explosion>();

            //Damage object if able
            int damage = explosion.getDamage();
            DealDamage(object1, damage);
        }

        // Player attacks

        // If a player hits an enemy with a melee attack
        if ( tag1.Equals("PlayerMeleeTag") && tag2.Equals("EnemyTag") )
        {
            //rename objects for clarity
            GameObject targetObject = object2;
            GameObject attackerObject = object1;

            //Get Melee Damage
            int damage = GetMeleeDamage(attackerObject);

            //Deal damage to target and push
            BouncePushApart(attackerObject, targetObject);
            DealDamage(targetObject, damage);
        }

        // If an enemy hits the player with a melee attack
        if (tag1.Equals("PlayerTag") && tag2.Equals("EnemyMeleeTag"))
        {


            GameObject attackerObject = object2;
            GameObject targetObject = object1;

            // Get Melee Damage
            int damage = GetMeleeDamage(attackerObject);

            // Deal damage to target
            DealDamage(targetObject, damage);

        }

        // If a player attacks a plot, attempt to harvest it
        if (tag1.Equals("PlayerMeleeTag") && tag2.Equals("PlotTag"))
        {
            PlotScript plotScript = object2.GetComponent<PlotScript>();
            if (plotScript)
            {
                plotScript.HarvestPlant();
            }
        }

        // If an enemy collides with a pickup, allow it to only pass through the pickup
        if (tag1.Equals("EnemyTag") && tag2.Equals("PickupTag"))
        {
            // Get the rigidbody of the enemy
            Rigidbody2D rb = object1.GetComponent<Rigidbody2D>();
            // Set the enemy to kinematic
            rb.bodyType = RigidbodyType2D.Kinematic;
            // Set the enemy to dynamic
            StartCoroutine(ResetEnemyDynamic(rb));
        }
    }

    public void ManageCollisionExit(GameObject thisObject, GameObject otherObject)
    {
        string tag1 = thisObject.tag;
        string tag2 = otherObject.tag;

        // Player collisions
        // If the player object is colliding with an enemy set the speed of the player object to zero
        if (tag1.Equals("PlayerTag") && tag2.Equals("EnemyTag"))
        {
            Rigidbody2D rb2 = otherObject.GetComponent<Rigidbody2D>();

            rb2.linearVelocity = Vector2.zero;
            rb2.angularVelocity = 0;
            rb2.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    // coroutine to set the enemy back to dynamic
    private IEnumerator ResetEnemyDynamic(Rigidbody2D rb)
    {
        yield return new WaitForSeconds(1f);

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}

