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

    public async void Load(string url)
    {
        if (this.url == url) return;
        audioSource.Stop();
        audioSource.clip = await WebRequest.GetClip(url);
        this.url = url;

#if !UNITY_WEBGL
        audioSource.Play();
#else
            StopAllCoroutines();
            StartCoroutine(LoopPlay());
#endif
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