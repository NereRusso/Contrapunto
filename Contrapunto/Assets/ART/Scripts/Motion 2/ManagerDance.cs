using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class ManagerDance : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject canvasDDR;
    public GameObject player;
    public RawImage rawImage;

    [Header("Videos")]
    public VideoPlayer videoIntro;
    public VideoPlayer videoLoop;
    public VideoPlayer videoCarga;
    public VideoPlayer videoFondo;

    [Header("UI")]
    public GameObject pantallaCanciones;

    [Header("Fade")]
    public RawImage fadeImage;
    public float fadeDuration = 1f;

    private bool canPressEnter = false;

    void Start()
    {
        canvasDDR.SetActive(false);

        videoIntro.gameObject.SetActive(false);
        videoLoop.gameObject.SetActive(false);
        videoCarga.gameObject.SetActive(false);
        videoFondo.gameObject.SetActive(false);

        pantallaCanciones.SetActive(false);

        fadeImage.color = new Color(0, 0, 0, 0); // transparencia al inicio
    }

    public void ActivarDDR()
    {
        player.SetActive(false);
        canvasDDR.SetActive(true);

        videoIntro.gameObject.SetActive(true);
        rawImage.texture = videoIntro.targetTexture;

        videoIntro.loopPointReached += OnIntroFinished;

        videoIntro.Prepare();
        StartCoroutine(WaitForPrepared(videoIntro));
    }

    void OnIntroFinished(VideoPlayer vp)
    {
        videoLoop.gameObject.SetActive(true);
        rawImage.texture = videoLoop.targetTexture;
        videoLoop.isLooping = true;

        videoLoop.Prepare();
        StartCoroutine(WaitForPrepared(videoLoop));

        videoIntro.Stop();
        videoIntro.gameObject.SetActive(false);

        canPressEnter = true;
    }

    void Update()
    {
        if (canPressEnter && Input.GetKeyDown(KeyCode.Return))
        {
            canPressEnter = false;
            StartCoroutine(FadeAndSwitchVideo(videoLoop, videoCarga, OnCargaFinished));
        }
    }

    void OnCargaFinished()
    {
        videoFondo.gameObject.SetActive(true);
        rawImage.texture = videoFondo.targetTexture;
        videoFondo.isLooping = true;

        videoFondo.Prepare();
        StartCoroutine(WaitForPrepared(videoFondo));

        videoCarga.Stop();
        videoCarga.gameObject.SetActive(false);

        pantallaCanciones.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    IEnumerator FadeAndSwitchVideo(VideoPlayer current, VideoPlayer next, System.Action onFinish)
    {
        float timer = 0f;
        float startVolume = current.GetDirectAudioVolume(0); // Volumen actual del video

        // FADE IN (a negro + BAJAR VOLUMEN)
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);

            // Fade visual
            float alpha = Mathf.Lerp(0f, 1f, t);
            fadeImage.color = new Color(0, 0, 0, alpha);

            // Fade de volumen
            current.SetDirectAudioVolume(0, Mathf.Lerp(startVolume, 0f, t));

            yield return null;
        }

        current.SetDirectAudioVolume(0, 0f); // Asegurar que quede en 0 justo antes del cambio

        // CAMBIO DE VIDEO
        next.gameObject.SetActive(true);
        rawImage.texture = next.targetTexture;

        next.loopPointReached += vp => onFinish?.Invoke();
        next.Prepare();
        while (!next.isPrepared)
            yield return null;

        next.Play();
        current.Stop();
        current.gameObject.SetActive(false);

        // FADE OUT (de negro a visible, sin tocar volumen porque es otro video)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    IEnumerator WaitForPrepared(VideoPlayer vp)
    {
        while (!vp.isPrepared)
            yield return null;

        vp.Play();
    }
}
