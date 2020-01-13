using Malee;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TamCam.MainGame
{
    [CreateAssetMenu(fileName = "Route", menuName = "Story/Route")]
    public class Route : ScriptableObject
    {
        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] DialogueList dialogues = null;

        [SerializeField] private string changeRouteQuestion;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] QuestionList choices = null;

        [Tooltip("If this route doesn't have any question, it will change to this assigned route")]
        [SerializeField] Route nextRoute = null;

        public DialogueList Dialogues { get => dialogues; }

        public string ChangeRouteQuestion { get => changeRouteQuestion; set => changeRouteQuestion = value; }
        public QuestionList Choices { get => choices; }

        public Route NextRoute { get => nextRoute; }
    }

    [Serializable]
    public class Choice
    {
        public string choiceName;
        public Route nextRoute;
    }
    [Serializable]
    public class DialogueList : ReorderableArray<Dialogue> { }
    [Serializable]
    public class QuestionList : ReorderableArray<Choice> { }
    [Serializable]
    public class BackgroundList : ReorderableArray<Sprite> { }
    [Serializable]
    public class AudioClipList : ReorderableArray<AudioClip> { }
}
