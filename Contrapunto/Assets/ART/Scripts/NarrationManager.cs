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

    [System.Serializable]
    public class ClipSubtitle
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
    }

    [Header("Definición de subtítulos")]
    public List<ClipSubtitle> subtitles = new List<ClipSubtitle>();

    private Coroutine subtitleCoroutine;

    [HideInInspector]
    public bool repeatEnabled = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Detectar fin de reproducción
        if (isPlayingNarration && !narrationSource.isPlaying)
            isPlayingNarration = false;

        // Repetir última narración con E
        if (repeatEnabled &&
            Input.GetKeyDown(KeyCode.E) &&
            !isPlayingNarration &&
            lastNarrationClip != null)
        {
            PlayNarration(lastNarrationClip);
        }
    }

    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        lastNarrationClip = clip;
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        ShowSubtitleForClip(clip);
    }

    public void PlayNarration(AudioClip clip, Action onComplete)
    {
        if (clip == null) return;

        lastNarrationClip = clip;
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        ShowSubtitleForClip(clip, onComplete);
    }

    private void ShowSubtitleForClip(AudioClip clip, Action onComplete = null)
    {
        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        var entry = subtitles.Find(s => s.clip == clip);
        if (entry != null && subtitleText != null)
        {
            subtitleCoroutine = StartCoroutine(SubtitleRoutine(entry.subtitle, clip.length, onComplete));
        }
        else if (onComplete != null)
        {
            // Si no hay subtítulo pero queremos que igual se llame al final
            StartCoroutine(CallbackAfterDelay(clip.length, onComplete));
        }
    }

    private IEnumerator SubtitleRoutine(string text, float duration, Action onComplete = null)
    {
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 0f, 1f, backgroundFadeDuration));

        yield return new WaitForSeconds(backgroundFadeDuration);

        subtitleText.text = text;
        subtitleText.gameObject.SetActive(true);

        yield return new WaitForSeconds(duration);

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);

        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));

        onComplete?.Invoke();
    }

    private IEnumerator CallbackAfterDelay(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
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
