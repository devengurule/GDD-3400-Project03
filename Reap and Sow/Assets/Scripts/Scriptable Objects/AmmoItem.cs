using UnityEngine;

[CreateAssetMenu(fileName = "AmmoItem", menuName = "AmmoItem")]
public class AmmoItem : Item
{
    public int time; // How long the seed projectile is on screen.
    public int aoe; // Area of effect of the seed projectile.
    public int damage; // Damage the seed projectile deals.
    public int speed; // Speed of the seed projectile.


    [Tooltip("The prefab used as a projectile")]
    public GameObject projectilePrefab; //Prefab for projectiles
}
