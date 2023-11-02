using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : MonoBehaviour
{
    public static SoundController Instance;

    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip[] playOnAwakeSounds;
    [SerializeField] private AudioClip[] clips;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        foreach (var j in Instance.playOnAwakeSounds)
        {
            Instance.source.PlayOneShot(j);
        }
    }

    public static void PlayAudioClip(AudioClip clip)
    {
        if (Instance == null || Instance.source == null)
            return;
        Instance.source.PlayOneShot(clip);
    }
    public static void PlayAudioClip(string clipName)
    {
        if (Instance == null || Instance.source == null || Instance.clips.Length == 0)
            return;

        foreach (var j in Instance.clips)
            if (j.name == clipName)
                Instance.source.PlayOneShot(j);
    }
}
