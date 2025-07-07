using UnityEngine;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class CilindroInteract : MonoBehaviour
{
    [System.Serializable]
    public class VideoReplaceEntry
    {
        public VideoPlayer player;      // VideoPlayer asociado al RawImage
        public VideoClip newClip;       // Nuevo clip a asignar
    }

    [Header("Referencias en Inspector")]
    public VideoPlayer videoPlayer;
    public GameObject player;
    public GameObject logoTp;
    public Canvas canvasCilindro;

    [Header("Cambio de material")]
    public Renderer objetoRenderer;
    public Material nuevoMaterial;

    [Header("Videos que deben reanudarse al finalizar")]
    public List<VideoPlayer> videosAReactivar = new List<VideoPlayer>();

    [Header("RawImages y videos a cambiar (en paralelo)")]
    public List<VideoReplaceEntry> rawImageVideosAReemplazar = new List<VideoReplaceEntry>();

    [Header("FadeScreen para detener glitches")]
    public FadeScreen fadeScreen;

    [Header("Sonidos ambiente")]
    public AudioSource sonidoAmbienteActual;
    public AudioSource sonidoAmbienteNuevo;
    public float fadeDuration = 2f;

    [Header("Narración al finalizar")]
    public AudioClip audio3Mili;

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput piSystem;

    private bool puedeInteractuar = true;

    void Start()
    {
        fpsController = player.GetComponent<FirstPersonController>();
        starterInputs = player.GetComponent<StarterAssetsInputs>();
        piSystem = player.GetComponent<PlayerInput>();

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.gameObject.SetActive(false);

        if (logoTp != null)
            logoTp.SetActive(false);

        if (canvasCilindro != null)
            canvasCilindro.gameObject.SetActive(false);
    }

    void OnMouseDown()
    {
        if (!puedeInteractuar) return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider == GetComponent<Collider>())
        {
            if (MotionManager.Instance.AllCubesCollected())
            {
                StartCoroutine(FadeOutAndStop(sonidoAmbienteActual, fadeDuration));

                videoPlayer.gameObject.SetActive(true);
                videoPlayer.Play();

                StartCoroutine(CambiarMaterialEnMitadDelVideo());

                fpsController.enabled = false;
                starterInputs.enabled = false;
                piSystem.enabled = false;
            }
        }
    }

    IEnumerator CambiarMaterialEnMitadDelVideo()
    {
        while (!videoPlayer.isPrepared)
            yield return null;

        double mitad = videoPlayer.length / 2.0;
        yield return new WaitForSeconds((float)mitad);

        if (objetoRenderer != null && nuevoMaterial != null)
        {
            objetoRenderer.material = nuevoMaterial;
        }
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        vp.Stop();
        vp.gameObject.SetActive(false);

        StartCoroutine(FadeInAudio(sonidoAmbienteNuevo, 0.03f, fadeDuration));

        fpsController.enabled = true;
        starterInputs.enabled = true;
        piSystem.enabled = true;

        puedeInteractuar = false;

        if (logoTp != null)
            logoTp.SetActive(true);

        if (canvasCilindro != null)
            canvasCilindro.gameObject.SetActive(false);

        RestaurarVideos();

        // Nueva narración al finalizar el video
        if (audio3Mili != null)
        {
            NarrationManager.Instance.PlayNarration(audio3Mili);
        }
    }

    void RestaurarVideos()
    {
        if (fadeScreen != null)
        {
            fadeScreen.StopAllGlitching();
        }

        foreach (var vp in videosAReactivar)
        {
            if (vp != null)
            {
                vp.Play();
            }
        }

        foreach (var entry in rawImageVideosAReemplazar)
        {
            if (entry.player != null && entry.newClip != null)
            {
                entry.player.clip = entry.newClip;
                entry.player.Play();
            }
        }
    }

    IEnumerator FadeOutAndStop(AudioSource audioSource, float duration)
    {
        if (audioSource == null) yield break;

        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

    IEnumerator FadeInAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        if (audioSource == null) yield break;

        audioSource.volume = 0f;
        audioSource.Play();

        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, time / duration);
            yield return null;
        }

        audioSource.volume = targetVolume;
    }
}
