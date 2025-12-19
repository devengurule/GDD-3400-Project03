using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// A container for the configuration data
/// </summary>
public class ConfigurationData
{
    #region Fields

    const string ConfigurationDataFileName = "ReapAndSowData.csv";

    Dictionary<ConfigurationDataValueName, float> values = 
        new Dictionary<ConfigurationDataValueName, float>();

    #endregion

    #region Properties

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int direction
    {
        get { return (int)values[ConfigurationDataValueName.direction]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int enemySpeed
    {
        get { return (int)values[ConfigurationDataValueName.enemySpeed]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int speed
    {
        get { return (int)values[ConfigurationDataValueName.speed]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float minDistance
    {
        get { return values[ConfigurationDataValueName.minDistance]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float maxDistance
    {
        get { return values[ConfigurationDataValueName.maxDistance]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float respawnTimer
    {
        get { return values[ConfigurationDataValueName.respawnTimer]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float respawnDelay
    {
        get { return values[ConfigurationDataValueName.respawnDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public int lungeSpeed
    {
        get { return (int)values[ConfigurationDataValueName.lungeSpeed]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float lungeDelay
    {
        get { return values[ConfigurationDataValueName.lungeDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float lungeDuration
    {
        get { return values[ConfigurationDataValueName.lungeDuration]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float lastLungeTime
    {
        get { return values[ConfigurationDataValueName.lastLungeTime]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackDuration
    {
        get { return values[ConfigurationDataValueName.meleeAttackDuration]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackDelay
    {
        get { return values[ConfigurationDataValueName.meleeAttackDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackRange
    {
        get { return values[ConfigurationDataValueName.meleeAttackRange]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeHitboxSize
    {
        get { return values[ConfigurationDataValueName.meleeHitboxSize]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackDamage
    {
        get { return values[ConfigurationDataValueName.meleeAttackDamage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackCooldown
    {
        get { return values[ConfigurationDataValueName.meleeAttackCooldown]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float meleeAttackStun
    {
        get { return values[ConfigurationDataValueName.meleeAttackStun]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float pushDamage
    {
        get { return values[ConfigurationDataValueName.pushDamage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float pushForce
    {
        get { return values[ConfigurationDataValueName.pushForce]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float bulletDamage
    {
        get { return values[ConfigurationDataValueName.bulletDamage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float maxRange
    {
        get { return values[ConfigurationDataValueName.maxRange]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float fireRateDelay
    {
        get { return values[ConfigurationDataValueName.fireRateDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float initialFireDelay
    {
        get { return values[ConfigurationDataValueName.initialFireDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float animationDelayTime
    {
        get { return values[ConfigurationDataValueName.animationDelayTime]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float seedlingTimer
    {
        get { return values[ConfigurationDataValueName.seedlingTimer]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float youngTimer
    {
        get { return values[ConfigurationDataValueName.youngTimer]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float adultTimer
    {
        get { return values[ConfigurationDataValueName.adultTimer]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float harvestSpreadRadius
    {
        get { return values[ConfigurationDataValueName.harvestSpreadRadius]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float typeSpeed
    {
        get { return values[ConfigurationDataValueName.typeSpeed]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float minDisplayTime
    {
        get { return values[ConfigurationDataValueName.minDisplayTime]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float defaultDamage
    {
        get { return values[ConfigurationDataValueName.defaultDamage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float totalSeconds
    {
        get { return values[ConfigurationDataValueName.totalSeconds]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float elapsedSeconds
    {
        get { return values[ConfigurationDataValueName.elapsedSeconds]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float imageIndex
    {
        get { return values[ConfigurationDataValueName.imageIndex]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float totalDurationForDay
    {
        get { return values[ConfigurationDataValueName.totalDurationForDay]; }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float gameTimer
    {
        get { return values[ConfigurationDataValueName.gameTimer]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float explosionDelay
    {
        get { return values[ConfigurationDataValueName.explosionDelay]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float explosiveDamage
    {
        get { return values[ConfigurationDataValueName.explosiveDamage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float explosionDuration
    {
        get { return values[ConfigurationDataValueName.explosionDuration]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float timeUntilExplode
    {
        get { return values[ConfigurationDataValueName.timeUntilExplode]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float arraySize
    {
        get { return values[ConfigurationDataValueName.arraySize]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float currentIndex
    {
        get { return values[ConfigurationDataValueName.currentIndex]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float time
    {
        get { return values[ConfigurationDataValueName.time]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float aoe
    {
        get { return values[ConfigurationDataValueName.aoe]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float damage
    {
        get { return values[ConfigurationDataValueName.damage]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float healAmount
    {
        get { return values[ConfigurationDataValueName.healAmount]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float healthCap
    {
        get { return values[ConfigurationDataValueName.healthCap]; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <value></value>
    public float health
    {
        get { return values[ConfigurationDataValueName.health]; }
    }

    #endregion

    #region Constructor

    /// <summary>
    /// Constructor
    /// Reads configuration data from a file. If the file
    /// read fails, the object contains default values for
    /// the configuration data
    /// </summary>
    public ConfigurationData()
    {
        // read and save configuration data from file
        StreamReader input = null;
        try
        {
            // create stream reader object
            input = File.OpenText(Path.Combine(
                Application.streamingAssetsPath, ConfigurationDataFileName));

            // read in names and values
            string currentLine = input.ReadLine();
            while (currentLine != null)
            {
                // gets the name of vale in our csv file and adds it to the dictionary
                string[] tokens = currentLine.Split(",");
                ConfigurationDataValueName valueName =
                    (ConfigurationDataValueName)Enum.Parse(
                        typeof(ConfigurationDataValueName), tokens[0]);
                values.Add(valueName, float.Parse(tokens[1])); ;
                currentLine = input.ReadLine();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Configuration Exception: {e.Message}, setting default values.");

            // set defualt values if something went wrong
            SetDefaultValues();
        }
        finally
        {
            // always close input file
            if (input != null)
            {
                input.Close();
            }
        }
    }

	#endregion

	#region SetDataFields

	/// <summary>
	/// Sets the configuration data fields to default values
	/// </summary>
	void SetDefaultValues()
    {
        // clear the dictionary and add default values
        values.Clear();
        values.Add(ConfigurationDataValueName.direction, 0);
        values.Add(ConfigurationDataValueName.enemySpeed, 0);
        values.Add(ConfigurationDataValueName.speed, 0);
        values.Add(ConfigurationDataValueName.minDistance, 0);
        values.Add(ConfigurationDataValueName.maxDistance, 0);
        values.Add(ConfigurationDataValueName.respawnTimer, 0);
        values.Add(ConfigurationDataValueName.respawnDelay, 0);
        values.Add(ConfigurationDataValueName.lungeSpeed, 0);
        values.Add(ConfigurationDataValueName.lungeCooldown, 0);
        values.Add(ConfigurationDataValueName.lungeDelay, 0);
        values.Add(ConfigurationDataValueName.lungeDuration, 0);
        values.Add(ConfigurationDataValueName.lastLungeTime, 0);
        values.Add(ConfigurationDataValueName.meleeAttackDuration, 0);
        values.Add(ConfigurationDataValueName.meleeAttackDelay, 0);
        values.Add(ConfigurationDataValueName.meleeAttackRange, 0);
        values.Add(ConfigurationDataValueName.meleeHitboxSize, 0);
        values.Add(ConfigurationDataValueName.meleeAttackDamage, 0);
        values.Add(ConfigurationDataValueName.meleeAttackCooldown, 0);
        values.Add(ConfigurationDataValueName.meleeAttackStun, 0);
        values.Add(ConfigurationDataValueName.pushDamage, 0);
        values.Add(ConfigurationDataValueName.pushForce, 0);
        values.Add(ConfigurationDataValueName.bulletDamage, 0);
        values.Add(ConfigurationDataValueName.maxRange, 0);
        values.Add(ConfigurationDataValueName.fireRateDelay, 0);
        values.Add(ConfigurationDataValueName.initialFireDelay, 0);
        values.Add(ConfigurationDataValueName.animationDelayTime, 0);
        values.Add(ConfigurationDataValueName.seedlingTimer, 0);
        values.Add(ConfigurationDataValueName.youngTimer, 0);
        values.Add(ConfigurationDataValueName.adultTimer, 0);
        values.Add(ConfigurationDataValueName.harvestSpreadRadius, 0);
        values.Add(ConfigurationDataValueName.typeSpeed, 0);
        values.Add(ConfigurationDataValueName.minDisplayTime, 0);
        values.Add(ConfigurationDataValueName.defaultDamage, 0);
        values.Add(ConfigurationDataValueName.totalSeconds, 0);
        values.Add(ConfigurationDataValueName.elapsedSeconds, 0);
        values.Add(ConfigurationDataValueName.imageIndex, 0);
        values.Add(ConfigurationDataValueName.totalDurationForDay, 0);
        values.Add(ConfigurationDataValueName.gameTimer, 0);
        values.Add(ConfigurationDataValueName.explosionDelay, 0);
        values.Add(ConfigurationDataValueName.explosiveDamage, 0);
        values.Add(ConfigurationDataValueName.explosionDuration, 0);
        values.Add(ConfigurationDataValueName.timeUntilExplode, 0);
        values.Add(ConfigurationDataValueName.arraySize, 0);
        values.Add(ConfigurationDataValueName.currentIndex, 0);
        values.Add(ConfigurationDataValueName.time, 0);
        values.Add(ConfigurationDataValueName.aoe, 0);
        values.Add(ConfigurationDataValueName.damage, 0);
        values.Add(ConfigurationDataValueName.healAmount, 0);
        values.Add(ConfigurationDataValueName.healthCap, 0);
        values.Add(ConfigurationDataValueName.health, 0);
    }

	#endregion
}
