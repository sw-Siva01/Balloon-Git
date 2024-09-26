using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundAudio : MonoBehaviour
{
    [SerializeField] private AudioClip normalBgm;
    //[SerializeField] private AudioClip betBgmStart, betBgmMid, betBgmEnd;
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private float betBgmVolume = 0.4f;
    [SerializeField] private float BgmVolume = 0.2f;

    private void Start()
    {
        bgmAudioSource.loop = true;
    }

    public void CallBetBGM(bool _isStart)
    {
        //StopCoroutine(PlayBetBgm(_isStart));
        //StartCoroutine(PlayBetBgm(_isStart));
    }

    //private IEnumerator PlayBetBgm(bool _isStart)
    //{
    //    bgmAudioSource.volume = betBgmVolume;
    //    float wait = 0;
    //    if (_isStart)
    //    {
    //        bgmAudioSource.clip = betBgmStart;
    //        bgmAudioSource.Play();
    //        wait = bgmAudioSource.clip.length - 0.1f;
    //    }

    //    yield return new WaitForSeconds(wait);

    //    if (_isStart)
    //    {
    //        bgmAudioSource.Stop();
    //        bgmAudioSource.clip = betBgmMid;
    //        bgmAudioSource.loop = true;
    //        bgmAudioSource.Play();
    //    }

    //    if (!_isStart)
    //    {
    //        yield return new WaitForSeconds((bgmAudioSource.clip.length - 0.1f) * 3.2f);
    //        bgmAudioSource.Stop();
    //        bgmAudioSource.loop = false;
    //        bgmAudioSource.clip = betBgmEnd;
    //        bgmAudioSource.Play();
    //        yield return new WaitForSeconds(bgmAudioSource.clip.length + 0.5f);

    //        if (bgmAudioSource.clip == betBgmEnd)
    //        {
    //            bgmAudioSource.Stop();
    //        }
    //    }
    //}

    public void CallWithDelay(bool _isHighDelay)
    {
        Invoke(nameof(PlayGameBGM), _isHighDelay ? 4f : 1f);
    }

    public void SetBgmSoundStatus(bool _state)
    {
        if (_state)
        {
            bgmAudioSource.mute = false;
        }
        else
        {
            bgmAudioSource.mute = true;
        }
    }

    public void PlayGameBGM()
    {

        ResetAudioSource();
        bgmAudioSource.clip = normalBgm;
        bgmAudioSource.loop = true;
        bgmAudioSource.volume = BgmVolume;
        bgmAudioSource.Play();
    }

    private IEnumerator PlayLoop()
    {
        bgmAudioSource.Stop();
        bgmAudioSource.Play();
        yield return new WaitForSeconds(1f);
        StartCoroutine(PlayLoop());
    }

    public void ResetAudioSource()
    {
        if (bgmAudioSource.clip != null)
        {
            bgmAudioSource.Stop();
            bgmAudioSource.clip = null;
            bgmAudioSource.loop = false;
            bgmAudioSource.pitch = 1f;
        }
    }
}

