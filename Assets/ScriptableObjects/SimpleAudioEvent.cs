using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using GD.MinMaxSlider;

[CreateAssetMenu(menuName = "AudioEvents/Simple") ]
public class SimpleAudioEvent : AudioEvent
{
    public AudioClip[] clips;

    [MinMaxSlider(0, 1)]
    public Vector2 volume = new Vector2(0f, 1f);

    [MinMaxSlider(-3, 3)]
    public Vector2 pitch = new Vector2(-0.5f, 0.5f);
    

    public override void Play(AudioSource source)
    {
        if (clips.Length == 0) return;

        source.clip = clips[Random.Range(0, clips.Length)];
        source.volume = Random.Range(volume.x, volume.y);
        source.pitch = Random.Range(pitch.x, pitch.y);
        source.Play();
    }
}
