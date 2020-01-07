using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace TamCam.Commons
{
    public enum IntroTextEffect { Normal, Typewriter, FadeIn }

    public enum OutroTextEffect { Normal, FadeOut }

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