using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace TamCam.Intro
{
    public class IntroController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI contentText = null;
        [SerializeField] private IntroStory story = null;
        [SerializeField] private AudioSource bgmAudioSource = null;
        AudioSource audioSource;


        Coroutine fadeInCoroutine, fadeOutCoroutine, introFadeCoroutine;
        private Queue<IntroDialogue> dialogueQueue;
        IntroDialogue previousDialogue;

        bool isFadingIn = false;
        bool isFadingOut = false;

        private void Start()
        {
            audioSource = GetComponent<AudioSource>();
            if (story == null)
            {
                Debug.LogWarning("Story file is missing");
                return;
            }
            dialogueQueue = new Queue<IntroDialogue>(story.dialogues);
            if (dialogueQueue.Count == 0)
            {
                Debug.LogWarning("Story doesn't have any dialogue");
                return;
            }
            HandleDialogue();
        }

        private void Update()
        {
            if (Input.GetMouseButtonUp(0))
            {
                HandleDialogue();
            }
        }

        private void HandleDialogue()
        {
            //Handle Fading In text transistion
            if (isFadingIn)
            {
                if (fadeInCoroutine != null)
                {
                    StopCoroutine(fadeInCoroutine);
                    fadeInCoroutine = null;
                }
                isFadingIn = false;
                ShowNormalDialogue(previousDialogue.content);
                return;
            }
            //Handle Fading Out text transistion
            if (isFadingOut)
            {

                if (introFadeCoroutine != null)
                {
                    StopCoroutine(introFadeCoroutine);
                    introFadeCoroutine = null;
                }

                if (fadeOutCoroutine != null)
                {
                    StopCoroutine(fadeOutCoroutine);
                    fadeOutCoroutine = null;
                }

                isFadingOut = false;
                if (previousDialogue.isFadedIn)
                {
                    ShowFadedInDialogue(previousDialogue.content, previousDialogue.fadedInDuration);
                }
                else
                {
                    ShowNormalDialogue(previousDialogue.content);
                }
                return;
            }

            //Handle completed text transistion
            IntroDialogue currentDialogue = dialogueQueue.Peek();

            HandleDialogueText(currentDialogue);
            HandleDialogueBGM(currentDialogue);
            HandleDialogueSFX(currentDialogue);

            previousDialogue = dialogueQueue.Dequeue();
        }

        private void HandleDialogueSFX(IntroDialogue currentDialogue)
        {
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            if (currentDialogue.SFX != null)
            {
                audioSource.PlayOneShot(currentDialogue.SFX.clip, currentDialogue.SFX.volume);
            }
        }

        private void HandleDialogueBGM(IntroDialogue currentDialogue)
        {
            if (currentDialogue.bgm != null)
            {
                Debug.Log(bgmAudioSource.isPlaying);
                if (bgmAudioSource.isPlaying)
                {
                    if (currentDialogue.startOverBGM)
                    {
                        PlayBGM(currentDialogue.bgm);
                    }
                }
                else
                {
                    PlayBGM(currentDialogue.bgm);
                }
            }
        }

        private void PlayBGM(Audio audio)
        {
            Debug.Log("PLAY BGM");
            bgmAudioSource.clip = audio.clip;
            bgmAudioSource.loop = true;
            bgmAudioSource.volume = audio.volume;
            bgmAudioSource.Play();
        }

        private void HandleDialogueText(IntroDialogue currentDialogue)
        {
            contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, 1f);
            if (previousDialogue != null && previousDialogue.isFadedOut)
            {
                if (currentDialogue.isFadedIn)
                {
                    HandleFadedOutDialogue(currentDialogue.content, previousDialogue.fadedOutDuration, currentDialogue.fadedInDuration);
                }
                else
                {
                    HandleFadedOutDialogue(currentDialogue.content, previousDialogue.fadedOutDuration);
                }
            }
            else
            {
                if (!currentDialogue.isFadedIn)
                {
                    ShowNormalDialogue(currentDialogue.content);
                }
                else
                {
                    ShowFadedInDialogue(currentDialogue.content, currentDialogue.fadedInDuration);
                }
            }
        }

        private void ShowNormalDialogue(string text)
        {
            contentText.color = new Color(contentText.color.r, contentText.color.g, contentText.color.b, 1f);
            contentText.text = text;
        }
        private void ShowFadedInDialogue(string text, float fadedInDuration)
        {
            fadeInCoroutine = StartCoroutine(FadeInTextCoroutine(text, fadedInDuration, contentText));
        }
        private void HandleFadedOutDialogue(string text, float fadedOutDuration, float fadedInDuration)
        {
            introFadeCoroutine = StartCoroutine(IntroFade(text, fadedOutDuration, fadedInDuration, contentText));
        }
        private void HandleFadedOutDialogue(string text, float fadedOutDuration)
        {
            introFadeCoroutine = StartCoroutine(IntroFade(text, fadedOutDuration, contentText));
        }
        private IEnumerator FadeInTextCoroutine(string text, float duration, TextMeshProUGUI textMesh)
        {
            if (duration > Mathf.Epsilon)
            {
                isFadingIn = true;
                textMesh.text = text;

                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 0f);
                float timeLapsed = 0f;
                while (timeLapsed < duration)
                {
                    timeLapsed += Time.deltaTime;
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, timeLapsed / duration);
                    yield return null;
                }
                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, 1f);
                fadeInCoroutine = null;
                isFadingIn = false;
            }
        }
        private IEnumerator FadeOutTextCoroutine(float duration, TextMeshProUGUI textMesh)
        {
            if (duration > Mathf.Epsilon)
            {
                isFadingOut = true;

                float alpha = 1f;
                float timeLapsed = duration;

                textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha);

                while (timeLapsed > 0f)
                {
                    timeLapsed -= Time.deltaTime;
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, alpha * (timeLapsed / duration));
                    yield return null;
                }
                fadeOutCoroutine = null;
                isFadingOut = false;
            }
        }

        private IEnumerator IntroFade(string text, float fadeOutDuration, float fadeInDuration, TextMeshProUGUI textMesh)
        {
            //Fade out previous dialogue then fade in new dialogue
            yield return fadeOutCoroutine = StartCoroutine(FadeOutTextCoroutine(fadeOutDuration, textMesh));
            yield return fadeInCoroutine = StartCoroutine(FadeInTextCoroutine(text, fadeInDuration, textMesh));
            introFadeCoroutine = null;
        }

        private IEnumerator IntroFade(string text, float fadeOutDuration, TextMeshProUGUI textMesh)
        {
            //Fade out previous dialogue then fade in new dialogue
            yield return fadeOutCoroutine = StartCoroutine(FadeOutTextCoroutine(fadeOutDuration, textMesh));
            ShowNormalDialogue(text);
            introFadeCoroutine = null;
        }

    } 
}
