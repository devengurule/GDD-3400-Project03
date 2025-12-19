using UnityEngine;
using UnityEngine.UI;

public class AudioMixerscript : MonoBehaviour
{
    public Slider musicVolumeSlider;
    public Slider ambientVolumeSlider;
    public Slider sfxVolumeSlider;

    void Start()
    {
        // this just Initialize sliders with current volume
        musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
        ambientVolumeSlider.onValueChanged.AddListener(UpdateAmbientVolume);
        sfxVolumeSlider.onValueChanged.AddListener(UpdateSFXVolume);
    }

    // this is just called when the music volume slider changes
    public void UpdateMusicVolume(float value)
    {
        AudioManager.SetVolume("Music", value); 
    }

    // this is just called when the ambient volume slider changes
    public void UpdateAmbientVolume(float value)
    {
        AudioManager.SetVolume("Ambient", value);
    }

    // this is just called when the SFX volume slider changes
    public void UpdateSFXVolume(float value)
    {
        AudioManager.SetVolume("SFX", value);

    }
}
