using UnityEngine;

public class AgarrarObjeto : MonoBehaviour
{
    public string objectID;
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

    // ? Bandera estática para detectar si ya se agarró el primero
    private static bool primerObjetoAgarrado = false;

    private void Start()
    {
        mainCamera = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseEnter()
    {
        if (mainCamera != null && clickCanvas != null &&
            Vector3.Distance(mainCamera.transform.position, transform.position) <= pickupRange)
        {
            clickCanvas.SetActive(true);
        }
    }

    private void OnMouseExit()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (HeldInventory.Instance != null && HeldInventory.Instance.AddObject(this))
        {
            if (silhouetteToActivate != null)
                silhouetteToActivate.SetActive(true);

            SoundManager.Instance.PlayPickupSound();

            // ? Si es el primer objeto que se agarra
            if (!primerObjetoAgarrado)
            {
                primerObjetoAgarrado = true;

                // Reproducir narración
                if (audio2Reni != null && NarrationManager.Instance != null)
                    NarrationManager.Instance.PlayNarration(audio2Reni);

                // Activar los objetos
                foreach (var obj in objetosAActivar)
                {
                    if (obj != null)
                        obj.SetActive(true);
                }
            }

            Destroy(gameObject);
        }
    }
}
