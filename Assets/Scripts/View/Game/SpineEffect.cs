using System;
using Spine;
using Spine.Unity;
using UnityEngine;

public class SpineEffect : SkeletonGraphic
{
    public AudioClip audioClip;

    protected override void Start()
    {
        base.Start();
        if (audioClip != null) Audio.Instance.effect.PlayOneShot(audioClip);
        AnimationState.Complete += x => Destroy(gameObject);
    }
}
