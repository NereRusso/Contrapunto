using UnityEngine;

public class PovInteraction : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;

    [Header("Audio al hacer click")]
    public AudioSource clickSound;  // Asigná el AudioSource con el sonido en el Inspector

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    private Camera mainCamera;
    private bool hasBeenClicked = false;

    void Start()
    {
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // Ya fue clickeado, no mostramos más
        if (hasBeenClicked)
            return;

        // Si estamos cerca y mirando el collider, activa el prompt
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Ya fue clickeado, no hace falta ocultar (está siempre oculto)
        if (hasBeenClicked)
            return;

        // Al salir del collider, oculta el prompt
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        // Ocultamos el prompt al clickear
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // Reproducir sonido de click
        if (clickSound != null)
            clickSound.Play();
        else
            Debug.LogWarning("No se asignó ningún AudioSource para el sonido de click.");

        // Activo el modo visor
        player.SetActive(false);
        visorController.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
    }
}
