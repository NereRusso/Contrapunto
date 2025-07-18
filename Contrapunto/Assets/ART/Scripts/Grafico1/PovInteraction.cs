using UnityEngine;

public class PovInteraction : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;
    public GameObject flecha;

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

    private void Update()
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
    }

    void OnMouseDown()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // oculto prompt propio (si lo tuvieras)
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        // deshabilito el script para que PromptClickDG ya no lo detecte
        this.enabled = false;

        // resto de tu lógica...
        if (clickSound != null) clickSound.Play();
        player.SetActive(false);
        visorController.SetActive(true);
        flecha.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

}