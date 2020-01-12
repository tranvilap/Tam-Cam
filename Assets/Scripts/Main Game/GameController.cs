using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TamCam.Commons;
namespace TamCam.MainGame
{
    public class GameController : MonoBehaviour
    {
        [Header("GUI")]
        [SerializeField] TextMeshProUGUI contentTextGUI = null;
        [SerializeField] GameObject questionAndChoicesPanel = null;
        [SerializeField] TextMeshProUGUI questionGUI = null;
        [SerializeField] GameObject choicesArea = null;
        [SerializeField] Image completeBubble = null;
        [SerializeField] Button choiceButtonPrefab = null;

        [Header("Game")]
        [SerializeField] Route firstRoute = null;
        [SerializeField] float typeWriterSpeed = 0.02f;

        Route currentRoute;
        int currentDialogueIndex = 0;
        string currentDisplayContent = "";
        Coroutine fadeIn, typeWriter;
        bool isChoosingQuestion = false;
        List<Route> playedRoute = new List<Route>();
        public List<Sprite> characters;

        private static GameController instance;
        private GameController() { }
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public static GameController Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameController>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject(typeof(GameController).Name);
                        instance = go.AddComponent<GameController>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        bool isTextTransitioning = false;

        // Start is called before the first frame update
        void Start()
        {
            if (CheckNotNullChoiceGUIComponents())
            {
                questionAndChoicesPanel.SetActive(false);

            }
            currentDisplayContent = contentTextGUI.text;
            if (currentRoute == null)
            {
                currentRoute = firstRoute;
            }
            ProceedNextDialogue();
        }

        // Update is called once per frame
        void Update()
        {

            if (Input.GetMouseButtonUp(0))
            {
                if (isTextTransitioning)
                {
                    CompleteCurrentTextTransition();
                }
                else
                {
                    if (currentDialogueIndex >= currentRoute.Dialogues.Count)
                    {
                        if (isChoosingQuestion) { return; }
                        if (currentRoute.Choices.Count > 0)
                        {
                            ShowQuestionAndChoices(currentRoute.ChangeRouteQuestion, currentRoute.Choices.List);
                            return;
                        }
                        if(currentRoute.NextRoute != null)
                        {
                            ChangeRoute(currentRoute.NextRoute);
                        }
                    }
                    else
                    {
                        ProceedNextDialogue();
                    }
                }
            }
        }

        #region Main Methods
        private void ProceedNextDialogue()
        {
            Dialogue currentDialogue = currentRoute.Dialogues[currentDialogueIndex];
            DisplayContent(currentDialogue);
            currentDialogueIndex++;
        }
        private void ShowQuestionAndChoices(string question, List<Choice> choices)
        {
            if (!CheckNotNullChoiceGUIComponents()) { return; }

            isChoosingQuestion = true;
            questionAndChoicesPanel.SetActive(true);
            questionGUI.text = question;
            foreach (var choice in choices)
            {
                Button button = Instantiate(choiceButtonPrefab, choicesArea.transform);
                //Add Name to the button
                button.onClick.AddListener(delegate { ChangeRoute(choice.nextRoute); });
            }

        }
        #endregion

        #region Common Methods
        private void SetContentText(string text)
        {
            contentTextGUI.text = text;
            currentDisplayContent = text;
        }
        private void ChangeRoute(Route route)
        {
            currentRoute = route;
            currentDialogueIndex = 0;
            isChoosingQuestion = false;
            playedRoute.Add(route);
            if (!CheckNotNullChoiceGUIComponents()) { return; }

            questionAndChoicesPanel.SetActive(false);
            foreach (Transform child in choicesArea.transform)
            {
                Destroy(child.gameObject);
            }
        }
        #endregion

        #region Displaying Content Methods
        private void DisplayContent(Dialogue dialogue)
        {
            if (fadeIn != null) { fadeIn = null; }
            if (typeWriter != null) { typeWriter = null; }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(false);
            }

            if (dialogue.IntroTextEffect == IntroTextEffect.Normal)
            {
                DisplayText_Normal(dialogue.Content, dialogue.IsAdditive);
                return;
            }

            isTextTransitioning = true;
            if (dialogue.IntroTextEffect == IntroTextEffect.Typewriter)
            {
                typeWriter = StartCoroutine(TypewriterEffect(dialogue.Content, typeWriterSpeed, dialogue.IsAdditive));
            }
            else if (dialogue.IntroTextEffect == IntroTextEffect.FadeIn)
            {
                fadeIn = StartCoroutine(FadeInEffect(dialogue.Content, dialogue.FadeInDuration, dialogue.IsAdditive));
            }

        }

        private void CompleteCurrentTextTransition()
        {
            if (!isTextTransitioning) { return; }

            isTextTransitioning = false;

            if (fadeIn != null)
            {
                Debug.Log("Completed Fade In transition");
                StopCoroutine(fadeIn);
                fadeIn = null;
                string temp = contentTextGUI.text;
                contentTextGUI.text = "";
                SetContentText(temp);
                contentTextGUI.color = new Color(contentTextGUI.color.r, contentTextGUI.color.g, contentTextGUI.color.b, 1f);
            }
            if (typeWriter != null)
            {
                Debug.Log("Completed TypeWriter transition");
                StopCoroutine(typeWriter);
                typeWriter = null;
                if (!currentRoute.Dialogues[currentDialogueIndex - 1].IsAdditive)
                {
                    SetContentText(currentRoute.Dialogues[currentDialogueIndex - 1].Content);
                }
                else
                {
                    if ((currentDialogueIndex - 1) < 0)
                    {
                        SetContentText(currentDisplayContent);
                    }
                    else
                    {
                        SetContentText(currentDisplayContent + currentRoute.Dialogues[currentDialogueIndex - 1].Content);
                    }
                }
            }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
        }

