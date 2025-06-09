using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoIntroManager : MonoBehaviour
{
    [Header("Asignaciones")]
    public VideoPlayer videoPlayer;
    public RawImage videoScreen;
    public GameObject videoCanvas;
    public GameObject playerController;
    public Camera mainCamera;
    public FadeScreen screenFader;

    [Header("Configuración")]
    public float fadeDuration = 1.5f;  // Duración del fade de salida
    public float waitBeforeFadeOut = 0.5f; // Espera antes de empezar el fade-out

    [Header("Audios")]
    public AudioClip audioMili1;
    public AudioSource sonidoAmbiente;

    void Start()
    {
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
        // 1) Apago sólo el VideoCanvas (aparece el FadePanel negro detrás)
        videoCanvas.SetActive(false);

        // 2) Pequeño delay en negro si quieres
        yield return new WaitForSeconds(waitBeforeFadeOut);

        // 3) Activo el First Person Controller (su cámara ya está lista)
        playerController.SetActive(true);

        // 4) ¡Ahora sí! Desactivo la Main Camera vieja
        mainCamera.gameObject.SetActive(false);

        // 5) Hago el fade-out del panel negro, revelando tu First Person POV
        screenFader.fadeDuration = fadeDuration;
        yield return screenFader.FadeOut();

        if (audioMili1 != null)
            NarrationManager.Instance.PlayNarration(audioMili1);

        if (sonidoAmbiente != null)
            sonidoAmbiente.Play();
    }


}
