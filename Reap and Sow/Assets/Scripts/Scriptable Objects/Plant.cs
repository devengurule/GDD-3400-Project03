using UnityEngine;

/// <summary>
/// Contains the information for plants.
/// </summary>
[CreateAssetMenu(fileName = "Plant", menuName = "Plant")]
public class Plant : ScriptableObject
{
    public Sprite seedlingSprite;
    public Sprite youngSprite;
    public Sprite adultSprite;
    public float seedlingTimer;
    public float youngTimer;
    public float adultTimer;
    public Item drop;
 }
