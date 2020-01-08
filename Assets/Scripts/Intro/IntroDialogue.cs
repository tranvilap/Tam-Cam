using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IntroDialogue
{
    [TextArea(5, 5)]public string content;

    [Header("Audio")]
    public Audio bgm;
    public bool startOverBGM;
    public Audio SFX;

    [Header("Text Transitions")]
    public bool isFadedIn;
    public float fadedInDuration;

    public bool isFadedOut;
    public float fadedOutDuration;
}

[Serializable]
public class Audio
{
    public AudioClip clip;
    [Range(0f,1f)]public float volume = 1;
}