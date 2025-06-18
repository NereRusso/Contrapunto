using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;

public class ManagerCod1 : MonoBehaviour
{
    [Header("Jugador")]
    public GameObject playerObject;

    [Header("Video Intro")]
    public RawImage videoImage;        // Texture = RenderTexture (IntroRT)
    public VideoPlayer videoPlayer;    // Render Mode = RenderTexture, Target Texture = IntroRT
    public float videoDelay = 1f;

    [Header("Narración")]
    public AudioClip audio1Nere;

    // Referencias al jugador
    FirstPersonController movementScript;
    StarterAssetsInputs inputScript;
    PlayerInput playerInput;

    void Start()
    {
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        // Desactivar controles
        movementScript.enabled = false;
        inputScript.enabled = false;
        playerInput.enabled = false;

        // Asegurarnos de que la RawImage arranque oculta
        videoImage.gameObject.SetActive(false);

        StartCoroutine(PlayIntroVideo());
    }

    IEnumerator PlayIntroVideo()
    {
        yield return new WaitForSeconds(videoDelay);

        // Mostrar el RawImage
        videoImage.gameObject.SetActive(true);

        // Preparar el VideoPlayer
        videoPlayer.Prepare();
        yield return new WaitWhile(() => !videoPlayer.isPrepared);

        // Asegurarnos de que la RawImage use el RenderTexture
        videoImage.texture = videoPlayer.targetTexture;

        // Suscribirse al evento de fin de vídeo
        videoPlayer.loopPointReached += OnVideoFinished;

        // Iniciar reproducción
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        // Nos desuscribimos para evitar llamadas repetidas
        vp.loopPointReached -= OnVideoFinished;

        // Ocultar la RawImage
        videoImage.gameObject.SetActive(false);

        // Llamar a la narración
        if (audio1Nere != null)
            NarrationManager.Instance.PlayNarration(audio1Nere, OnNarrationEnded);
    }

    void OnNarrationEnded()
    {
        // Re-activar controles del jugador
        movementScript.enabled = true;
        inputScript.enabled = true;
        playerInput.enabled = true;
    }
}
