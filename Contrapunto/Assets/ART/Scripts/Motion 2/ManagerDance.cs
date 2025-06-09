using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ManagerDance : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject canvasDDR;
    public GameObject player;
    public RawImage rawImage;

    [Header("Videos")]
    public VideoPlayer videoIntro;
    public VideoPlayer videoLoop;
    public VideoPlayer videoTitulo;
    public VideoPlayer videoCarga;
    public VideoPlayer videoPersonaje;
    public VideoPlayer videoFondo;

    [Header("UI")]
    public GameObject botonSincronizar;
    public GameObject pantallaCanciones;

    private bool canPressEnter = false;

    void Start()
    {
        canvasDDR.SetActive(false);

        videoIntro.gameObject.SetActive(false);
        videoLoop.gameObject.SetActive(false);
        videoTitulo.gameObject.SetActive(false);
        videoCarga.gameObject.SetActive(false);
        videoPersonaje.gameObject.SetActive(false);
        videoFondo.gameObject.SetActive(false);

        botonSincronizar.SetActive(false);
        pantallaCanciones.SetActive(false);
    }

    public void ActivarDDR()
    {
        player.SetActive(false);
        canvasDDR.SetActive(true);

        videoIntro.gameObject.SetActive(true);
        rawImage.texture = videoIntro.targetTexture;
        videoIntro.Play();
        videoIntro.loopPointReached += OnIntroFinished;
    }

    void OnIntroFinished(VideoPlayer vp)
    {
        videoLoop.gameObject.SetActive(true);
        rawImage.texture = videoLoop.targetTexture;
        videoLoop.isLooping = true;
        videoLoop.Play();

        videoIntro.Stop();
        videoIntro.gameObject.SetActive(false);

        canPressEnter = true;
    }

    void Update()
    {
        if (canPressEnter && Input.GetKeyDown(KeyCode.Return))
        {
            canPressEnter = false;

            videoTitulo.gameObject.SetActive(true);
            rawImage.texture = videoTitulo.targetTexture;
            videoTitulo.isLooping = true;
            videoTitulo.Play();

            videoLoop.Stop();
            videoLoop.gameObject.SetActive(false);

            botonSincronizar.SetActive(true);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void OnClickSincronizar()
    {
        videoCarga.gameObject.SetActive(true);
        rawImage.texture = videoCarga.targetTexture;
        videoCarga.Play();

        videoTitulo.Stop();
        videoTitulo.gameObject.SetActive(false);

        botonSincronizar.SetActive(false);
        videoCarga.loopPointReached += OnFinal1Finished;
    }

    void OnFinal1Finished(VideoPlayer vp)
    {
        videoPersonaje.gameObject.SetActive(true);
        rawImage.texture = videoPersonaje.targetTexture;
        videoPersonaje.Play();

        videoCarga.Stop();
        videoCarga.gameObject.SetActive(false);

        videoPersonaje.loopPointReached += OnFinal2Finished;
    }

    void OnFinal2Finished(VideoPlayer vp)
    {
        videoFondo.gameObject.SetActive(true);
        rawImage.texture = videoFondo.targetTexture;
        videoFondo.isLooping = true;
        videoFondo.Play();

        videoPersonaje.Stop();
        videoPersonaje.gameObject.SetActive(false);

        pantallaCanciones.SetActive(true);
    }
}
