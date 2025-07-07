using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("Narration")]
    public AudioSource narrationSource;
    private AudioClip lastNarrationClip;
    private bool isPlayingNarration;

    [Header("Subtítulos UI")]
    public TextMeshProUGUI subtitleText;
    public CanvasGroup subtitleBackgroundGroup;
    public float backgroundFadeDuration = 0.5f;

    [Header("Extra Text UI")]
    [Tooltip("Arrastrá acá el segundo TextMeshProUGUI que querés que parpadee")]
    public TextMeshProUGUI extraText;

    [System.Serializable]
    public class ClipSubtitle
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
    }

    [Header("Definición de subtítulos")]
    public List<ClipSubtitle> subtitles = new List<ClipSubtitle>();

    private Coroutine subtitleCoroutine;
    private Coroutine blinkCoroutine;

    [HideInInspector]
    public bool repeatEnabled = true;

    // NUEVO: Cola de narraciones
    private Queue<(AudioClip clip, Action onComplete)> narrationQueue = new Queue<(AudioClip, Action)>();
    private bool isQueueProcessing = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isPlayingNarration && !narrationSource.isPlaying)
            isPlayingNarration = false;

        if (repeatEnabled &&
            Input.GetKeyDown(KeyCode.E) &&
            !isPlayingNarration &&
            lastNarrationClip != null)
        {
            PlayNarration(lastNarrationClip);
        }
    }

    // PUBLIC INTERFACE
    public void PlayNarration(AudioClip clip)
    {
        EnqueueNarration(clip, null);
    }

    public void PlayNarration(AudioClip clip, Action onComplete)
    {
        EnqueueNarration(clip, onComplete);
    }

    public bool IsNarrationPlaying()
    {
        return isPlayingNarration;
    }

    // QUEUE SYSTEM
    private void EnqueueNarration(AudioClip clip, Action onComplete)
    {
        if (clip == null) return;

        narrationQueue.Enqueue((clip, onComplete));

        if (!isQueueProcessing)
            StartCoroutine(ProcessNarrationQueue());
    }

    private IEnumerator ProcessNarrationQueue()
    {
        isQueueProcessing = true;

        while (narrationQueue.Count > 0)
        {
            var (clip, onComplete) = narrationQueue.Dequeue();
            lastNarrationClip = clip;
            yield return StartCoroutine(PlayNarrationWithDelay(clip, onComplete));
        }

        isQueueProcessing = false;
    }

    // NARRATION WITH SUBTITLES + EFFECTS
    private IEnumerator PlayNarrationWithDelay(AudioClip clip, Action onComplete = null)
    {
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 0f, 1f, backgroundFadeDuration));

        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        if (extraText != null)
        {
            var c = extraText.color;
            extraText.color = new Color(c.r, c.g, c.b, 0f);
            extraText.gameObject.SetActive(true);
            blinkCoroutine = StartCoroutine(BlinkExtraText());
        }

        var entry = subtitles.Find(s => s.clip == clip);
        if (entry != null && subtitleText != null)
        {
            subtitleText.text = entry.subtitle;
            subtitleText.gameObject.SetActive(true);

            yield return new WaitForSeconds(clip.length);

            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);

            if (subtitleBackgroundGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(clip.length);
            if (subtitleBackgroundGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));
        }

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        if (extraText != null)
            extraText.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    // ANIMACIONES Y FADES
    private IEnumerator BlinkExtraText()
    {
        while (isPlayingNarration)
        {
            yield return StartCoroutine(FadeTextAlpha(extraText, 0f, 1f, backgroundFadeDuration));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(FadeTextAlpha(extraText, 1f, 0f, backgroundFadeDuration));
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator FadeTextAlpha(TextMeshProUGUI text, float from, float to, float duration)
    {
        float elapsed = 0f;
        Color c = text.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float a = Mathf.Lerp(from, to, elapsed / duration);
            text.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }
        text.color = new Color(c.r, c.g, c.b, to);
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup group, float start, float end, float duration)
    {
        float elapsed = 0f;
        group.alpha = start;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(start, end, elapsed / duration);
            yield return null;
        }
        group.alpha = end;
    }
}
