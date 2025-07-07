using UnityEngine;

public class AgarrarObjeto : MonoBehaviour
{
    public string objectID;
    public GameObject silhouetteToActivate;

    [Header("Ajustes visuales para el inventario")]
    public Vector3 inventoryRotation = Vector3.zero;
    public float inventoryScale = 0.3f;

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    private Camera mainCamera;

    private void Start()
    {
        // Cacheamos la cámara principal y desactivamos el prompt al inicio
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        // Cuando el cursor entra en el collider, chequeamos distancia y mostramos el prompt
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        // Al salir del collider, ocultamos el prompt
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseDown()
    {
        // Ocultamos el prompt al pickup
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        // Tu lógica original de inventario
        if (HeldInventory.Instance != null && HeldInventory.Instance.AddObject(this))
        {
            if (silhouetteToActivate != null)
                silhouetteToActivate.SetActive(true);

            SoundManager.Instance.PlayPickupSound();
            Destroy(gameObject);
        }
    }
}
