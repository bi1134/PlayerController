using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private int activeSounds = 0;
    private readonly int maxSounds = 10;
    private readonly float soundCooldown = 0.05f; // delay between plays

    private Queue<AudioClip> queue = new Queue<AudioClip>();
    private bool isPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void PlaySound(AudioSource source, AudioClip clip)
    {
        if (clip == null || source == null) return;

        if (activeSounds < maxSounds)
        {
            source.pitch = Random.Range(0.8f, 1f);
            source.PlayOneShot(clip);
            activeSounds++;
            StartCoroutine(ReleaseSlot(clip.length / source.pitch));
        }
        else
        {
            queue.Enqueue(clip);
            if (!isPlaying) StartCoroutine(DrainQueue(source));
        }
    }

    private IEnumerator ReleaseSlot(float delay)
    {
        yield return new WaitForSeconds(delay);
        activeSounds = Mathf.Max(0, activeSounds - 1);
    }

    private IEnumerator DrainQueue(AudioSource source)
    {
        isPlaying = true;

        while (queue.Count > 0)
        {
            if (activeSounds < maxSounds)
            {
                var next = queue.Dequeue();
                PlaySound(source, next);
                yield return new WaitForSeconds(soundCooldown);
            }
            else
            {
                yield return null;
            }
        }

        isPlaying = false;
    }
}
