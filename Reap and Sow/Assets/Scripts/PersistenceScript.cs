using UnityEngine;

/// <summary>
/// This script is added to any game object that should be persistent through scene changes
/// </summary>
public class PersistenceScript : MonoBehaviour
{
    #region Fields
    // Static dictionary to track instances per id
    private static readonly System.Collections.Generic.Dictionary<string, PersistenceScript> instancesPerID
        = new System.Collections.Generic.Dictionary<string, PersistenceScript>();

    [SerializeField] private string ID; // Assign a unique ID for this object


    #endregion

    /// <summary>
    /// Unity calls Awake when an enabled script instance is being loaded.
    /// </summary>
    private void Awake()
    {
        //delete if duplicate
        if (string.IsNullOrEmpty(ID))
        {
            Debug.LogError("PersistenceScript requires a roomID!");
            return;
        }
        if (instancesPerID.ContainsKey(ID))
        {
            Destroy(gameObject); // Another instance exists for this room
        }
        else
        {
            instancesPerID[ID] = this;
            DontDestroyOnLoad(gameObject); // Keep this object persistent
        }
    }

    private void OnDestroy()
    {
        if (!string.IsNullOrEmpty(ID))
        {
            if (instancesPerID.ContainsKey(ID) && instancesPerID[ID] == this)
            {
                instancesPerID.Remove(ID);
            }
        }
    }
}
