using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FinalDance : MonoBehaviour
{
    public static FinalDance instance;

    [Header("Referencias DDR")]
    public FlechaSpawner spawner;
    public InputFlechas inputManager;
    public GameObject canvasDDR;

    [Header("Video Felicitaciones")]
    public GameObject canvasFelicitaciones;
    public VideoPlayer videoFelicitaciones;
    public RawImage rawImageVideo;

    [Header("Vuelta al mapa")]
    public GameObject player;
    public GameObject objetoParaHabilitar;
    public AudioClip audio2Mili;

    [Header("Audio nuevo con fade-in")]
    public AudioSource audioSourceParaFadeIn;
    public AudioClip nuevoAudioClip;

    [Header("Videos que se deben reactivar al volver al mapa")]
    public List<VideoPlayer> videosAReactivar;

    [Header("Referencias externas")]
    public MotionDosManager motionDosManager;

    [Header("Video con audio a bajar")]
    public ManagerOpOne managerOpOne; // Asignalo desde el inspector

    [Header("Cambio de Material")]
    public Renderer objetoRenderer;      // Objeto al que se le cambia el material
    public Material nuevoMaterial;       // Material nuevo a aplicar

    void Awake()
    {
        instance = this;
    }

    public void FinDelJuego()
    {
        spawner.DetenerSpawner();
        inputManager.enabled = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // ?? Fade out del audio del VideoPlayer
        if (managerOpOne != null && managerOpOne.videoFondo != null)
        {
            StartCoroutine(FadeOutVideoAudio(managerOpOne.videoFondo, 1f));
        }

        // ?? Cambiar el material del objeto
        if (objetoRenderer != null && nuevoMaterial != null)
        {
            objetoRenderer.material = nuevoMaterial;
        }

        canvasFelicitaciones.SetActive(true);
        rawImageVideo.texture = videoFelicitaciones.targetTexture;
        videoFelicitaciones.gameObject.SetActive(true);
        rawImageVideo.gameObject.SetActive(true);

        videoFelicitaciones.Play();
        videoFelicitaciones.loopPointReached += VolverAlMapa;
    }

    void VolverAlMapa(VideoPlayer vp)
    {
        canvasFelicitaciones.SetActive(false);
        canvasDDR.SetActive(false);

        player.SetActive(true);
        objetoParaHabilitar.SetActive(true);

        if (audio2Mili != null)
        {
            NarrationManager.Instance.PlayNarration(audio2Mili);
        }

        // ?? Fade in del nuevo audio
        if (audioSourceParaFadeIn != null && nuevoAudioClip != null)
        {
            audioSourceParaFadeIn.clip = nuevoAudioClip;
            audioSourceParaFadeIn.volume = 0f;
            audioSourceParaFadeIn.Play();
            StartCoroutine(FadeInAudio(audioSourceParaFadeIn, 1f));
        }

        if (motionDosManager != null)
        {
            motionDosManager.RestaurarTodosVideos();
        }

        foreach (var video in videosAReactivar)
        {
            if (video != null)
                video.Play();
        }
    }

    private IEnumerator FadeInAudio(AudioSource source, float duration)
    {
        float targetVolume = 0.1f;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, t / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }

    private IEnumerator FadeOutVideoAudio(VideoPlayer video, float duration)
    {
        float startVolume = video.GetDirectAudioVolume(0);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, t / duration);
            video.SetDirectAudioVolume(0, newVolume);
            yield return null;
        }

        video.SetDirectAudioVolume(0, 0f);
    }
}
