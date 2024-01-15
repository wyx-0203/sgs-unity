using UnityEngine;
using System.Collections;

public class BGM : GlobalSingletonMono<BGM>
{
    private AudioSource audioSource;
    private string url;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.4f;
#if !UNITY_WEBGL
        audioSource.loop = true;
#endif
    }

    public void Load(AudioClip clip)
    {
        if (audioSource.clip == clip) return;
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();
        // this.url = url;

        // #if !UNITY_WEBGL
        // #else
        //             StopAllCoroutines();
        //             StartCoroutine(LoopPlay());
        // #endif
    }

    private IEnumerator LoopPlay()
    {
        while (true)
        {
            audioSource.Play();
            yield return new WaitForSeconds(audioSource.clip.length);
        }
    }

    public void Stop() => audioSource.Stop();
}