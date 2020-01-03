using Malee;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Route",menuName = "Story/Route")]
public class Route : ScriptableObject
{
    [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
    public DialogueList dialogues;

    [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
    public QuestionList questions;

    [Tooltip("If this route doesn't have any question, it will change to this assigned route")]
    public Route nextRoute;
    
}

[Serializable]
public class Question
{
    public string question;
    public Route nextRoute;
}
[Serializable]
public class DialogueList : ReorderableArray<Dialogue> { }
[Serializable]
public class QuestionList : ReorderableArray<Question> { }