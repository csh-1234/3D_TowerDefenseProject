using UnityEngine;
using System.Collections.Generic;
using System;

public class SoundManager : MonoBehaviour
{
    public enum Sound
    {
        Bgm,
        Effect,
        MaxCount 
    }

    private static SoundManager instance;
    public static SoundManager Instance {
        get 
        {
            if (instance == null)
            {
                GameObject go = GameObject.Find("@Sound");
                if (go == null)
                {
                    go = new GameObject { name = "@Sound" };
                    go.AddComponent<SoundManager>(); 
                }
                else
                {
                    instance = go.GetComponent<SoundManager>();
                }
                DontDestroyOnLoad(go);
            }
            return instance; 
        } 
    }

    private AudioSource[] _audioSources = new AudioSource[(int)Sound.MaxCount]; 
    private Dictionary<string, AudioClip> _audioClips = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void Init()
    {
        string[] soundNames = System.Enum.GetNames(typeof(Sound));
        for (int i = 0; i < soundNames.Length - 1; i++)
        {
            GameObject go = new GameObject { name = soundNames[i] };
            _audioSources[i] = go.AddComponent<AudioSource>();
            go.transform.parent = transform;
        }

        _audioSources[(int)Sound.Bgm].loop = true; 
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    public void Play(string path, Sound type = Sound.Effect, float pitch = 1.0f)
    {
        Play(path, type, pitch, true);
    }

    public void Play(string path, Sound type, float pitch, bool loop)
    {
        if (string.IsNullOrEmpty(path))
            return;

        AudioClip audioClip = GetOrAddAudioClip(path, type);
        if (audioClip == null)
            return;

        if (type == Sound.Bgm)
        {
            AudioSource audioSource = _audioSources[(int)Sound.Bgm];
            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.pitch = pitch;
            audioSource.loop = loop;
            audioSource.clip = audioClip;
            audioSource.Play();
        }
        else
        {
            AudioSource audioSource = _audioSources[(int)Sound.Effect];
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(audioClip);
        }
    }

    private AudioClip GetOrAddAudioClip(string path, Sound type = Sound.Effect)
    {
        if (path.Contains("Sounds/") == false)
        {
            path = $"Sounds/{path}";
        }

        AudioClip audioClip = null;

        if (type == Sound.Bgm)
        {
            audioClip = Resources.Load<AudioClip>(path);
        }
        else
        {
            if (_audioClips.TryGetValue(path, out audioClip) == false)
            {
                audioClip = Resources.Load<AudioClip>(path);
                if (audioClip != null)
                {
                    _audioClips.Add(path, audioClip);
                }
                else
                {
                    AudioClip[] allClips = Resources.LoadAll<AudioClip>("Sounds");
                    foreach (var clip in allClips)
                    {
                        Debug.Log($"- {clip.name}");
                    }
                }
            }
        }

        return audioClip;
    }

    public void Stop(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.Stop();
    }

    public void SetVolume(Sound type, float volume)
    {
        AudioSource audioSource = _audioSources[(int)type];
        audioSource.volume = volume;
    }

    public float GetVolume(Sound type)
    {
        AudioSource audioSource = _audioSources[(int)type];
        return audioSource.volume;
    }

    private float masterVolume= 1f;
    private float effectVolume = 1f;
    private float bgmVolume = 1f;

    public float getMasterVolume()
    {
        return masterVolume;
    }
    public float getEffectVolume()
    {
        return effectVolume;
    }
    public float getBgmVolume()
    {
        return bgmVolume;
    }

    public void SetMasterVolume(float masterAmount)
    {
        masterVolume = masterAmount / 100f;
        AudioSource effectSource = _audioSources[(int)Sound.Effect];
        AudioSource bgmSource = _audioSources[(int)Sound.Bgm];

        effectSource.volume = masterVolume * effectVolume;
        bgmSource.volume = masterVolume * bgmVolume;
    }

    public void SetEffectVolume(float effectAmount)
    {
        effectVolume = effectAmount / 100f;
        AudioSource effectSource = _audioSources[(int)Sound.Effect];
        effectSource.volume = masterVolume * effectVolume;
    }

    public void SetBgmVolume(float bgmAmount)
    {
        bgmVolume = bgmAmount / 100f;
        AudioSource bgmSource = _audioSources[(int)Sound.Bgm];
        bgmSource.volume = masterVolume * bgmVolume;
    }
}   
