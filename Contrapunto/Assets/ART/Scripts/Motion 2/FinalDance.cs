using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FinalDance : MonoBehaviour
{
    public static FinalDance instance;

    [System.Serializable]
    public class VideoReplaceEntry
    {
        public VideoPlayer player;    // VideoPlayer que ya tiene targetTexture asignado
        public VideoClip newClip;     // Nuevo clip a asignar
    }

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

    [Header("Videos a reactivar y cambiar clip (en paralelo)")]
    public List<VideoReplaceEntry> videosAReproducirConCambio = new List<VideoReplaceEntry>();

    [Header("Referencias externas")]
    public MotionDosManager motionDosManager;

    [Header("Video con audio a bajar")]
    public ManagerOpOne managerOpOne;
    public ManagerOpOne managerOpTwo;
    public ManagerOpOne managerOpThree;
    public ManagerOpOne managerOpFour;// Asignalo desde el inspector

    [Header("Cambio de Material")]
    public Renderer objetoRenderer;      // Objeto al que se le cambia el material
    public Material nuevoMaterial;       // Material nuevo a aplicar

    [Header("Managers de Visibilidad")]
    [Tooltip("Arrastrá acá cada VideoVisibilityManagerAdvanced (o VideoVisibilityManager) que quieras activar")]
    public List<VideoVisibilityManagerAdvanced> visibilityManagers;

    void Awake()
    {
        instance = this;

        // Nos aseguramos de suscribirnos solo una vez
        if (videoFelicitaciones != null)
        {
            videoFelicitaciones.loopPointReached -= VolverAlMapa;
            videoFelicitaciones.loopPointReached += VolverAlMapa;
        }
    }

    public void FinDelJuego()
    {
        spawner.DetenerSpawner();
        inputManager.enabled = false;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Fade out del audio del VideoPlayer de fondo
        if (managerOpOne != null && managerOpOne.videoFondo != null)
        {
            StartCoroutine(FadeOutVideoAudio(managerOpOne.videoFondo, 1f));
        }

        if (managerOpTwo != null && managerOpTwo.videoFondo != null)
        {
            StartCoroutine(FadeOutVideoAudio(managerOpTwo.videoFondo, 1f));
        }

        if (managerOpThree != null && managerOpThree.videoFondo != null)
        {
            StartCoroutine(FadeOutVideoAudio(managerOpThree.videoFondo, 1f));
        }

        if (managerOpFour != null && managerOpFour.videoFondo != null)
        {
            StartCoroutine(FadeOutVideoAudio(managerOpFour.videoFondo, 1f));
        }

        // Cambiar el material del objeto
        if (objetoRenderer != null && nuevoMaterial != null)
        {
            objetoRenderer.material = nuevoMaterial;
        }

        // Mostrar felicitaciones y reproducir video
        canvasFelicitaciones.SetActive(true);
        rawImageVideo.texture = videoFelicitaciones.targetTexture;
        videoFelicitaciones.gameObject.SetActive(true);
        rawImageVideo.gameObject.SetActive(true);
        videoFelicitaciones.Play();
    }

    void VolverAlMapa(VideoPlayer vp)
    {
        // Desuscribirse para no volver a llamar
        vp.loopPointReached -= VolverAlMapa;

        // Ocultar UI de felicitaciones y DDR
        canvasFelicitaciones.SetActive(false);
        canvasDDR.SetActive(false);

        // Reactivar al jugador y objetos del mapa
        player.SetActive(true);
        objetoParaHabilitar.SetActive(true);

        // Reproducir narración una sola vez
        if (audio2Mili != null)
        {
            NarrationManager.Instance.PlayNarration(audio2Mili);
        }

        // Fade-in del nuevo audio
        if (audioSourceParaFadeIn != null && nuevoAudioClip != null)
        {
            audioSourceParaFadeIn.clip = nuevoAudioClip;
            audioSourceParaFadeIn.volume = 0f;
            audioSourceParaFadeIn.Play();
            StartCoroutine(FadeInAudio(audioSourceParaFadeIn, 1f));
        }

        // Restaurar videos gestionados por MotionDosManager
        if (motionDosManager != null)
        {
            motionDosManager.RestaurarTodosVideos();
        }

        // 1) Reactivar videos existentes
        foreach (var video in videosAReactivar)
        {
            if (video != null)
                video.Play();
        }

        // 2) Cambiar clips y reactivar en paralelo
        foreach (var entry in videosAReproducirConCambio)
        {
            if (entry.player != null && entry.newClip != null)
            {
                entry.player.clip = entry.newClip;
                entry.player.Play();
            }
        }

        // 3) Activar los VideoVisibilityManagers
        foreach (var vm in visibilityManagers)
        {
            if (vm != null)
                vm.enabled = true;
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
