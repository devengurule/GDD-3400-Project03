using UnityEngine;

public class AudioListenerChecker : MonoBehaviour
{
    [Tooltip("Check interval in seconds")]
    [SerializeField] private float checkInterval = 2f;

    private AudioListener myListener;

    private void Awake()
    {
        // Ensure this object has an AudioListener component
        myListener = GetComponent<AudioListener>();
        if (myListener == null)
        {
            myListener = gameObject.AddComponent<AudioListener>();
            myListener.enabled = false; // Start disabled
        }

        // Start the periodic check
        InvokeRepeating(nameof(CheckForAudioListener), 0f, checkInterval);
    }

    private void CheckForAudioListener()
    {
        // Check if any enabled AudioListener exists in the scene
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();
        bool anyEnabled = false;

        foreach (var listener in listeners)
        {
            if (listener.enabled)
            {
                anyEnabled = true;
                break;
            }
        }

        // If none are enabled, enable this object's AudioListener
        if (!anyEnabled)
        {
            myListener.enabled = true;
            Debug.Log("No active AudioListener found, enabling listener on " + gameObject.name);
        }
        else
        {
            // Optional: keep your listener disabled if another exists
            myListener.enabled = false;
        }
    }

    private void OnDestroy()
    {
        CancelInvoke(nameof(CheckForAudioListener));
    }
}
