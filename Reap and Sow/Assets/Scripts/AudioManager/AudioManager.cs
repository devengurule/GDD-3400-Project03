using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class AudioManager
{
    #region Fields
    public static List<AudioSource> audioSources = new List<AudioSource>();  // List to hold all audio sources
    static AudioMixer audioMixer;
    static Dictionary<AudioClipName, AudioClip> audioClips = new Dictionary<AudioClipName, AudioClip>();
    static Dictionary<AudioType, string> audioTypes = new Dictionary<AudioType, string>();
    static bool audioClipsLoaded = false;
    #endregion

    public static void Initialize(AudioMixer mixer)
    {
        audioMixer = mixer;

        if (audioClipsLoaded == false)
        {
            // this is loading the audio clips relative to the Resources folder
            audioClips.Add(AudioClipName.None, Resources.Load<AudioClip>("Audio/Music/"));
            audioClips.Add(AudioClipName.mus_Core, Resources.Load<AudioClip>("Audio/Music/Mus_ReapandSow_Core_loop"));
            audioClips.Add(AudioClipName.mus_slow, Resources.Load<AudioClip>("Audio/Music/Mus_ReapandSow_Slow_loop"));
            audioClips.Add(AudioClipName.mus_sweetlysoft, Resources.Load<AudioClip>("Audio/Music/Mus_ReapandSow_Sweetlysoft_loop"));
            audioClips.Add(AudioClipName.mus_lost, Resources.Load<AudioClip>("Audio/Music/Mus_ReapandSow_Lost_loop"));
            audioClips.Add(AudioClipName.mus_wander, Resources.Load<AudioClip>("Audio/Music/Mus_ReapandSow_Secondareawandermusic_loop"));
            audioClips.Add(AudioClipName.mus_boss2, Resources.Load<AudioClip>("Audio/Music/Mus_Bossmusicworld2"));
            audioClips.Add(AudioClipName.mus_boss3, Resources.Load<AudioClip>("Audio/Music/Mus_Bossmusicworld3"));
            audioClips.Add(AudioClipName.mus_missing, Resources.Load<AudioClip>("Audio/Music/mus_missing"));
            audioClips.Add(AudioClipName.sfx_player_ShovelSwing, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_Swingshovel_8bit_Attack"));
            audioClips.Add(AudioClipName.sfx_player_Walking, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_walk_8bit_movement"));
            audioClips.Add(AudioClipName.sfx_player_harvestplant, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_Plant Harvest_Plants"));
            audioClips.Add(AudioClipName.sfx_player_rangedattack, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_ThrowItem_player"));
            audioClips.Add(AudioClipName.sfx_player_shoveldigging, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_Shoveldigging_8bit_harvest"));
            audioClips.Add(AudioClipName.sfx_player_eatitem, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_eatitem_player"));
            audioClips.Add(AudioClipName.sfx_player_hit, Resources.Load<AudioClip>("Audio/SFX/Player/sfx_Player_hit_player"));
            audioClips.Add(AudioClipName.sfx_enemy_thornattack, Resources.Load<AudioClip>("Audio/SFX/Enemies/Thornplant/sfx_Thorn_Plant_Attack_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_thornidle, Resources.Load<AudioClip>("Audio/SFX/Enemies/sfx_Thorn Plant Idle_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_thornwalk, Resources.Load<AudioClip>("Audio/SFX/Enemies/Thornplant/sfx_ThornPlantWalking_Enemies"));
            audioClips.Add(AudioClipName.sfx_enemy_thornhit, Resources.Load<AudioClip>("Audio/SFX/Enemies/Thornplant/sfx_ThornTakesDamage_Enemies"));
            audioClips.Add(AudioClipName.sfx_enemy_pinetree_explode, Resources.Load<AudioClip>("Audio/SFX/Enemies/pinetreegrenade/sfx_Pine Tree Grenadier_explode"));
            audioClips.Add(AudioClipName.sfx_enemy_pinetree_throw, Resources.Load<AudioClip>("Audio/SFX/Enemies/pinetreegrenade/sfx_Pine Tree Grenadier_throw"));
            audioClips.Add(AudioClipName.sfx_enemy_pouncer_idle, Resources.Load<AudioClip>("Audio/SFX/Enemies/pouncer/sfx_Pouncer_idle_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_pouncer_scurring, Resources.Load<AudioClip>("Audio/SFX/Enemies/pouncer/sfx_pouncer_scurring_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_pouncer_hit, Resources.Load<AudioClip>("Audio/SFX/Enemies/pouncer/sfx_pouncer_hit_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_slimewalk, Resources.Load<AudioClip>("Audio/SFX/Enemies/slime/sfx_slimewalking_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_slimehit, Resources.Load<AudioClip>("Audio/SFX/Enemies/slime/sfx_SlimeTakesDamage_Enemies"));
            audioClips.Add(AudioClipName.sfx_enemy_stalagopede_dig, Resources.Load<AudioClip>("Audio/SFX/Enemies/Stalagopede/sfx_Stalagopede_digging_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_stalagopede_headground, Resources.Load<AudioClip>("Audio/SFX/Enemies/Stalagopede/sfx_Stalagopede_headground_Enemy"));
            audioClips.Add(AudioClipName.sfx_enemy_treetrap_attack, Resources.Load<AudioClip>("Audio/SFX/Enemies/treethrowtrap/sfx_TreeTrap_attack_Enemy"));
            audioClips.Add(AudioClipName.sfx_Ui_grab, Resources.Load<AudioClip>("Audio/SFX/Ui/sfx_Grab_inventory"));
            audioClips.Add(AudioClipName.sfx_Ui_press, Resources.Load<AudioClip>("Audio/SFX/Ui/sfx_Presssounds_Ui"));
            audioClips.Add(AudioClipName.sfx_Ui_switch, Resources.Load<AudioClip>("Audio/SFX/Ui/sfx_Switch__inventory"));
            audioClips.Add(AudioClipName.sfx_Boss_Burrow_Spike, Resources.Load<AudioClip>("Audio/SFX/Enemies/CorpseBoss/sfx_Boss_Burrow_Spike"));
            audioClips.Add(AudioClipName.sfx_Boss_Fire_Wall, Resources.Load<AudioClip>("Audio/SFX/Enemies/CorpseBoss/sfx_Boss_Fire_Wall"));
            audioClips.Add(AudioClipName.sfx_CorpseTreeBossIdle, Resources.Load<AudioClip>("Audio/SFX/Enemies/CorpseBoss/sfx_CorpseTreeBossIdle"));
            audioClips.Add(AudioClipName.sfx_firelog_idle, Resources.Load<AudioClip>("Audio/SFX/Enemies/Firelog/sfx_FireLogIdle"));
            audioClips.Add(AudioClipName.sfx_firelog_fire, Resources.Load<AudioClip>("Audio/SFX/Enemies/Firelog/sfx_firelog_fire"));
            audioClips.Add(AudioClipName.sfx_ant_walk, Resources.Load<AudioClip>("Audio/SFX/Enemies/FireAnts/sfx_Fireants_walking"));
            audioClips.Add(AudioClipName.Sfx_Corpsebloom_Fire, Resources.Load<AudioClip>("Audio/SFX/Enemies/Corpse Flower/Sfx_Corpsebloom_Fire"));
            audioClips.Add(AudioClipName.Sfx_Corpsebloom_GasCloud, Resources.Load<AudioClip>("Audio/SFX/Enemies/Corpse Flower/Sfx_Corpsebloom_GasCloud"));
            audioClips.Add(AudioClipName.Sfx_Corpsebloom_Idle, Resources.Load<AudioClip>("Audio/SFX/Enemies/Corpse Flower/Sfx_Corpsebloom_Idle"));
            audioClips.Add(AudioClipName.sfx_GasCloud, Resources.Load<AudioClip>("Audio/SFX/Enemies/Corpse Flower/sfx_GasCloud"));
            audioClips.Add(AudioClipName.Sfx_CordycepsFire, Resources.Load<AudioClip>("Audio/SFX/Enemies/Cordycep/Sfx_CordycepsBase"));

            audioTypes.Add(AudioType.SFX, "SFX");
            audioTypes.Add(AudioType.Music, "Music");
            audioTypes.Add(AudioType.Dialogue, "Dialogue");
            audioClipsLoaded = true;
            
        }



        // this just checking to see if we have an audio mixer or not 
        if (audioMixer == null)
        {
            Debug.LogError("Audio Mixer is not assigned.");
        }
    }


    /// <summary>
    ///  this is just stopping the sound by AudioClip name
    /// </summary>
    /// <param name="name">this is grabbing audio clip name to stop sound</param>
    public static void StopLoopedSoundByName(AudioClipName name)
    {
        if (name == AudioClipName.None)
        {

        }
        else
        {
            // this is find the source playing the specific clip name and stopping it
            for (int i = audioSources.Count - 1; i >= 0; i--)
            {
                var source = audioSources[i];
                if (source == null)
                {
                    audioSources.RemoveAt(i);
                }
                else if (source.clip == audioClips[name])
                {
                    // this is removing the source audio from the array and also destroy the source object.
                    source.Stop();
                    audioSources.RemoveAt(i);
                    Object.Destroy(source); // only destroy the AudioSource component
                    break;
                }
            }
        }
       
    }


    /// <summary>
    /// this is starting to play the sounds through audio clip names 
    /// </summary>
    /// <param name="name">this is our audio clip names being set</param>
    /// <param name="loop">this is checking if its a loop or not</param>
    public static void Play(AudioClipName name, bool loop = false, AudioType audio = AudioType.SFX, GameObject currentObject = null, bool is3D = false)
    {
        // this is checking if the 
        if (name != AudioClipName.None && audioClips.TryGetValue(name, out var clip) && clip != null)
        {
            // this is setting the new audio source into the array and also crearing a new audio source.
            AudioSource source = CreateNewAudioSource(audioTypes[audio], currentObject, is3D);
            if (source != null)
            {
                // this is setting the source file by the params 
                source.loop = loop;
                source.clip = audioClips[name];
                source.Play();
                //Object.DontDestroyOnLoad(source.gameObject);

                // Debug.Log($"Playing sound: {name}");
                // this is checking if the sound is not looping and starting the coroutine to destroy the AudioSource when finished
                if (!loop)
                {
                    AudioCoroutine audioCoroutine = source.gameObject.GetComponent<AudioCoroutine>();
                    if (audioCoroutine == null)
                    {
                        audioCoroutine = source.gameObject.AddComponent<AudioCoroutine>();
                    }

                    // this is starting a coroutine to stop/destroy safely
                    audioCoroutine.StartCoroutine(audioCoroutine.HandleAudioDelete(source, currentObject == null));
                }
            }
            else
            {
                Debug.LogWarning("Unable to create a new AudioSource.");
            }
        }
    }



    /// <summary>
    /// this checks to create a new AudioSource dynamically
    /// </summary>
    /// <returns>a new source value</returns>
    private static AudioSource CreateNewAudioSource(string types, GameObject currentObject = null, bool is3D = true)
    {
        GameObject audioObject;

        if (currentObject != null)
        {
            audioObject = currentObject;  // attach to provided object
        }
        else
        {
            audioObject = new GameObject("AudioSource_" + audioSources.Count);  // Create a new GameObject for the AudioSource
        }

        AudioSource newSource = audioObject.AddComponent<AudioSource>();  // Add the AudioSource component
        newSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups(types)[0];  // Assign the AudioMixer group

        // set 2D or 3D mode
        newSource.spatialBlend = is3D ? 1.0f : 0.0f;

        // Add the new AudioSource to the list for management
        audioSources.Add(newSource);
        return newSource;
    }


    /// <summary>
    /// this is setting the mixer to a new volume value
    /// </summary>
    /// <param name="groupName"></param>
    /// <param name="volume"></param>
    public static void SetVolume(string groupName, float volume)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(groupName, volume);
        }
        else
        {
            Debug.LogError("AudioMixer is not assigned!");
        }
    }
}