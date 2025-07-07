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
    public TextMeshProUGUI extraText;  // ya lo tenías

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
        if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
        subtitleCoroutine = StartCoroutine(PlayNarrationWithDelay(clip, null));
    }

    public void PlayNarration(AudioClip clip, Action onComplete)
    {
        if (clip == null) return;
        lastNarrationClip = clip;
        if (subtitleCoroutine != null) StopCoroutine(subtitleCoroutine);
        subtitleCoroutine = StartCoroutine(PlayNarrationWithDelay(clip, onComplete));
    }

    private IEnumerator PlayNarrationWithDelay(AudioClip clip, Action onComplete = null)
    {
        // 1) Fade in del fondo de subtítulos
        if (subtitleBackgroundGroup != null)
            yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 0f, 1f, backgroundFadeDuration));

        // 2) Reproducir audio
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        // 3) Iniciar parpadeo de extraText
        if (extraText != null)
        {
            // Asegurarse de que empiece invisible
            var c = extraText.color;
            extraText.color = new Color(c.r, c.g, c.b, 0f);
            extraText.gameObject.SetActive(true);

            blinkCoroutine = StartCoroutine(BlinkExtraText());
        }

        // 4) Mostrar subtítulos normales
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
            // Si no hay subtítulos definidos
            yield return new WaitForSeconds(clip.length);
            if (subtitleBackgroundGroup != null)
                yield return StartCoroutine(FadeCanvasGroup(subtitleBackgroundGroup, 1f, 0f, backgroundFadeDuration));
        }

        // 5) Terminado el audio: asegurar que el blink se detenga
        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        // Dejar extraText oculto al final
        if (extraText != null)
            extraText.gameObject.SetActive(false);

        onComplete?.Invoke();
    }

    // Parpadeo: fade in ? 1s ? fade out ? 2s, mientras siga isPlayingNarration
    private IEnumerator BlinkExtraText()
    {
        while (isPlayingNarration)
        {
            // Fade in
            yield return StartCoroutine(FadeTextAlpha(extraText, 0f, 1f, backgroundFadeDuration));
            // Queda 1 segundo
            yield return new WaitForSeconds(1f);
            // Fade out
            yield return StartCoroutine(FadeTextAlpha(extraText, 1f, 0f, backgroundFadeDuration));
            // Espera 2 segundos antes de volver a aparecer
            yield return new WaitForSeconds(2f);
        }
    }

    // Helper para fade de alpha en TextMeshProUGUI
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
