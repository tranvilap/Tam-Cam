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
        [SerializeField] TextMeshProUGUI contentTextGUI = null;
        [SerializeField] Route firstRoute = null;
        [SerializeField] float typeWriterSpeed = 0.02f;

        [SerializeField] Image completeBubble = null;

        Route isPlayingRoute;
        int currentDialogueIndex = 0;

        Coroutine fadeIn, fadeOut, typeWriter;

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
            if (isPlayingRoute == null)
            {
                isPlayingRoute = firstRoute;
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
                    if (currentDialogueIndex >= isPlayingRoute.Dialogues.Count)
                    {
                        Debug.Log("END ROUTE");
                    }
                    else
                    {
                        ProceedNextDialogue();
                    }
                }
            }
        }

        private void ProceedNextDialogue()
        {
            Dialogue currentDialogue = isPlayingRoute.Dialogues[currentDialogueIndex];
            DisplayContent(currentDialogue);
            currentDialogueIndex++;
        }

        private void DisplayContent(Dialogue dialogue)
        {

            Debug.Log("Display Content");

            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(false);
            }

            if (dialogue.IntroTextEffect == IntroTextEffect.Normal)
            {
                DisplayText_Normal(contentTextGUI, dialogue.Content, dialogue.IsAdditive);
                return;
            }

            isTextTransitioning = true;
            if (dialogue.IntroTextEffect == IntroTextEffect.Typewriter)
            {
                typeWriter = StartCoroutine(TypewriterEffect(contentTextGUI, dialogue.Content, typeWriterSpeed, dialogue.IsAdditive));
            }
            else if (dialogue.IntroTextEffect == IntroTextEffect.FadeIn)
            {
                fadeIn = StartCoroutine(FadeInEffect(contentTextGUI, dialogue.Content, dialogue.FadeInDuration, dialogue.IsAdditive));
            }

        }

        private void CompleteCurrentTextTransition()
        {
            if (!isTextTransitioning) { return; }

            isTextTransitioning = false;
            Debug.Log("Complete transition");
            if (fadeIn != null)
            {
                StopCoroutine(fadeIn);
                fadeIn = null;
                string temp = contentTextGUI.text;
                contentTextGUI.text = "";
                contentTextGUI.text = temp;
                contentTextGUI.color = new Color(contentTextGUI.color.r, contentTextGUI.color.g, contentTextGUI.color.b, 1f);
            }
            if (typeWriter != null)
            {
                StopCoroutine(typeWriter);
                typeWriter = null;
                contentTextGUI.text = isPlayingRoute.Dialogues[currentDialogueIndex - 1].Content;
            }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
        }

        private void ChangeRoute(Route route)
        {
            isPlayingRoute = route;
            currentDialogueIndex = 0;
        }


        public void DisplayText_Normal(TextMeshProUGUI textMesh, string text, bool isConjunctive = false)
        {
            if (isConjunctive)
            {
                textMesh.text += text;
            }
            else
            {
                textMesh.text = text;
            }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
        }

        public IEnumerator TypewriterEffect(TextMeshProUGUI textMesh, string text, float speed, bool isAdditive)
        {
            int isSkipping = 0;
            string temp = "";
            if (!isAdditive)
            {
                textMesh.text = "";
            }
            if (speed < 0)
            {
                textMesh.text = text;
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
                        textMesh.text += character;
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
                            textMesh.text = temp;

                        }
                    }
                }
            }
            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
        }

        public IEnumerator FadeInEffect(TextMeshProUGUI textMesh, string text, float duration, bool isAdditive)
        {
            if (duration > Mathf.Epsilon)
            {
                if (isAdditive)
                {
                    int lastIndex = 0;
                    int inTags = 0;
                    foreach (var c in textMesh.text)
                    {
                        if (c.Equals('<')) { inTags++; continue; }
                        if (inTags == 0)
                        {
                            lastIndex++;
                        }
                        if (c.Equals('>')) { inTags--; }
                    }

                    textMesh.text += text;
                    int textLength = lastIndex + text.Length;

                    textMesh.ForceMeshUpdate();

                    Color32[] newVertexColors;
                    var textInfo = textMesh.textInfo;
                    float timeLapsed = 0f;

                    while (timeLapsed < duration)
                    {
                        timeLapsed += Time.deltaTime;
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
                                textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);

                                // This last process could be done to only update the vertex data that has changed as opposed to all of the vertex data but it would require extra steps and knowing what type of renderer is used.
                                // These extra steps would be a performance optimization but it is unlikely that such optimization will be necessary.
                            }
                        }
                        yield return null;
                    }
                }
                else
                {
                    textMesh.text = text;
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0f);
                    float timeLapsed = 0f;
                    while (timeLapsed < duration)
                    {
                        timeLapsed += Time.deltaTime;
                        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, timeLapsed / duration);
                        yield return null;
                    }
                }
            }
            else
            {
                if (isAdditive)
                {
                    textMesh.text += text;
                }
                else
                {
                    textMesh.text = text;
                }
            }

            if (completeBubble != null)
            {
                completeBubble.gameObject.SetActive(true);
            }
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1f);
        }
    }

}
