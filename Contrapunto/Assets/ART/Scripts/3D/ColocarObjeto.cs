using UnityEngine;

public class ColocarObjeto : MonoBehaviour
{
    public string requiredObjectID;
    public GameObject placedVisual;
    public AmbientZone ambientZone;

    [Tooltip("Arrastrá acá tu script ControladorBienYMal")]
    public ControladorBienYMal controlador;

    [Header("Prompt de click")]
    public GameObject clickCanvas;
    public float pickupRange = 3f;

    [Header("Narraciones por paso")]
    public AudioClip audio12Reni;
    public AudioClip audio22Reni;
    public AudioClip audio32Reni;

    private static int objetosColocados = 0; // Lleva la cuenta total de colocados válidos
    private Camera mainCamera;

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

    private void OnMouseDown()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

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

            // Reproducir narración por orden de colocación
            if (NarrationManager.Instance != null)
            {
                if (objetosColocados == 1 && audio12Reni != null)
                    NarrationManager.Instance.PlayNarration(audio12Reni);
                else if (objetosColocados == 2 && audio22Reni != null) { 
                    NarrationManager.Instance.PlayNarration(audio22Reni);
                    NarrationManager.Instance.PlayNarration(audio32Reni);}
                    
            }

            objetosColocados++; // Incrementamos después
        }
        else
        {
            SoundManager.Instance.PlayErrorWithNarration();
        }
    }
}
