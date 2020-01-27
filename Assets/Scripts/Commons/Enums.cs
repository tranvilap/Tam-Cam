using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TamCam.Commons
{
    public enum IntroTextEffect { Normal, Typewriter, FadeIn }

    public enum OutroTextEffect { Normal, FadeOut }

    public enum DialogueEventType
    {
        PlayBGM, //PlayBGM(int index); PlayBGM(int index, float volume); PlayBGM(int index, float volume, bool isLoop);
        PlaySFX, //PlaySFX(int index); PlaySFX(int index, float volume); PlaySFX(int index, float volume, bool isLoop);
        ChangeVolumeBGM, //ChangeVolumeBGM(float volume)
        ChangeVolumeSFX, //ChangeVolumeSFX(float volume)
        StopBGM, StopSFX, //StopBGM(); StopSFX();
        ChangeBackground, //ChangeBackground(int index); ChangeBackground(int index, float fadeDuration);
        AddOrChangeCharacter, //AddCharacter(string name, int spriteIndex, float posX=0f, float posY=0f, float scaleX=1f, float scaleY=1f);
        MoveCharacter, //MoveCharacter(string name, float posX, float posY);
        ScalingCharacter, //ScalingCharacter(string name, float scaleX, float scaleY);
        HideCharacter, //HideCharacter(string name);
        ShowCharacter, //ShowCharacter(string name);
        RemoveCharacter

    }

    //Example for multiple choices enum
    //[System.Flags]
    //public enum OutroTextEffect { Normal, FadeOut }
    //[EnumFlagsAttribute] private IntroTextEffect introTextEffect;
}

public class EnumFlagsAttribute : PropertyAttribute
{
    public EnumFlagsAttribute() { }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(EnumFlagsAttribute))]
public class EnumFlagsAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect _position, SerializedProperty _property, GUIContent _label)
    {
        _property.intValue = EditorGUI.MaskField(_position, _label, _property.intValue, _property.enumNames);
    }
}
#endif