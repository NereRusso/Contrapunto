using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine.Video;

public class MotionDosManager : MonoBehaviour
{
    public FirstPersonController playerController;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;
    public AudioClip audio1Mili;

    [Header("Videos que se deben pausar al iniciar")]
    public List<VideoPlayer> videosPausados;

    [Header("Videos que deben arrancar en glitch (bajo fps)")]
    public List<VideoPlayer> videosConGlitch;
    public float glitchInterval = 0.3f;

    private List<Coroutine> glitchCoroutines = new List<Coroutine>();

    private void Start()
    {
        // ?? Frenar videos antes del fade
        foreach (var vp in videosPausados)
        {
            if (vp != null)
                vp.Pause();
        }

        // ?? Activar glitch en videos antes del fade
        foreach (var vp in videosConGlitch)
        {
            if (vp != null)
            {
                vp.Pause();
                Coroutine c = StartCoroutine(PlaySteppedVideo(vp));
                glitchCoroutines.Add(c);
            }
        }

        // Luego recién hacemos el fade in
        playerController.enabled = false;
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeCanvas.alpha = 1;
        fadeCanvas.blocksRaycasts = true;

        yield return new WaitForSeconds(delayBeforeFade);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = 1 - (timer / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;

        playerController.enabled = true;

        if (audio1Mili != null)
        {
            NarrationManager.Instance.PlayNarration(audio1Mili);
        }
    }

    private IEnumerator PlaySteppedVideo(VideoPlayer vp)
    {
        while (true)
        {
            vp.StepForward();
            yield return new WaitForSeconds(glitchInterval);
        }
    }

    public void RestaurarTodosVideos()
    {
        foreach (var c in glitchCoroutines)
        {
            if (c != null)
                StopCoroutine(c);
        }

        glitchCoroutines.Clear();

        foreach (var vp in videosPausados)
        {
            if (vp != null)
                vp.Play();
        }

        foreach (var vp in videosConGlitch)
        {
            if (vp != null)
                vp.Play();
        }
    }
}
