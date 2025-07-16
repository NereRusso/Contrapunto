using UnityEngine;

public class AgarrarObjeto : MonoBehaviour
{
    [Header("Identificador")]
    public string objectID;

    [Header("Silhouette")]
    public GameObject silhouetteToActivate;

    [Header("Ajustes visuales para el inventario")]
    public Vector3 inventoryRotation = Vector3.zero;
    public float inventoryScale = 0.3f;

    [Header("Narración")]
    public AudioClip audio2Reni;

    [Header("Objetos a activar al primer pickup")]
    public GameObject[] objetosAActivar;

    [Header("Prompt de click")]
    [Tooltip("Arrastrá acá tu Canvas (o GameObject) con el texto “click”")]
    public GameObject clickCanvas;
    [Tooltip("Distancia máxima para que aparezca el prompt")]
    public float pickupRange = 3f;

    private Camera mainCamera;
    private static bool primerObjetoAgarrado = false;

    private void Start()
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
        // Ocultamos el prompt
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        // Solo intentamos recoger si estamos dentro del rango
        if (mainCamera == null ||
            Vector3.Distance(mainCamera.transform.position, transform.position) > pickupRange)
            return;

        // Intentamos añadir al inventario
        if (HeldInventory.Instance != null && HeldInventory.Instance.AddObject(this))
        {
            // Activar la silueta en el UI
            if (silhouetteToActivate != null)
                silhouetteToActivate.SetActive(true);

            // Sonido de pickup
            SoundManager.Instance.PlayPickupSound();

            // Lógica solo para el primer objeto Agarrado
            if (!primerObjetoAgarrado)
            {
                primerObjetoAgarrado = true;

                // Reproducir narración
                if (audio2Reni != null && NarrationManager.Instance != null)
                    NarrationManager.Instance.PlayNarration(audio2Reni);

                // Activar objetos configurados
                foreach (var obj in objetosAActivar)
                    if (obj != null)
                        obj.SetActive(true);
            }

            // Destruir el objeto en escena
            Destroy(gameObject);
        }
    }
}
