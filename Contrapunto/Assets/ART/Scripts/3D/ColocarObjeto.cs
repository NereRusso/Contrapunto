using UnityEngine;

public class ColocarObjeto : MonoBehaviour
{
    public string requiredObjectID;
    public GameObject placedVisual;
    public AmbientZone ambientZone;

    [Tooltip("Arrastr� ac� tu script ControladorBienYMal")]
    public ControladorBienYMal controlador;

    [Header("Prompt de click")]
    public GameObject clickCanvas;
    public float pickupRange = 3f;

    [Header("Narraciones por paso")]
    public AudioClip audio4Reni;
    public AudioClip audio12Reni;
    public AudioClip audio22Reni;

    private static int objetosColocados = 0; // Lleva la cuenta total de colocados v�lidos
    private Camera mainCamera;

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

            // ? Reproducir narraci�n por orden de colocaci�n
            if (NarrationManager.Instance != null)
            {
                if (objetosColocados == 0 && audio4Reni != null)
                    NarrationManager.Instance.PlayNarration(audio4Reni);
                else if (objetosColocados == 1 && audio12Reni != null)
                    NarrationManager.Instance.PlayNarration(audio12Reni);
                else if (objetosColocados == 2 && audio22Reni != null)
                    NarrationManager.Instance.PlayNarration(audio22Reni);
            }

            objetosColocados++; // Incrementamos despu�s
        }
        else
        {
            SoundManager.Instance.PlayErrorWithNarration();
        }
    }
}
