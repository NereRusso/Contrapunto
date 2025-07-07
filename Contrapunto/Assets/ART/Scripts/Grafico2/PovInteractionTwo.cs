using UnityEngine;

public class PovInteractionTwo : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;

    [Header("Sonido al hacer click")]
    public AudioSource audioSource;
    public AudioClip clickSound;

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
        // Si ya fue clickeado, no mostramos más el prompt
        if (hasBeenClicked) return;

        // Mostrar prompt solo si estamos dentro de pickupRange
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Si ya fue clickeado, no ocultamos (ya está oculto)
        if (hasBeenClicked) return;

        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void OnMouseDown()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // Ocultar prompt al clicar
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        // Reproducir sonido de click
        if (audioSource != null && clickSound != null)
            audioSource.PlayOneShot(clickSound);

        // Activo el modo visor
        player.SetActive(false);
        visorController.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
    }
}
