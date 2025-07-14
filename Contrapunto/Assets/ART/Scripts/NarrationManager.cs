// NarrationManager.cs
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

    [Header("Efecto de distancia")]
    [Range(0, 1)] public float distantVolumeFactor = 0.5f;
    [Range(0, 1)] public float distantSpatialBlend = 1f;
    [Range(0, 1)] public float normalSpatialBlend = 0f;

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

    [HideInInspector] public bool repeatEnabled = true;

    // Cola de narraciones: clip, callback, isDistant
    private Queue<(AudioClip clip, Action onComplete, bool isDistant)> narrationQueue
        = new Queue<(AudioClip, Action, bool)>();
    private bool isQueueProcessing = false;

    private Coroutine blinkCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isPlayingNarration && !narrationSource.isPlaying)
            isPlayingNarration = false;

        if (repeatEnabled && Input.GetKeyDown(KeyCode.E) && !isPlayingNarration)
        {
            AudioClip clipToRepeat = lastNarrationClip ?? narrationSource.clip;
            if (clipToRepeat != null)
                PlayNarration(clipToRepeat, false);
        }
    }

    // --- INTERFAZ PÚBLICA ---
    public void PlayNarration(AudioClip clip)
        => PlayNarration(clip, false);

    public void PlayNarration(AudioClip clip, bool isDistant)
        => EnqueueNarration(clip, null, isDistant);

    public void PlayNarration(AudioClip clip, Action onComplete, bool isDistant)
        => EnqueueNarration(clip, onComplete, isDistant);

    public bool IsNarrationPlaying() => isPlayingNarration;

    // --- COLA Y PROCESAMIENTO ---
    private void EnqueueNarration(AudioClip clip, Action onComplete, bool isDistant)
    {
        if (clip == null) return;
        narrationQueue.Enqueue((clip, onComplete, isDistant));
        if (!isQueueProcessing)
            StartCoroutine(ProcessNarrationQueue());
    }

    private IEnumerator ProcessNarrationQueue()
    {
        isQueueProcessing = true;
        while (narrationQueue.Count > 0)
        {
            var (clip, onComplete, isDistant) = narrationQueue.Dequeue();
            lastNarrationClip = clip;
            yield return StartCoroutine(
                PlayNarrationWithDelay(clip, onComplete, isDistant));
        }
        isQueueProcessing = false;
    }

    private IEnumerator PlayNarrationWithDelay(
        AudioClip clip, Action onComplete = null, bool isDistant = false)
    {
        // Fade in subtítulos
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(
                FadeCanvasGroup(subtitleBackgroundGroup, 0f, 1f, backgroundFadeDuration));

        // Ajuste de volumen y spatialBlend
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.volume = isDistant ? distantVolumeFactor : 1f;
        narrationSource.spatialBlend = isDistant ? distantSpatialBlend : normalSpatialBlend;
        narrationSource.Play();
        isPlayingNarration = true;

        // Extra text blink
        if (extraText != null)
        {
            Color c = extraText.color;
            extraText.color = new Color(c.r, c.g, c.b, 0f);
            extraText.gameObject.SetActive(true);
            blinkCoroutine = StartCoroutine(BlinkExtraText());
        }

        // Subtítulo
        var entry = subtitles.Find(s => s.clip == clip);
        if (entry != null && subtitleText != null)
        {
            subtitleText.text = entry.subtitle;
            subtitleText.gameObject.SetActive(true);
        }

        yield return new WaitForSeconds(clip.length);

        // Limpio subtítulos
        if (subtitleText != null)
        {
            subtitleText.text = "";
            subtitleText.gameObject.SetActive(false);
        }
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(
                FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));

        // Apago blink
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);
        if (extraText != null)
            extraText.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    // --- ANIMACIONES AUXILIARES ---
    private IEnumerator BlinkExtraText()
    {
        while (isPlayingNarration)
        {
            yield return StartCoroutine(
                FadeTextAlpha(extraText, 0f, 1f, backgroundFadeDuration));
            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(
                FadeTextAlpha(extraText, 1f, 0f, backgroundFadeDuration));
            yield return new WaitForSeconds(2f);
        }
    }

    private IEnumerator FadeTextAlpha(
        TextMeshProUGUI text, float from, float to, float duration)
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

    private IEnumerator FadeCanvasGroup(
        CanvasGroup group, float start, float end, float duration)
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
