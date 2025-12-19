using UnityEngine;

/// <summary>
/// Class that contains methods related to item usage.
/// </summary>
public static class ItemUse
{
    /// <summary>
    /// Use the given ammo.
    /// </summary>
    /// <param name="ammo">The seed item to use.</param>
    public static void UseAmmo(AmmoItem ammo, Vector3 mousePos, Transform playerTransform)
    {
        //Get prefab for projectile from item
        GameObject projectilePrefab = ammo.projectilePrefab;

        //Instantiate Projectile and grab its projectileScript
        GameObject projectile = Object.Instantiate(projectilePrefab, playerTransform.position, Quaternion.identity);
        ProjectileScript projscript = projectile.GetComponent<ProjectileScript>();

        //Reference start and target for bullet
        Vector2 targetLocation = Camera.main.ScreenToWorldPoint(mousePos);

        //Fire the darn thing
        projscript.Fire(targetLocation, ammo);
    }

    /// <summary>
    /// Plant the given item.
    /// </summary>
    public static bool PlantSeed(SeedItem seed, Vector3 position)
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(position);
        //GameObject plot = Physics2D.OverlapCircle(position, 0.2f).gameObject;
        Collider2D[] results = Physics2D.OverlapPointAll(mousePos);

        if (results.Length != 0)
        {
            GameObject plot = System.Array.Find<Collider2D>(results, x => x.gameObject.CompareTag("PlotTag")).gameObject;


            if (plot != null)
            {
                PlotScript script = plot.GetComponent<PlotScript>();

                if (script != null)
                {
                    if (script.GetCurrentPlant() == null)
                    {
                        script.PlantSeed(seed.plant);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        return true;
    }


}