        public void DisplayText_Normal(string text, bool isConjunctive = false)
        {
            if (isConjunctive)
            {
                SetContentText(contentTextGUI.text + text);
            }
            else
            {
                SetContentText(text);
            }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
        }

        public IEnumerator TypewriterEffect(string text, float speed, bool isAdditive)
        {
            int isSkipping = 0;
            isTextTransitioning = true;
            if (!isAdditive)
            {
                contentTextGUI.text = "";
            }
            string temp = contentTextGUI.text;
            if (speed < 0)
            {
                contentTextGUI.text = text;
            }
            else
            {
                foreach (var character in text)
                {
                    temp += character;
                    if (character.Equals('<'))
                    {
                        isSkipping++;
                    }
                    if (isSkipping == 0)
                    {
                        contentTextGUI.text += character;
                        yield return new WaitForSeconds(speed);
                    }
                    if (character.Equals('>'))
                    {
                        if (isSkipping > 0)
                        {
                            isSkipping--;
                        }
                        if (isSkipping == 0)
                        {
                            contentTextGUI.text = temp;
                        }
                    }
                }
            }
            SetContentText(contentTextGUI.text);
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
            isTextTransitioning = false;
        }

        public IEnumerator FadeInEffect(string text, float duration, bool isAdditive)
        {
            isTextTransitioning = true;
            if (duration > Mathf.Epsilon)
            {
                if (isAdditive)
                {
                    contentTextGUI.ForceMeshUpdate();
                    var textInfo = contentTextGUI.textInfo;

                    int lastIndex = textInfo.characterCount;
                    contentTextGUI.text += text;

                    contentTextGUI.ForceMeshUpdate();
                    int textLength = textInfo.characterCount;

                    Color32[] newVertexColors;
                    float timeLapsed = 0f;


                    while (timeLapsed < duration)
                    {
                        for (int i = lastIndex; i < textLength; i++)
                        {
                            // Get the index of the material used by the current character.
                            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;

                            // Get the vertex colors of the mesh used by this text element (character or sprite).
                            newVertexColors = textInfo.meshInfo[materialIndex].colors32;

                            // Get the index of the first vertex used by this text element.
                            int vertexIndex = textInfo.characterInfo[i].vertexIndex;

                            if (textInfo.characterInfo[i].isVisible)
                            {

                                byte alpha = (byte)Mathf.Clamp((timeLapsed / duration) * 255, 0, 255);

                                newVertexColors[vertexIndex + 0].a = alpha;
                                newVertexColors[vertexIndex + 1].a = alpha;
                                newVertexColors[vertexIndex + 2].a = alpha;
                                newVertexColors[vertexIndex + 3].a = alpha;

                                // New function which pushes (all) updated vertex data to the appropriate meshes when using either the Mesh Renderer or CanvasRenderer.
                                contentTextGUI.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                                // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                                // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
                            }
                        }
                        timeLapsed += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    contentTextGUI.text = text;
                    contentTextGUI.color = new Color(contentTextGUI.color.r, contentTextGUI.color.g, contentTextGUI.color.b, 0f);
                    float timeLapsed = 0f;
                    while (timeLapsed < duration)
                    {
                        timeLapsed += Time.deltaTime;
                        contentTextGUI.color = new Color(contentTextGUI.color.r, contentTextGUI.color.g, contentTextGUI.color.b, timeLapsed / duration);
                        yield return null;
                    }
                }
            }
            else
            {
                if (isAdditive)
                {
                    contentTextGUI.text += text;
                }
                else
                {
                    contentTextGUI.text = text;
                }
            }
            SetContentText(contentTextGUI.text);
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
            contentTextGUI.color = new Color(contentTextGUI.color.r, contentTextGUI.color.g, contentTextGUI.color.b, 1f);
            isTextTransitioning = false;
        }
        #endregion
        
        #region Misc
        private bool CheckNotNullChoiceGUIComponents(bool showErrorLog = true)
        {
            bool result = (questionAndChoicesPanel != null) && (questionGUI != null) && (choicesArea != null) && (choiceButtonPrefab != null);
            if (!result && showErrorLog)
            {
                if (questionAndChoicesPanel == null)
                {
                    Debug.LogError("Couldn't access to QuestionAndChoices Panel ");
                }
                if (questionGUI == null)
                {
                    Debug.LogError("Couldn't access to Question Text");
                }
                if (choicesArea == null)
                {
                    Debug.LogError("Couldn't access to Choice Area");
                }
                if (choiceButtonPrefab == null)
                {
                    Debug.LogError("Button prefab is null");
                }
            }
            return result;

        }
        public void ResetAllRoute()
        {
            if (firstRoute == null)
            {
                Debug.LogError("Missing First Route");
                return;
            }
            ChangeRoute(firstRoute);
        }
        #endregion
    }
}
