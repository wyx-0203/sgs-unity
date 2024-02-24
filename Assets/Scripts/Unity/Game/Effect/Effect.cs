using Spine.Unity;
using UnityEngine;

[RequireComponent(typeof(SkeletonGraphic))]
public class Effect : MonoBehaviour
{
    public AudioClip audioClip;
    public bool autoDestroyed = true;

    private void Start()
    {
        if (audioClip != null) AudioManager.Instance.effect.PlayOneShot(audioClip);
        if (autoDestroyed) GetComponent<SkeletonGraphic>().AnimationState.Complete += x => Destroy(gameObject);
    }
}
