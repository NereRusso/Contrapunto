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
    public AudioClip audioMili1;
    public AudioSource sonidoAmbiente;

    [Header("Configuración")]
    public float fadeDuration = 1.5f;
    public float waitBeforeFadeOut = 0.5f;

    // Scripts a desactivar temporalmente
    private StarterAssetsInputs inputScript;
    private FirstPersonController movementScript;
    private PlayerInput playerInput;

    void Start()
    {
        // Obtener scripts del jugador
        inputScript = playerController.GetComponent<StarterAssetsInputs>();
        movementScript = playerController.GetComponent<FirstPersonController>();
        playerInput = playerController.GetComponent<PlayerInput>();

        // Desactivar control de jugador completo
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

        // Activo la cámara del jugador, pero sin controles todavía
        playerController.SetActive(true);
        mainCamera.gameObject.SetActive(false);

        screenFader.fadeDuration = fadeDuration;
        yield return screenFader.FadeOut();

        // Empieza la narración y el ambiente
        if (audioMili1 != null)
            NarrationManager.Instance.PlayNarration(audioMili1, OnNarrationEnded);

        if (sonidoAmbiente != null)
            sonidoAmbiente.Play();
    }

    void OnNarrationEnded()
    {
        // Recién ahora activo los controles del jugador
        if (inputScript != null) inputScript.enabled = true;
        if (movementScript != null) movementScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }
}
