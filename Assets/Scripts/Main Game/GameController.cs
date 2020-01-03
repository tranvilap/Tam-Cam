using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class GameController : MonoBehaviour
{
    
    [SerializeField] Route firstRoute;
    [SerializeField] Button buttonPrefab;
    [SerializeField] TextMeshProUGUI contentTextGUI;

    Route playingRoute;
    int currentDialogueIndex = 0;
    // Start is called before the first frame update
    void Start()
    {
        if(playingRoute == null)
        {
            playingRoute = firstRoute;
        }
        ProceedNextDialogue();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            if(currentDialogueIndex >= playingRoute.dialogues.Count)
            {
                Debug.Log("END ROUTE");
            }
            else
            {
                ProceedNextDialogue();
            }
        }
    }

    private void ProceedNextDialogue()
    {
        Dialogue currentDialogue = playingRoute.dialogues[currentDialogueIndex];
        DisplayContent(currentDialogue);
        currentDialogueIndex++;
    }

    private void DisplayContent(Dialogue dialogue)
    {
        contentTextGUI.text = dialogue.content;
    }

    private void ChangeRoute(Route route)
    {
        playingRoute = route;
        currentDialogueIndex = 0;
    }
}
