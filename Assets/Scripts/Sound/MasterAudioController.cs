using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class MasterAudioController : MonoBehaviour
{
    public static MasterAudioController instance;
    public bool muteAllAudio;

    public Queue<AudioSource> audioSources = new Queue<AudioSource>();

    public int count;

    [System.Serializable]
    public class AudioByType
    {
        public AudioEnum audioEnum;
        public List<AudioSource> audioActive;
    }

    [SerializeField] List<AudioByType> audiosActive = new List<AudioByType>();

    int poolSize = 4;

    public AudioCollection audioCollection;

    public BackgroundAudio BackgroundAudio;

    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
#if !UNITY_SERVER
        count = audioSources.Count;
#endif
    }
    private void Start()
    {
        audioCollection = Resources.Load("AudioData", typeof(AudioCollection)) as AudioCollection;
        InitializePool();

    }


    public void StopAudio(AudioEnum audioToPlay)
    {
        while (audiosActive.Exists(x => x.audioEnum == audioToPlay))
        {
            AudioByType audiotypeTostop = audiosActive.Find(x => x.audioEnum == audioToPlay);
            if (audiotypeTostop == null) return;
            foreach (var item in audiotypeTostop.audioActive)
            {
                item.Stop();
                audioSources.Enqueue(item);
            }
            audiosActive.Remove(audiotypeTostop);
        }
    }

    public void PlayAudio(AudioEnum audioToPlay, bool loop = false)
    {
        if (muteAllAudio || audioCollection == null) return;

        AudioDate collection = audioCollection.audioData.Find(x => x.audioName == audioToPlay);

        if (collection.mute) return;
        AudioSource source = GetAudioSource();
        source.Stop();
        source.clip = collection.audioClip;
        source.loop = loop;
        source.volume = collection.volume;
        source.pitch = collection.pitch;
        source.Play();

        if (audioToPlay == AudioEnum.UiButtonClick) source.volume = 0.5f;
        //if (audioToPlay == AudioEnum.Button) source.volume = 0.2f;

        AudioByType audioType = audiosActive.Find(x => x.audioEnum == audioToPlay);
        if (audioType != null)
            audioType.audioActive.Add(source);
        else
            audiosActive.Add(new AudioByType { audioEnum = audioToPlay, audioActive = new List<AudioSource> { source } });

        if (!loop)
        {
            StartCoroutine(EnqueueAudioSource(source));
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    void CreateNewAudioSource()
    {
        GameObject go = new GameObject("Audio", typeof(AudioSource));
        go.hideFlags = HideFlags.HideAndDontSave;
        go.transform.SetParent(transform);
        AudioSource audiosource = go.GetComponent<AudioSource>();
        audiosource.loop = false;
        audiosource.volume = 1;
        audiosource.playOnAwake = false;
        audioSources.Enqueue(audiosource);
    }

    AudioSource GetAudioSource()
    {
        AudioSource audioSource = null;
        if (audioSources.Count == 0)
        {
            CreateNewAudioSource();
        }
        audioSource = audioSources.Dequeue();
        return audioSource;
    }

    IEnumerator EnqueueAudioSource(AudioSource source)
    {
        if (source.clip == null)
        {
            foreach (var audiotype in audiosActive)
            {
                if (audiotype.audioActive.Contains(source))
                {
                    audiotype.audioActive.Remove(source);
                }
            }

            yield break;
        }

        int randomNumber = Random.Range(500, 50000);
        yield return new WaitForSeconds(source.clip.length + .5f);

        if (source.clip != null)
        {
            source.Stop();
            if (!audioSources.Contains(source))
                audioSources.Enqueue(source);
            source.clip = null;
            foreach (var audiotype in audiosActive)
            {
                if (audiotype.audioActive.Contains(source))
                {
                    audiotype.audioActive.Remove(source);
                }
            }
        }
    }

    public bool MuteStatus;
    public void SetVolumeMute()
    {
        muteAllAudio = true;

        foreach (var item in audioSources)
        {
            item.mute = true;
        }

        foreach (var collection in audiosActive)
        {
            foreach (var audio in collection.audioActive)
            {
                audio.mute = true;
            }
        }
    }

    public void SetVolumeUnMute()
    {
       
        muteAllAudio = false;

        foreach (var item in audioSources)
        {
            item.mute = false;
        }

        foreach (var collection in audiosActive)
        {
            foreach (var audio in collection.audioActive)
            {
                audio.mute = false;
            }
        }
       
    }
}

