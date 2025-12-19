using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    // Projectile Speed
    [SerializeField] public float bulletSpeed = 1;

    // Projectile damage
    [SerializeField] public int bulletDamage = 1;

    Vector2 startLocation = Vector2.zero;

    /// <summary>
    /// When the projectile leaves the map, destroy this instance
    /// </summary>
    private void OnBecameInvisible()
    {
        Destroy(this.gameObject);
    }


    /// <summary>
    /// Fires a projectile in a given direction
    /// </summary>
    /// <param name="targetLocation">destination of bullet</param>
    /// <param name="ammo">Optional: an AmmoItem to designate specific ammo values</param>
    virtual public void Fire(Vector2 targetLocation, AmmoItem ammo = null)
    {
        startLocation = transform.position;

        // get data from player item if
        if (ammo != null)
        {
            this.bulletSpeed = ammo.speed;
        }

        // Calculate attack direction and rotation
        Vector2 attackDir = PlayerAttack.CalculateDirection(startLocation, targetLocation);
        float angle = (Mathf.Atan2(attackDir.y, attackDir.x) * Mathf.Rad2Deg) - 90f;
        this.transform.rotation = Quaternion.Euler(0, 0, angle);

        //add velocity
        GetComponent<Rigidbody2D>().linearVelocity = attackDir * bulletSpeed;
    }

    /// <summary>
    /// Give a damage value to the projectile
    /// </summary>
    /// <param name="damage">Damage value passed through from player</param>
    public void SetDamage(int damage)
    {
        this.bulletDamage = damage;
    }

    /// <summary>
    /// Sets the speed value of the projectile
    /// </summary>
    /// <param name="speed"></param>
    public void SetSpeed(float speed)
    {
        this.bulletSpeed = speed;
    }

    /// <summary>
    /// Return a damage value so we're not directly accessing it
    /// </summary>
    /// <returns>Damage of the projectile</returns>
    public int GetDamage()
    {
        return bulletDamage;
    }

    /// <summary>
    /// Called when the projectile collides with something.
    ///     For now this is used simply to destroy the projectile on collision
    ///     Damage is handled by collisionManager
    /// </summary>
    /// <param name="collision"></param>
    virtual protected void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(this.gameObject);
    }

    /// <summary>
    /// Calculates a Vector2 to return the direction between the player and the mouse
    /// </summary>
    /// <returns>Direction between player and mouse/target</returns>
    public static Vector2 CalculateDirection(Vector3 originPoint, Vector3 targetPoint)
    {
        // Gets mouse position for attacking/facing direction
        Vector2 targetDirection = new Vector2(
            targetPoint.x - originPoint.x,
            targetPoint.y - originPoint.y);
        targetDirection.Normalize();
        return targetDirection;
    }
}