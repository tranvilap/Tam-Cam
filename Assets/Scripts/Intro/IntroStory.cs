using Malee;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace TamCam.Intro
{
    [CreateAssetMenu(menuName = "Story/Intro")]
    public class IntroStory : ScriptableObject
    {
        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        public IntroDialogueList dialogues;
    }

    [Serializable]
    public class IntroDialogueList : ReorderableArray<IntroDialogue> { } 
}