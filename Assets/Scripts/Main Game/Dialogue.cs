using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Dialogue
{
    public enum EffectType { Typewriter, FadeIn }


    public string name;

    [TextArea(5, 5)]
    public string content;

    public EffectType effectType;
}
