using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AudioData", menuName = "SwipeWire/AudioData", order = 1)]
public class AudioCollection : ScriptableObject
{
    public List<AudioDate> audioData = new List<AudioDate>();
}

[System.Serializable]
public struct AudioDate
{
    public AudioClip audioClip;
    public AudioEnum audioName;
    [Range(0,1)]
    public float volume;
    [Range(0, 3)]
    public float pitch;
    public bool mute;
}

public enum AudioEnum
{
    buttonClick,
    winGame,
    Movement,
    ballonPopOut,
    bonus,
    bonusEntry3,
    infiniteScrollview,
    reverseSlider,
    startSlider,
    toggle,
}