using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TamCam.Commons;
using System.Reflection;
using System;
using Malee;

namespace TamCam.MainGame
{
    public class GameController : MonoBehaviour
    {
        [Header("Reference")]
        [SerializeField] TextMeshProUGUI contentTextGUI = null;
        [SerializeField] Image background = null;
        [SerializeField] GameObject questionAndChoicesPanel = null;
        [SerializeField] TextMeshProUGUI questionGUI = null;
        [SerializeField] GameObject choicesArea = null;
        [SerializeField] Image completeBubble = null;
        [SerializeField] Button choiceButtonPrefab = null;
        [SerializeField] AudioSource bgmPlayer = null;
        [SerializeField] AudioSource sfxPlayer = null;

        [Header("Game")]
        [SerializeField] Route firstRoute = null;
        [SerializeField] float typeWriterSpeed = 0.02f;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] BackgroundList backgrounds = null;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] AudioClipList bgms = null;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] AudioClipList sfxs = null;


        bool isChoosingQuestion = false;
        bool isTextTransitioning = false;
        bool isUIChanging = false;

        Route currentRoute;
        int currentDialogueIndex = 0;
        string currentDisplayContent = "";
        Coroutine fadeIn, typeWriter;
        List<Route> playedRoute = new List<Route>();

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

        // Start is called before the first frame update
        void Start()
        {
            if (!CheckNullChoiceGUIComponents())
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
                if (isUIChanging) { return; }
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
                        if (currentRoute.NextRoute != null)
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
            ExecuteEvents(currentDialogue);
            currentDialogueIndex++;
        }
        private void ShowQuestionAndChoices(string question, List<Choice> choices)
        {
            if (CheckNullChoiceGUIComponents()) { return; }

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
            if (CheckNullChoiceGUIComponents()) { return; }

            questionAndChoicesPanel.SetActive(false);
            foreach (Transform child in choicesArea.transform)
            {
                Destroy(child.gameObject);
            }
        }
        public Sprite GetBackground(int index)
        {
            if (index >= backgrounds.Count) { return null; }
            return backgrounds.List[index];
        }
        public AudioClip GetAudioClip(AudioClipList list, int index)
        {
            if (index >= list.Count) { return null; }
            return list.List[index];
        }

        #endregion

        #region Backgrounds
        private void ChangeBackground(int backgroundIndex)
        {
            Sprite bg;
            if (currentRoute == null)
            {
                Debug.LogError("Current route is null");
                return;
            }
            if ((bg = GetBackground(backgroundIndex)) == null)
            {
                Debug.LogWarning("Couldn't get this index's background");
                return;
            }
            if (CheckNullBackgroundGUIComponent()) { return; }
            Debug.Log("Changed background");
            background.sprite = bg;
        }
        private void ChangeBackground(int backgroundIndex, float fadeDuration)
        {
            ChangeBackground(backgroundIndex);
            StartCoroutine(BackgroundFade(fadeDuration));
        }
        private IEnumerator BackgroundFade(float duration)
        {
            if (duration > Mathf.Epsilon && !isUIChanging)
            {
                isUIChanging = true;
                background.color = new Color32(0, 0, 0, 255);
                float timeLapsed = 0f;
                while (timeLapsed < duration)
                {
                    timeLapsed += Time.deltaTime;
                    byte colorByte = (byte)(Mathf.Clamp((timeLapsed / duration) * 255, 0f, 255f));
                    background.color = new Color32(colorByte, colorByte, colorByte, 255);
                    yield return null;
                }
                background.color = new Color32(255, 255, 255, 255);
                isUIChanging = false;
            }
        }
        #endregion

        #region Audio
        private void PlayAudio(AudioSource player, AudioClip clip, int index, float volume, bool isLoop = true)
        {
            player.Stop();
            player.clip = clip;
            player.loop = isLoop;
            if (volume >= Mathf.Epsilon)
            {
                player.volume = volume;
            }
            else
            {
                player.volume = 0;
            }
            player.Play();
        }
        private void PlaySFX(int index, float volume = 1f, bool isLoop = false)
        {
            if (currentRoute == null) { Debug.LogError("Current route is null"); return; }
            if (sfxPlayer == null) { Debug.LogError("Unassigned SFX Player"); return; }
            AudioClip clip = GetAudioClip(sfxs, index);
            if (clip == null) { Debug.LogWarning("Couldn't get this index's SFX clip"); return; }

            PlayAudio(sfxPlayer, clip, index, volume, isLoop);
        }
        private void PlayBGM(int index, float volume = 1f, bool isLoop = true)
        {
            if (currentRoute == null) { Debug.LogError("Current route is null"); return; }
            if (bgmPlayer == null) { Debug.LogError("Unassigned SFX Player"); return; }
            AudioClip clip = GetAudioClip(bgms, index);
            if (clip == null) { Debug.LogWarning("Couldn't get this index's SFX clip"); return; }

            PlayAudio(bgmPlayer, clip, index, volume, isLoop);
        }
        #endregion

        #region Dialogue's Events
        private void ExecuteEvents(Dialogue dialogue)
        {
            if (dialogue.Events.Length == 0) { return; }
            foreach (var ev in dialogue.Events)
            {
                switch (ev.eventType)
                {
                    case DialogueEventType.ChangeBackground:
                        {
                            Debug.Log("Event type: ChangeBackground");
                            HandleChangeBackgroundEvent(ev);
                            break;
                        }
                    case DialogueEventType.PlayBGM:
                        {
                            Debug.Log("Event type: PlayBGM");
                            HandlePlayAudioEvents(ev, bgmPlayer, bgms);
                            break;
                        }
                    case DialogueEventType.PlaySFX:
                        {
                            Debug.Log("Event type: PlaySFX");
                            HandlePlayAudioEvents(ev, sfxPlayer, sfxs);
                            break;
                        }
                }

            }
        }
        private void HandleChangeBackgroundEvent(DialogueEvent ev)
        {
            string[] parameters = ev.parameters;
            if (parameters.Length <= 0)
            {
                Debug.LogWarning("ChangeBackground event need at least one integer argument\n" +
                                 "ChangeBackground(int index); ChangeBackground(int index, float fadeDuration);");
            }
            else if (parameters.Length == 1)
            {
                //ChangeBackground(int backgroundIndex)
                if (parameters.Length == 1 && isInteger(parameters[0]))
                {
                    ChangeBackground(Int32.Parse(ev.parameters[0]));
                }
                else
                {
                    Debug.LogWarning("Invalid Parameters/ ChangeBackground(int index)");
                }
            }
            else
            {
                if (isInteger(parameters[0]) && isFloat(parameters[1]))
                {
                    ChangeBackground(Int32.Parse(parameters[0]), float.Parse(parameters[1]));
                }
                else
                {
                    Debug.LogWarning("Invalid Parameter / ChangeBackground(int index, float fadeTime)");
                }
            }
        }
        private void HandlePlayAudioEvents(DialogueEvent ev, AudioSource player, AudioClipList audioClipList)
        {
            string functionName = "PlayBGM";
            if (ev.eventType == DialogueEventType.PlaySFX)
            {
                functionName = "PlaySFX";
            }
            if (ev.parameters.Length <= 0)
            {
                Debug.LogWarning(functionName + " need at least one integer argument");
            }
            else
            {
                string[] parameter = ev.parameters;


                if (parameter.Length == 1)
                {
                    //PlayAudio(int clipIndex);
                    if (isInteger(parameter[0]))
                    {
                        if(ev.eventType == DialogueEventType.PlayBGM)
                        {
                            PlayBGM(Int32.Parse(parameter[0]));
                        }
                        else if(ev.eventType == DialogueEventType.PlaySFX)
                        {
                            PlaySFX(Int32.Parse(parameter[0]));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid argument/ " + functionName + "(int index)");
                    }
                }
                else if (parameter.Length == 2)
                {
                    //PlayAudio(int clipIndex, float volume);
                    if (isInteger(parameter[0]) && isFloat(parameter[1]))
                    {
                        if (ev.eventType == DialogueEventType.PlayBGM)
                        {
                            PlayBGM(Int32.Parse(parameter[0]), float.Parse(parameter[1]));
                        }
                        else if (ev.eventType == DialogueEventType.PlaySFX)
                        {
                            PlaySFX(Int32.Parse(parameter[0]), float.Parse(parameter[1]));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid argument/ " + functionName + "(int index, float volume)");
                    }
                }
                else if (parameter.Length > 2)
                {
                    //PlayAudio(int clipIndex, float volume, bool isLoop);
                    if (isInteger(parameter[0]) && isFloat(parameter[1]))
                    {

                        if (isBoolean(parameter[2]))
                        {
                            if (ev.eventType == DialogueEventType.PlayBGM)
                            {
                                PlayBGM(Int32.Parse(parameter[0]), float.Parse(parameter[1]), Boolean.Parse(parameter[2]));
                            }
                            else if (ev.eventType == DialogueEventType.PlaySFX)
                            {
                                PlaySFX(Int32.Parse(parameter[0]), float.Parse(parameter[1]), Boolean.Parse(parameter[2]));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid boolean argument");
                            if (ev.eventType == DialogueEventType.PlayBGM)
                            {
                                PlayBGM(Int32.Parse(parameter[0]), float.Parse(parameter[1]));
                            }
                            else if (ev.eventType == DialogueEventType.PlaySFX)
                            {
                                PlaySFX(Int32.Parse(parameter[0]), float.Parse(parameter[1]));
                            }
                            
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid argument/ " + functionName + "(int index, float volume, bool isLoop)");
                    }
                }
            }
        }
        #endregion


        #region Content Displaying
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
        private bool CheckNullChoiceGUIComponents(bool showErrorLog = true)
        {
            bool isNull = (questionAndChoicesPanel == null) || (questionGUI == null) || (choicesArea == null) || (choiceButtonPrefab == null);
            if (isNull && showErrorLog)
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
            return isNull;

        }
        private bool CheckNullBackgroundGUIComponent(bool showErrorLog = true)
        {
            if (background == null)
            {
                Debug.LogError("Background Image Component is null");
            }
            return background == null;
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
        private bool isInteger(string value)
        {
            return Int32.TryParse(value.Trim(), out _);
        }
        private bool isFloat(string value)
        {
            return float.TryParse(value.Trim(), out _);
        }
        private bool isBoolean(string value)
        {
            return Boolean.TryParse(value.Trim(), out _);
        }
        #endregion
    }
}
