using UnityEngine;

public class PovInteractionTwo : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;
    public GameObject flecha;

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

    /*private void Update()
    {
        if (mainCamera == null || clickCanvas == null)
            return;

        float d = Vector3.Distance(mainCamera.transform.position, transform.position);
        if (d <= pickupRange)
        {
            Ray ray = new Ray(mainCamera.transform.position, mainCamera.transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, pickupRange) && hit.transform == transform)
                clickCanvas.SetActive(true);
            else
                clickCanvas.SetActive(false);
        }
        else
        {
            clickCanvas.SetActive(false);
        }
    }*/

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
        flecha.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
    }
}
