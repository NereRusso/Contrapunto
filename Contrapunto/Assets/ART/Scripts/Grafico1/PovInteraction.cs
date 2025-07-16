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

    void Update()
    {
        // Si ya clickeamos o falta config, no hacemos nada
        if (hasBeenClicked || mainCamera == null || clickCanvas == null)
            return;

        /* float d = Vector3.Distance(mainCamera.transform.position, transform.position);
        if (d <= pickupRange)
        {
            // Raycast desde la cámara hacia adelante
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange) && hit.transform == transform)
                clickCanvas.SetActive(true);
            else
                clickCanvas.SetActive(false);
        }
        else
        {
            clickCanvas.SetActive(false);
        }*/
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
