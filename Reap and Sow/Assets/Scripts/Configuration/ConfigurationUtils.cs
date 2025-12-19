/// <summary>
/// Provides access to configuration data
/// </summary>
public static class ConfigurationUtils
{
    #region Fields

    // makes an object for our config data
    static ConfigurationData configurationData;

    #endregion

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static int direction
    {
        get { return (int)configurationData.direction; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static int enemySpeed
    {
        get { return (int)configurationData.enemySpeed; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static int speed
    {
        get { return (int)configurationData.speed; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float minDistance
    {
        get { return configurationData.minDistance; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float maxDistance
    {
        get { return configurationData.maxDistance; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float respawnTimer
    {
        get { return configurationData.respawnTimer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float respawnDelay
    {
        get { return configurationData.respawnDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static int lungeSpeed
    {
        get { return (int)configurationData.lungeSpeed; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float lungeDelay
    {
        get { return configurationData.lungeDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float lungeDuration
    {
        get { return configurationData.lungeDuration; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float lastLungeTime
    {
        get { return configurationData.lastLungeTime; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackDuration
    {
        get { return configurationData.meleeAttackDuration; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackDelay
    {
        get { return configurationData.meleeAttackDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackRange
    {
        get { return configurationData.meleeAttackRange; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeHitboxSize
    {
        get { return configurationData.meleeHitboxSize; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackDamage
    {
        get { return configurationData.meleeAttackDamage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackCooldown
    {
        get { return configurationData.meleeAttackCooldown; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float meleeAttackStun
    {
        get { return configurationData.meleeAttackStun; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float pushDamage
    {
        get { return configurationData.pushDamage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float pushForce
    {
        get { return configurationData.pushForce; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float bulletDamage
    {
        get { return configurationData.bulletDamage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float maxRange
    {
        get { return configurationData.maxRange; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float fireRateDelay
    {
        get { return configurationData.fireRateDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float initialFireDelay
    {
        get { return configurationData.initialFireDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float animationDelayTime
    {
        get { return configurationData.animationDelayTime; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float seedlingTimer
    {
        get { return configurationData.seedlingTimer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float youngTimer
    {
        get { return configurationData.youngTimer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float adultTimer
    {
        get { return configurationData.adultTimer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float harvestSpreadRadius
    {
        get { return configurationData.harvestSpreadRadius; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float typeSpeed
    {
        get { return configurationData.typeSpeed; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float minDisplayTime
    {
        get { return configurationData.minDisplayTime; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float defaultDamage
    {
        get { return configurationData.defaultDamage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float totalSeconds
    {
        get { return configurationData.totalSeconds; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float elapsedSeconds
    {
        get { return configurationData.elapsedSeconds; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float imageIndex
    {
        get { return configurationData.imageIndex; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float totalDurationForDay
    {
        get { return configurationData.totalDurationForDay; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float gameTimer
    {
        get { return configurationData.gameTimer; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float explosionDelay
    {
        get { return configurationData.explosionDelay; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float explosiveDamage
    {
        get { return configurationData.explosiveDamage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float explosionDuration
    {
        get { return configurationData.explosionDuration; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float timeUntilExplode
    {
        get { return configurationData.timeUntilExplode; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float arraySize
    {
        get { return configurationData.arraySize; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float currentIndex
    {
        get { return configurationData.currentIndex; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float time
    {
        get { return configurationData.time; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float aoe
    {
        get { return configurationData.aoe; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float damage
    {
        get { return configurationData.damage; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float healAmount
    {
        get { return configurationData.healAmount; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float healthCap
    {
        get { return configurationData.healthCap; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public static float health
    {
        get { return configurationData.health; }
    }

    #endregion

    #region Methods
    /// <summary>
    /// Initializes the configuration utils
    /// </summary>
    public static void Initialize()
    {
        configurationData = new ConfigurationData();
    }
	#endregion
}
