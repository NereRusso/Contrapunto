using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using StarterAssets;
using UnityEngine.InputSystem;

public class VideoIntroManager : MonoBehaviour
{
    [Header("Asignaciones")]
    public VideoPlayer videoPlayer;
    public RawImage videoScreen;
    public GameObject videoCanvas;
    public GameObject playerController;
    public Camera mainCamera;
    public FadeScreen screenFader;

    [Header("Audios")]
    public AudioSource sonidoAmbiente;

    [Header("Configuración")]
    public float fadeDuration = 1.5f;
    public float waitBeforeFadeOut = 0.5f;
    public float sonidoAmbienteTargetVolume = 0.03f;
    public float sonidoAmbienteFadeTime = 2f;

    // Scripts a desactivar temporalmente
    private StarterAssetsInputs inputScript;
    private FirstPersonController movementScript;
    private PlayerInput playerInput;

    void Start()
    {
        inputScript = playerController.GetComponent<StarterAssetsInputs>();
        movementScript = playerController.GetComponent<FirstPersonController>();
        playerInput = playerController.GetComponent<PlayerInput>();

        if (inputScript != null) inputScript.enabled = false;
        if (movementScript != null) movementScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        playerController.SetActive(false);

        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Prepare();
    }

    void OnPrepared(VideoPlayer vp)
    {
        videoScreen.texture = vp.texture;
        vp.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        StartCoroutine(EndSequence());
    }

    IEnumerator EndSequence()
    {
        videoCanvas.SetActive(false);
        yield return new WaitForSeconds(waitBeforeFadeOut);

        // Activar sonido ambiente con fade
        if (sonidoAmbiente != null)
        {
            sonidoAmbiente.volume = 0f;
            sonidoAmbiente.Play();
            StartCoroutine(FadeInAudio(sonidoAmbiente, sonidoAmbienteTargetVolume, sonidoAmbienteFadeTime));
        }

        // Activar jugador y fade de pantalla
        playerController.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        screenFader.fadeDuration = fadeDuration;
        yield return screenFader.FadeOut();

        // Activar movimiento del jugador inmediatamente
        if (inputScript != null) inputScript.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }

    IEnumerator FadeInAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        float currentTime = 0f;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVolume, currentTime / duration);
            yield return null;
        }
        audioSource.volume = targetVolume;
    }
}
