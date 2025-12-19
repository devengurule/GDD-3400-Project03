using UnityEngine;
[System.Serializable]

public class SaveData
{
    ///public Vector2 playerLocation;
    //public string playerRoom;
    public int playerHealth;

    public string[] inventoryItems;  // Array of item names or IDs
    public int[] inventoryAmounts;   // Array of item amounts
    public Vector2 plotLocation;

    public bool roomLocked;

    public BossEnum[] bossCompleted;
}
