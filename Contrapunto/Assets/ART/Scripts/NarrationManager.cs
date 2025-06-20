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

    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        lastNarrationClip = clip;

        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        subtitleCoroutine = StartCoroutine(PlayNarrationWithDelay(clip, null));
    }

    public void PlayNarration(AudioClip clip, Action onComplete)
    {
        if (clip == null) return;

        lastNarrationClip = clip;

        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        subtitleCoroutine = StartCoroutine(PlayNarrationWithDelay(clip, onComplete));
    }

    private IEnumerator PlayNarrationWithDelay(AudioClip clip, Action onComplete = null)
    {
        // Mostrar fondo primero
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 0f, 1f, backgroundFadeDuration));

        // Reproducir audio
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        // Mostrar subtítulo
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
            // Si no hay subtítulo pero queremos hacer algo al final
            yield return new WaitForSeconds(clip.length);
            if (subtitleBackgroundGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));
        }

        onComplete?.Invoke();
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
