using UnityEngine;

public class PersistantMenuScript : MonoBehaviour
{
    private bool loaded = false;

    void OnEnable()
    {
        if (loaded == false)
        {
            loaded = true;
            gameObject.SetActive(false);
        }
    }
}
