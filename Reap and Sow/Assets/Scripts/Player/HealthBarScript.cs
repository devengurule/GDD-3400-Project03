using UnityEngine;
using UnityEngine.UI;
using System;


public class HealthBarScript : MonoBehaviour
{
    #region Sprites and Variables
    public Sprite health0;
    public Sprite health1;
    public Sprite health2;
    public Sprite health3;
    public Sprite health4;
    public Sprite health5;
    public Sprite health6;
    public Sprite health7;
    public Sprite health8;


    public Sprite[] healthSprites;

    private Image healthRenderer;
    #endregion


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        healthRenderer = GetComponent<Image>();
        healthSprites = new Sprite[] {health8,health7,health6,health5, health4, health3, health2, health1, health0};

        GameController.instance.EventManager.Subscribe(EventType.PlayerDamaged, OnPlayerDamagedHandler);
    }

    private void OnDestroy()
    {
        GameController.instance.EventManager.Unsubscribe(EventType.PlayerDamaged, OnPlayerDamagedHandler);
    }

    // Method that updates the health bar to reflect player health
    public void HeartChange(int health, int healthCap)
    {
       if (gameObject != null && healthSprites.Length > 0)
        {
            //limit health display to the possible bounds of healthsprites
            healthCap = Mathf.Clamp(healthCap, 0, healthSprites.Length - 1);

            //limit health value to between 0 and healthcap
            health = Mathf.Clamp(health, 0, healthCap);

            // index according to health value
            healthRenderer.sprite = healthSprites[health];
        }
    }

    // Method that resets the health of the player to full
    public void ResetHealth(int health)
    {
        HeartChange(health,health);
    }


    public void OnPlayerDamagedHandler(object data = null)
    {
        //incoming data should be passed as (damage, currenthealth, healthCap) is (int, int , int)
        if (data is ValueTuple<int, int, int> t)
        {
            int health = t.Item2;
            int maxhealth = t.Item3;
            HeartChange(health, maxhealth);
        }
    }
}

