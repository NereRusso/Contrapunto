using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
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

    [Header("Videos que se deben reactivar al volver al mapa")]
    public List<VideoPlayer> videosAReactivar;

    [Header("Referencias externas")]
    public MotionDosManager motionDosManager;

    void Awake()
    {
        instance = this;
    }

    // Se llama cuando la barra se llena
    public void FinDelJuego()
    {
        // Detener spawner e input
        spawner.DetenerSpawner();
        inputManager.enabled = false;

        // Ocultar mouse
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // Mostrar canvas de felicitaciones y video
        canvasFelicitaciones.SetActive(true);
        rawImageVideo.texture = videoFelicitaciones.targetTexture;
        videoFelicitaciones.gameObject.SetActive(true);
        rawImageVideo.gameObject.SetActive(true);

        videoFelicitaciones.Play();
        videoFelicitaciones.loopPointReached += VolverAlMapa;
    }

    // Se llama cuando termina el video de felicitaciones
    void VolverAlMapa(VideoPlayer vp)
    {
        // Ocultar canvas felicitaciones
        canvasFelicitaciones.SetActive(false);

        // Ocultar canvas DDR
        canvasDDR.SetActive(false);

        // Reactivar jugador
        player.SetActive(true);

        // Habilitar el objeto especial
        objetoParaHabilitar.SetActive(true);

        if (audio2Mili != null)
        {
            NarrationManager.Instance.PlayNarration(audio2Mili);
        }

        // ?? Restaurar videos glitch y pausados desde MotionDosManager
        if (motionDosManager != null)
        {
            motionDosManager.RestaurarTodosVideos();
        }

        // ? Si tenés videos extra definidos en esta escena
        foreach (var video in videosAReactivar)
        {
            if (video != null)
                video.Play();
        }
    }
}
