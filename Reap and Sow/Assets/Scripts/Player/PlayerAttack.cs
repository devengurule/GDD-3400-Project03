using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    #region Variables
    EventManager eventManager;
    PlayerScript playerScript;
    HealthScript healthScript;

    // Melee attacks
    [SerializeField] GameObject shovelHitBox;
    [SerializeField] float meleeAttackDuration = 1;
    [SerializeField] float meleeAttackCooldown = 1;
    [SerializeField] float meleeAttackRange = 0.5f;
    [SerializeField] int meleeAttackDamage = 25;
    [SerializeField] float meleeAttackStun = 0.5f;
    [SerializeField] private float meleeAttackInvincibilityDuration = 0.25f;
    private GameObject hitBox;
    private InputAction moveAction;
    private InputAction attackAction;
    private Timer projectileCooldownTimer;
    private Timer inventoryCooldownTimer;
    private Timer attackTimer;
    private Timer attackCooldownTimer;
    private bool canAttack = true;
    // Remove the problematic field initializer and initialize the field in the Start method instead.
    private int dirInt;

    string downIdleAnim = "Down_Idle";
    string upIdleAnim = "Up_Idle";
    string leftIdleAnim = "Left_Idle";
    string rightIdleAnim = "Right_Idle";


    [SerializeField] private float inventoryCooldown = 0;
    private bool canUseInventory = true;

    [SerializeField] private float projectileCooldown = 0;
    private bool canUseProjectile = true;

    // Shield related fields
    [SerializeField] float shieldDuration = 0;
    [SerializeField] float shieldCooldown = 5;
    private bool canShield = true;
    [SerializeField] GameObject shieldPrefab;

    // Projectile attacks
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] private GameObject Projectileparticle;


    PlayerMovement playerMovement;

    AttackAnim attackAnimation;
    MoveAnim moveAnimation;
    private Animator anim;
    #endregion

    #region Getters and Setters
    public void SetMeleeDamage(int meleeDamage)
    {
        meleeAttackDamage = meleeDamage;
    }
    public int GetMeleeDamage()
    {
        return meleeAttackDamage;
    }
    public float GetMeleeCooldown()
    {
        return meleeAttackCooldown;
    }

    public float GetAttackDurration()
    {
        return meleeAttackDuration;
    }

    public bool GetCanAttack()
    {
        return canAttack;
    }
    #endregion

    #region Unity Methods
    void Start()
    {
        // Get refs to object anim script and event manager
        playerMovement = GetComponent<PlayerMovement>();
        eventManager = GameController.instance.EventManager;
        attackAnimation = GetComponent<AttackAnim>();
        moveAnimation = GetComponent<MoveAnim>();
        anim = GetComponent<Animator>();
        healthScript = GetComponent<HealthScript>();

        // Initialize dirInt after attackAnimation is assigned
        dirInt = attackAnimation.GetDirInt();

        // Get references to player script 
        playerScript = GetComponent<PlayerScript>();

        // Attack Initializations
        if (attackTimer == null)
            attackTimer = gameObject.AddComponent<Timer>();
        attackTimer.TimerName = "Attack Timer";
        attackTimer.AddTimerFinishedListener(ShovelAttackEnd);
        attackTimer.Duration = meleeAttackDuration;

        // Attack cooldown timer initializations
        if (attackCooldownTimer == null)
            attackCooldownTimer = gameObject.AddComponent<Timer>();
        attackCooldownTimer.TimerName = "Attack Cooldown Timer";
        attackCooldownTimer.AddTimerFinishedListener(EndAttackCooldown);
        //attackCooldownTimer.Duration = meleeAttackCooldown;


        if (inventoryCooldownTimer == null)
            inventoryCooldownTimer = gameObject.AddComponent<Timer>();
        inventoryCooldownTimer.TimerName = "Inventory Cooldown Timer";
        inventoryCooldownTimer.AddTimerFinishedListener(EndInventoryCooldown);
        inventoryCooldownTimer.Duration = inventoryCooldown;


        if (projectileCooldownTimer == null)
            projectileCooldownTimer = gameObject.AddComponent<Timer>();
        projectileCooldownTimer.TimerName = "Projectile Cooldown Timer";
        projectileCooldownTimer.AddTimerFinishedListener(EndProjectileCooldown);
        projectileCooldownTimer.Duration = projectileCooldown;

        // Sub to appropriate events within event manager
        if (eventManager != null)
        {
            eventManager.Subscribe(EventType.Attack, OnAttackHandler);
            eventManager.Subscribe(EventType.Fire, OnFireHandler);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(canAttack);
        attackCooldownTimer.Duration = meleeAttackCooldown;

        // if the player's idle direction is not the same as the attack direction, play the idle animation in that direction
        if (dirInt != attackAnimation.GetDirInt())
        {
            dirInt = attackAnimation.GetDirInt();
            switch (dirInt)
            {
                case 0:
                    anim.Play(downIdleAnim);
                    break;
                case 1:
                    anim.Play(leftIdleAnim);
                    break;
                case 2:
                    anim.Play(upIdleAnim);
                    break;
                case 3:
                    anim.Play(rightIdleAnim);
                    break;
            }
        }
    }


    /// <summary>
    /// OnDestroy is called when the GameObject or component it is attached to is destroyed.
    /// </summary>
    void OnDestroy()
    {
        eventManager.Unsubscribe(EventType.Attack, OnAttackHandler);
        eventManager.Unsubscribe(EventType.Fire, OnFireHandler);
    }
    #endregion

    #region Attack Methods
    // Shovel attacks
    void ShovelAttack(Vector3 attackDir)
    {
        //get Z rotation on this somehow
        hitBox = Instantiate(shovelHitBox, this.transform);
        hitBox.transform.localPosition = attackDir * meleeAttackRange;
        attackTimer.Run();
        healthScript.RunInvincibilityFrames(meleeAttackInvincibilityDuration);
        canAttack = false;
        AudioManager.Play(AudioClipName.sfx_player_ShovelSwing, loop: false);
        attackAnimation.StartAttack(attackDir);
        if (playerMovement != null)
        {
            // Stop player movement
            playerMovement.Stun(meleeAttackStun);

            // set canmove bool to false
            playerMovement.SetCanMove(false);

            canAttack = false;

            // Sends out signal that an attack happened
            eventManager.Publish(EventType.AttackOn);
        }
    }
    void ShovelAttackEnd()
    {
        // play the idle animation in the direction of the attack
        switch (dirInt)
        {
            case 0:
                anim.Play(downIdleAnim);
                break;
            case 1:
                anim.Play(leftIdleAnim);
                break;
            case 2:
                anim.Play(upIdleAnim);
                break;
            case 3:
                anim.Play(rightIdleAnim);
                break;
        }

        Destroy(hitBox);
        if (playerMovement != null)
        {
            playerMovement.SetCanMove(true);
        }
        attackCooldownTimer.Run();
    }

    // Sets canAttack to true at the end of the cooldown
    void EndAttackCooldown()
    {
        canAttack = true;
        eventManager.Publish(EventType.AttackOff);
    }

    // Sets canUseInventory to true at the end of the cooldown
    void EndInventoryCooldown()
    {
        canUseInventory = true;
    }

    // Sets canUseProjectile to true at the end of the cooldown
    void EndProjectileCooldown()
    {
        canUseProjectile = true;
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
    #endregion

    #region Events
    public void OnAttackHandler(object target)
    {
        // Attempt to attack if no cooldown. (For now just shovel attack)
        if (canAttack && target is Vector3 mousePos)
        {
            ShovelAttack(CalculateDirection(this.transform.position, Camera.main.ScreenToWorldPoint(mousePos)));
        }
    }

    // Called when Fire action is performed
    public void OnFireHandler(object target)
    {
        Inventory inventory = playerScript.PlayerInventory;
        Item usedItem = inventory.GetCurrentItem();

        //Make sure usedItem isn't null
        //Check if input is a valid Vector3 (Should always be provided)
        if (target is Vector3 mousePos && usedItem != null)
        {
            // If usedItem is a usable item and can be used right now
            if (usedItem is AmmoItem usedAmmo && canUseProjectile)
            {
                UseProjectileItem(inventory, usedAmmo, mousePos);
            }
            else if (usedItem is SeedItem usedSeed && canUseInventory)
            {
                UsePlantItem(inventory, usedItem, usedSeed, mousePos);
            }
            else if(canUseInventory && usedItem is HealItem usedHeal)
            {
                UseHealItem(inventory, usedItem, usedHeal);
            }
            else if (usedItem is BossItem usedBossItem)
            {
                UseBossItem(usedBossItem);
            }
        }
    }
    #endregion

    /// <summary>
    /// Handles logic for using a boss item. Note that boss items are NOT consumed on usage.
    /// </summary>
    /// <param name="usedBossItem"></param>
    private void UseBossItem(BossItem usedBossItem)
    {
        // Switch statement that calls methods for handling boss items by 
        switch (usedBossItem.bossItemType)
        {
            case BossEnum.Boss1:
                UseShield();
                break;
        }
    }

    /// <summary>
    /// Handles planting a seed item. Only consumes item if able to plant
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="usedItem"></param>
    /// <param name="usedSeed"></param>
    /// <param name="mousePos"></param>
    /// <returns></returns>
    private Item UsePlantItem(Inventory inventory, Item usedItem, SeedItem usedSeed, Vector3 mousePos)
    {
        //If able to plant seed at mouseposition then consume the item
        if (ItemUse.PlantSeed(usedSeed, mousePos))
        {
            usedItem = inventory.UseItem();
            canUseInventory = false;
            inventoryCooldownTimer.Run();
        }

        return usedItem;
    }

    /// <summary>
    /// Handles logic for using a heal item. Only consumes item if it does something.
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="usedItem"></param>
    /// <param name="usedHeal"></param>
    /// <returns></returns>
    private Item UseHealItem(Inventory inventory, Item usedItem, HealItem usedHeal)
    {
        HealthScript healthScript = GetComponent<HealthScript>();
        AudioManager.Play(AudioClipName.sfx_player_eatitem, loop: false, AudioType.SFX, gameObject, false);
        // Attempt to Heal. If returned value (amount healed) is 0 then do not consume heal item
        int actualHeal = healthScript.TakeDamage(-usedHeal.healAmount);

        //Only consume item if the heal actually worked
        if (actualHeal != 0)
        {
            usedItem = inventory.UseItem();

            // Item was used
            canUseInventory = false;
            inventoryCooldownTimer.Run();

        }

        return usedItem;
    }

    /// <summary>
    /// Handles logic for using a projectile item. Only consumes ammo when used correctly.
    /// </summary>
    /// <param name="inventory"></param>
    /// <param name="usedAmmo"></param>
    /// <param name="mousePos"></param>
    private void UseProjectileItem(Inventory inventory, AmmoItem usedAmmo, Vector3 mousePos)
    {
        inventory.UseItem();
        AudioManager.Play(AudioClipName.sfx_player_rangedattack, loop: false, AudioType.SFX, null, false);
        ItemUse.UseAmmo(usedAmmo, mousePos, this.transform);
        Instantiate(Projectileparticle, transform.position, Quaternion.identity);

        //Ammo Used
        canUseProjectile = false;
        projectileCooldownTimer.Run();
    }
    private void UseShield()
    {
        if (canShield)
        {
            canShield = false;
            Instantiate(shieldPrefab, this.transform);
            attackTimer.Run();
        }
    }
}

