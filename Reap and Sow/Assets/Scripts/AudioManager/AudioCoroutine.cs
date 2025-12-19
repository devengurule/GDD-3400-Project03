using System.Collections;
using UnityEngine;

public class AudioCoroutine : MonoBehaviour
{
    // Coroutine to delete AudioSource when the sound is finished
    public IEnumerator HandleAudioDelete(AudioSource source, bool isTempObject)
    {

        // wait until the clip finishes
        yield return new WaitWhile(() => source != null && source.isPlaying);

        if (source != null)
        {
            if (isTempObject)
            {
                // Only destroy if we spawned a temporary GameObject
                Object.Destroy(source.gameObject);
            }
            else
            {
                // If it's on an existing object, just stop and remove the clip
                source.Stop();
                source.clip = null;
                DestroyImmediate(source, true);
            }
        }
    }
}
