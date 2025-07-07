using UnityEngine;

public class ColocarObjeto : MonoBehaviour
{
    public string requiredObjectID;
    public GameObject placedVisual;
    public AmbientZone ambientZone;

    [Tooltip("Arrastrá acá tu script ControladorBienYMal")]
    public ControladorBienYMal controlador; // NUEVO: arrastrá el otro script en el Inspector

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // Al pasar el cursor (o crosshair) sobre el collider y si estamos cerca, mostramos el texto
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Al salir del collider, ocultamos el texto
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseDown()
    {
        // Ocultamos el prompt al hacer click
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        // Lógica original de colocación
        var heldObject = HeldInventory.Instance.GetHeldObject(requiredObjectID);
        if (heldObject != null)
        {
            HeldInventory.Instance.RemoveObject(heldObject);
            gameObject.SetActive(false);

            if (placedVisual != null)
                placedVisual.SetActive(true);

            SoundManager.Instance.PlaySuccessSound();
            ObjectPlacementTracker.Instance.ObjectPlaced();

            if (ambientZone != null)
                ambientZone.ActivateGoodAmbient();

            if (controlador != null)
                controlador.IniciarSecuencia();
        }
        else
        {
            SoundManager.Instance.PlayErrorSound();
        }
    }
}
