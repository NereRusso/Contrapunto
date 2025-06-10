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

    [Header("Videos que deben reanudarse al finalizar")]
    public List<VideoPlayer> videosAReactivar = new List<VideoPlayer>();

    [Header("FadeScreen para detener glitches")]
    public FadeScreen fadeScreen; // Referencia al script que maneja el glitch

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
                videoPlayer.gameObject.SetActive(true);
                videoPlayer.Play();

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

        fpsController.enabled = true;
        starterInputs.enabled = true;
        piSystem.enabled = true;

        puedeInteractuar = false;

        if (logoTp != null)
            logoTp.SetActive(true);

        if (canvasCilindro != null)
            canvasCilindro.gameObject.SetActive(false);

        // ? Reactivar videos pausados o glitch
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
}
