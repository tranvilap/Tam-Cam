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
        [SerializeField] Canvas mainGameCanvas = null;
        [SerializeField] Image background = null;
        [SerializeField] TextMeshProUGUI contentText = null;
        [SerializeField] GameObject questionAndChoicesPanel = null;
        [SerializeField] TextMeshProUGUI questionText = null;
        [SerializeField] GameObject choicesArea = null;
        [SerializeField] RectTransform charactersArea = null;
        [SerializeField] AudioSource bgmPlayer = null;
        [SerializeField] AudioSource sfxPlayer = null;

        [Header("Game")]
        [SerializeField] Route firstRoute = null;
        [SerializeField] float typeWriterSpeed = 0.02f;
        [SerializeField] float fadeOutCharacterDuration = 0.2f;
        [SerializeField] float fadeInCharacterDuration = 0.2f;


        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] BackgroundList backgrounds = null;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] AudioClipList bgms = null;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] AudioClipList sfxs = null;

        [Reorderable(add = true, draggable = true, paginate = true, pageSize = 10, remove = true, sortable = false)]
        [SerializeField] CharacterSprite characterSprites = null;

        [Header("Prefabs")]
        [SerializeField] GameObject characterSpritePrefab = null;
        [SerializeField] Button choiceButtonPrefab = null;
        [SerializeField] Image completeBubble = null;

        bool isChoosingQuestion = false;
        bool isTextTransitioning = false;
        bool isBackgroundChanging = false;

        Route currentRoute;
        int currentDialogueIndex = 0;
        string currentDisplayContent = "";
        Coroutine fadeIn, typeWriter;
        List<Route> playedRoute = new List<Route>();
        Dictionary<string, GameObject> characterDictionary = new Dictionary<string, GameObject>();

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
            currentDisplayContent = contentText.text;
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
                if (isBackgroundChanging) { return; }
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
            questionText.text = question;
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
            contentText.text = text;
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
        private Sprite GetBackground(int index)
        {
            if (index >= backgrounds.Count) { return null; }
            return backgrounds.List[index];
        }
        private AudioClip GetAudioClip(AudioClipList list, int index)
        {
            if (index >= list.Count) { return null; }
            return list.List[index];
        }
        private Sprite GetCharacterSprite(int index)
        {
            if (index >= characterSprites.Count) { return null; }
            return characterSprites.List[index];
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
            if (duration > Mathf.Epsilon && !isBackgroundChanging)
            {
                isBackgroundChanging = true;
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
                isBackgroundChanging = false;
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
        private void ChangeVolume(AudioSource player, float volume)
        {
            if (volume <= Mathf.Epsilon)
            {
                player.volume = 0;
            }
            else
            {
                player.volume = volume;
            }
        }
        private void ChangeVolumeBGM(float volume)
        {
            ChangeVolume(bgmPlayer, volume);
        }
        private void ChangeVolumeSFX(float volume)
        {
            ChangeVolume(sfxPlayer, volume);
        }
        private void StopAudioPlayer(AudioSource player)
        {
            if (player.isPlaying)
            {
                player.Stop();
            }
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
                    case DialogueEventType.ChangeVolumeBGM:
                        {
                            Debug.Log("Event type: Change BGM Volume");
                            HandleChangeVolumeBGMEvent(ev);
                            break;
                        }
                    case DialogueEventType.ChangeVolumeSFX:
                        {
                            Debug.Log("Event type: Change SFX Volume");
                            HandleChangeSFXVolume(ev);
                            break;
                        }
                    case DialogueEventType.StopBGM:
                        {
                            Debug.Log("Event type: StopBGM");
                            StopAudioPlayer(bgmPlayer);
                            break;
                        }
                    case DialogueEventType.StopSFX:
                        {
                            Debug.Log("Event type: StopBGM");
                            StopAudioPlayer(sfxPlayer);
                            break;
                        }
                    case DialogueEventType.AddOrChangeCharacter:
                        {
                            Debug.Log("Event type: AddOrChangeCharacter");
                            HandleAddOrChangeCharacterEvent(ev);
                            break;
                        }
                    case DialogueEventType.MoveCharacter:
                        {
                            Debug.Log("Event type: MoveCharacter");
                            HandleMoveCharacterEvent(ev);
                            break;
                        }
                }
            }
        }

        private void HandleMoveCharacterEvent(DialogueEvent ev)
        {
            var parameters = ev.parameters;
            if (parameters.Length < 2 || String.IsNullOrWhiteSpace(parameters[0]))
            {
                Debug.LogWarning("HandleMoveCharacterEvent needs at least two arguments HandleMoveCharacterEvent(string name, float posX)");
                return;
            }
            GameObject character;
            if (!characterDictionary.TryGetValue(parameters[0], out character))
            {
                Debug.LogWarning("This character name doesn't exist");
                return;
            }

            var characterReactTransform = character.GetComponent<RectTransform>();
            float posX = characterReactTransform.localPosition.x;
            float posY = characterReactTransform.localPosition.y;
            if (!isFloat(parameters[1]))
            {
                Debug.LogWarning("Invalid second argument - float posX/ HandleMoveCharacterEvent(string name, float posX)");
            }
            else { posX = float.Parse(parameters[1]); }
            if(parameters.Length >= 3)
            {
                if (!isFloat(parameters[2]))
                {
                    Debug.LogWarning("Invalid second argument - float posX/ HandleMoveCharacterEvent(string name, float posX, float posY)");
                }
                else { posY = float.Parse(parameters[2]); }
            }
            Image imageComponent = character.GetComponent<Image>();
            ChangeCharacter(imageComponent, imageComponent.sprite, posX, posY, characterReactTransform.localScale.x, characterReactTransform.localScale.y);
        }

        private void HandleAddOrChangeCharacterEvent(DialogueEvent ev)
        {
            var parameters = ev.parameters;
            if (parameters.Length == 0 || String.IsNullOrWhiteSpace(parameters[0]))
            {
                Debug.LogWarning("AddCharacter needs at least a string argument AddCharacter(string name)");
                return;
            }
            GameObject character = null;
            if (parameters.Length >= 1)
            {
                //AddCharacter(string name)
                characterDictionary.TryGetValue(parameters[0], out character);
            }

            RectTransform characterRectTransform;
            float posX = 0f, posY = 0f, scaleX = 1f, scaleY = 1f;

            if (character == null)
            {
                character = Instantiate(characterSpritePrefab, charactersArea);
                character.name = parameters[0];
                characterDictionary.Add(parameters[0], character);
                Debug.Log("Added Character: " + parameters[0]);
                //Set Anchor to stretch all
                characterRectTransform = character.GetComponent<RectTransform>();
                characterRectTransform.pivot = new Vector2(0.5f, 0.5f);
                characterRectTransform.anchorMin = new Vector2(0, 0);
                characterRectTransform.anchorMax = new Vector2(1, 1);
            }
            else
            {
                characterRectTransform = character.GetComponent<RectTransform>();
                posX = characterRectTransform.localPosition.x;
                posY = characterRectTransform.localPosition.y;
                scaleX = characterRectTransform.localScale.x;
                scaleY = characterRectTransform.localScale.y;
            }

            var imageComponent = character.GetComponent<Image>();
            if (imageComponent == null)
            {
                imageComponent = character.AddComponent<Image>();
            }

            if (parameters.Length >= 2)
            {
                //Has sprite index
                if (!isInteger(parameters[1]))
                {
                    Debug.LogWarning("Invalid second argument - integer spriteIndex/ AddCharacter(string name, int spriteIndex)");
                }
                else
                {
                    //Read arguments
                    if (parameters.Length >= 3)
                    {
                        if (!isFloat(parameters[2]))
                        {
                            Debug.LogWarning("Invalid third argument - float positionX / " +
                                "AddCharacter(string name, int spriteIndex, float positionX");
                        }
                        else { posX = float.Parse(parameters[2]); }
                    }
                    if (parameters.Length >= 4)
                    {
                        if (!isFloat(parameters[3]))
                        {
                            Debug.LogWarning("Invalid 4th argument - float positionY / " +
                                "AddCharacter(string name, int spriteIndex, float positionX, float positionY");
                        }
                        else { posY = float.Parse(parameters[3]); }
                    }
                    if (parameters.Length >= 5)
                    {
                        if (!isFloat(parameters[4]))
                        {
                            Debug.LogWarning("Invalid 5th argument - float scaleX / " +
                                "AddCharacter(string name, int spriteIndex, float positionX, float positionY, scaleX");
                        }
                        else { scaleX = float.Parse(parameters[4]); }
                    }
                    if (parameters.Length >= 6)
                    {
                        if (!isFloat(parameters[5]))
                        {
                            Debug.LogWarning("Invalid 6th argument - float scaleY / " +
                                "AddCharacter(string name, int spriteIndex, float positionX, float positionY, scaleX, scaleY");
                        }
                        else { scaleY = float.Parse(parameters[5]); }
                    }
                    ChangeCharacter(imageComponent, GetCharacterSprite(Int32.Parse(parameters[1])), posX, posY, scaleX, scaleY);
                }
            }
            else if(parameters.Length == 1)
            {
                //if(imageComponent.sprite == null)
                //{
                //    character.SetActive(false);
                //}
            }
        }
        private void HandleChangeSFXVolume(DialogueEvent ev)
        {
            if (ev.parameters.Length <= 0)
            {
                Debug.LogWarning("ChangeVolumeSFX need at least one float argument");
            }
            else
            {
                if (isFloat(ev.parameters[0]))
                {
                    ChangeVolumeSFX(float.Parse(ev.parameters[0]));
                }
                else { Debug.LogWarning("Invalid argument/ ChangeVolumeSFX(float volume) - range: 0-1"); }
            }
        }
        private void HandleChangeVolumeBGMEvent(DialogueEvent ev)
        {
            if (ev.parameters.Length <= 0)
            {
                Debug.LogWarning("ChangeVolumeBGM need at least one float argument");
            }
            else
            {
                if (isFloat(ev.parameters[0]))
                {
                    ChangeVolumeBGM(float.Parse(ev.parameters[0]));
                }
                else { Debug.LogWarning("Invalid argument/ ChangeVolumeBGM(float volume) - range: 0-1"); }
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
                string[] parameters = ev.parameters;


                if (parameters.Length == 1)
                {
                    //PlayAudio(int clipIndex);
                    if (isInteger(parameters[0]))
                    {
                        if (ev.eventType == DialogueEventType.PlayBGM)
                        {
                            PlayBGM(Int32.Parse(parameters[0]));
                        }
                        else if (ev.eventType == DialogueEventType.PlaySFX)
                        {
                            PlaySFX(Int32.Parse(parameters[0]));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid argument/ " + functionName + "(int index)");
                    }
                }
                else if (parameters.Length == 2)
                {
                    //PlayAudio(int clipIndex, float volume);
                    if (isInteger(parameters[0]) && isFloat(parameters[1]))
                    {
                        if (ev.eventType == DialogueEventType.PlayBGM)
                        {
                            PlayBGM(Int32.Parse(parameters[0]), float.Parse(parameters[1]));
                        }
                        else if (ev.eventType == DialogueEventType.PlaySFX)
                        {
                            PlaySFX(Int32.Parse(parameters[0]), float.Parse(parameters[1]));
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Invalid argument/ " + functionName + "(int index, float volume)");
                    }
                }
                else if (parameters.Length > 2)
                {
                    //PlayAudio(int clipIndex, float volume, bool isLoop);
                    if (isInteger(parameters[0]) && isFloat(parameters[1]))
                    {

                        if (isBoolean(parameters[2]))
                        {
                            if (ev.eventType == DialogueEventType.PlayBGM)
                            {
                                PlayBGM(Int32.Parse(parameters[0]), float.Parse(parameters[1]), Boolean.Parse(parameters[2]));
                            }
                            else if (ev.eventType == DialogueEventType.PlaySFX)
                            {
                                PlaySFX(Int32.Parse(parameters[0]), float.Parse(parameters[1]), Boolean.Parse(parameters[2]));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Invalid boolean argument");
                            if (ev.eventType == DialogueEventType.PlayBGM)
                            {
                                PlayBGM(Int32.Parse(parameters[0]), float.Parse(parameters[1]));
                            }
                            else if (ev.eventType == DialogueEventType.PlaySFX)
                            {
                                PlaySFX(Int32.Parse(parameters[0]), float.Parse(parameters[1]));
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


        #region Character
        private void ChangeCharacter(Image imageComponent, Sprite sprite, float posX, float posY, float scaleX, float scaleY)
        {
            StartCoroutine(ChangeCharacterEffect(imageComponent, sprite, posX, posY, scaleX, scaleY));
        }
        IEnumerator ChangeCharacterEffect(Image imageComponent, Sprite sprite, float posX, float posY, float scaleX, float scaleY)
        {
            if (fadeOutCharacterDuration >= Mathf.Epsilon && fadeInCharacterDuration >= Mathf.Epsilon)
            {
                float fadeOutCounter = 0f, fadeInCounter = 0f;
                Color originalColor = imageComponent.color;
                //Fade out
                if (imageComponent.sprite != null)
                {
                    while (fadeOutCounter < fadeOutCharacterDuration)
                    {
                        imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                                                        1 - (fadeOutCounter / fadeOutCharacterDuration));
                        fadeOutCounter += Time.deltaTime;
                        yield return null;
                    }
                }
                else
                {
                    imageComponent.gameObject.SetActive(false);
                }
                imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);


                //Fade in
                //Align sprite size and position
                imageComponent.sprite = sprite;
                float spriteWidth = 0f, spriteHeight = 0f;
                if (sprite != null)
                {
                    spriteWidth = sprite.rect.width;
                    spriteHeight = sprite.rect.height;
                }
                var rectTransform = imageComponent.GetComponent<RectTransform>();
                var referenceResolution = mainGameCanvas.GetComponent<CanvasScaler>().referenceResolution;
                float left = ((referenceResolution.x - spriteWidth) / 2) + posX;
                float right = referenceResolution.x - (left + spriteWidth);
                float top = ((referenceResolution.y - spriteHeight) / 2) + posY;
                float bottom = referenceResolution.y - (top + spriteHeight);
                rectTransform.offsetMin = new Vector2(left, top);
                rectTransform.offsetMax = new Vector2(-right, -bottom);

                //Change Scale
                rectTransform.localScale = new Vector2(scaleX, scaleY);

                if (sprite != null)
                {
                    imageComponent.gameObject.SetActive(true);
                    while (fadeInCounter < fadeInCharacterDuration)
                    {
                        imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b,
                                                        (fadeInCounter / fadeInCharacterDuration));
                        fadeInCounter += Time.deltaTime;
                        yield return null;
                    }
                    imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                }
                else
                {
                    //Disable Character coz no sprite to display
                    imageComponent.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                    Debug.Log("SEt false");
                    imageComponent.gameObject.SetActive(false);
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
                string temp = contentText.text;
                contentText.text = "";
                SetContentText(temp);
                contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, 1f);
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
                SetContentText(contentText.text + text);
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
                contentText.text = "";
            }
            string temp = contentText.text;
            if (speed < 0)
            {
                contentText.text = text;
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
                        contentText.text += character;
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
                            contentText.text = temp;
                        }
                    }
                }
            }
            SetContentText(contentText.text);
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
                    contentText.ForceMeshUpdate();
                    var textInfo = contentText.textInfo;

                    int lastIndex = textInfo.characterCount;
                    contentText.text += text;

                    contentText.ForceMeshUpdate();
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
                                contentText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

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
                    contentText.text = text;
                    contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, 0f);
                    float timeLapsed = 0f;
                    while (timeLapsed < duration)
                    {
                        timeLapsed += Time.deltaTime;
                        contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, timeLapsed / duration);
                        yield return null;
                    }
                }
            }
            else
            {
                if (isAdditive)
                {
                    contentText.text += text;
                }
                else
                {
                    contentText.text = text;
                }
            }
            SetContentText(contentText.text);
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
            contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, 1f);
            isTextTransitioning = false;
        }
        #endregion

        #region Misc
        private bool CheckNullChoiceGUIComponents(bool showErrorLog = true)
        {
            bool isNull = (questionAndChoicesPanel == null) || (questionText == null) || (choicesArea == null) || (choiceButtonPrefab == null);
            if (isNull && showErrorLog)
            {
                if (questionAndChoicesPanel == null)
                {
                    Debug.LogError("Couldn't access to QuestionAndChoices Panel ");
                }
                if (questionText == null)
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
    [Serializable]
    public class BackgroundList : ReorderableArray<Sprite> { }
    [Serializable]
    public class AudioClipList : ReorderableArray<AudioClip> { }
    [Serializable]
    public class CharacterSprite : ReorderableArray<Sprite> { }
}
