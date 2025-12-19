using UnityEngine;
using UnityEngine.Audio;

public class GameAudioSource : MonoBehaviour
{
    public AudioMixer audioMixer;

    void Awake()
    {
        AudioSource audioSource = gameObject.AddComponent<AudioSource>();
        if (audioMixer != null)
        {
            AudioManager.Initialize(audioMixer);
        }
        else
        {
            Debug.LogError("AudioMixer not assigned!");
        }
    }
}
