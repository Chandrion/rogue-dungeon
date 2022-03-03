using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static readonly float HEAR_DISTANCE = 10f;

    public static readonly float VOLUME_MODIFIER = .6f;

    private void PlayAudio(AudioClip audio, float volume, float pitch, float startTime)
    {
        GameObject soundContainer = new GameObject(audio.name, typeof(AudioSource));
        AudioSource audioSource = soundContainer.GetComponent<AudioSource>();
        audioSource.clip = audio;
        audioSource.volume = volume * VOLUME_MODIFIER;
        audioSource.pitch = pitch;
        audioSource.time = startTime;
        audioSource.Play();
        StartCoroutine(KillWhenDone(audioSource));
    }

    public static void PlaySound(AudioClip clip, Transform position, float volume = .4f, float pitch = .8f, float startTime = 0)
    {
        if(Vector2.Distance(Camera.main.gameObject.transform.position, position.position) < HEAR_DISTANCE)
            GameManager.Instance.AudioManager.PlayAudio(clip, volume, pitch, startTime);
    }

    private IEnumerator KillWhenDone(AudioSource source)
    {
        yield return new WaitUntil(() =>
        {
            if (source)
                return !source.isPlaying;
            return true;
        });

        if(source && source.gameObject)
            Destroy(source.gameObject);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
