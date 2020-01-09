using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TamCam.Commons;

namespace TamCam.MainGame
{
    [Serializable]
    public class Dialogue
    {
        [SerializeField] private string characterName = "";

        [TextArea(5, 5)]
        [SerializeField]
        private string content = "";

        [Tooltip("If this value is true, the content will be showed next to previous one")]
        [SerializeField]
        private bool isAdditive = false;

        [Header("Text Effect")]
        [SerializeField] private IntroTextEffect introTextEffect = IntroTextEffect.Typewriter;
        //[SerializeField] private OutroTextEffect outroTextEffect = OutroTextEffect.Normal;

        [SerializeField] [Tooltip("Used for Fade In Intro Effect")] private float fadeInDuration = 0f;
        //[SerializeField] [Tooltip("Used for Fade Out Outro Effect")] private float fadeOutDuration = 0f;

        #region Properties (Getters and Setters)
        public IntroTextEffect IntroTextEffect { get => introTextEffect; }
        //public OutroTextEffect OutroTextEffect { get => outroTextEffect; }
        public string CharacterName { get => characterName; }
        public string Content { get => content; }
        public bool IsAdditive { get => isAdditive; }
        public float FadeInDuration { get => fadeInDuration; }
        //public float FadeOutDuration { get => fadeOutDuration; }
        #endregion
    }
}

