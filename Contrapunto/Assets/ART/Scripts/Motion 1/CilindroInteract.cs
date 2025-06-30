using UnityEngine;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class CilindroInteract : MonoBehaviour
{
    [Header("Referencias en Inspector")]
    public VideoPlayer videoPlayer;
    public GameObject player;
    public GameObject logoTp;
    public Canvas canvasCilindro;

    [Header("Cambio de material")]
    public Renderer objetoRenderer; // El objeto al que le vas a cambiar el material
    public Material nuevoMaterial;  // El nuevo material que querés aplicar

    [Header("Videos que deben reanudarse al finalizar")]
    public List<VideoPlayer> videosAReactivar = new List<VideoPlayer>();

    [Header("FadeScreen para detener glitches")]
    public FadeScreen fadeScreen; // Referencia al script que maneja el glitch

    [Header("Sonidos ambiente")]
    public AudioSource sonidoAmbienteActual;   // El que ya venía sonando
    public AudioSource sonidoAmbienteNuevo;    // El que arranca después del video
    public float fadeDuration = 2f;

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

                // CAMBIO DE MATERIAL
                if (objetoRenderer != null && nuevoMaterial != null)
                {
                    objetoRenderer.material = nuevoMaterial;
                }

                fpsController.enabled = false;
                starterInputs.enabled = false;
                piSystem.enabled = false;
            }
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
    }

    void RestaurarVideos()
    {
        if (fadeScreen != null)
        {
            fadeScreen.StopAllGlitching(); // Detenemos las corutinas que hacían StepForward()
        }

        foreach (var vp in videosAReactivar)
        {
            if (vp != null)
            {
                vp.Play(); // Volvemos a reproducir normalmente
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
        audioSource.volume = startVolume; // Por si lo volvés a usar después
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
