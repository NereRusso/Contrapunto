using UnityEngine;

public class ColocarObjeto : MonoBehaviour
{
    public string requiredObjectID;
    public GameObject placedVisual;
    public AmbientZone ambientZone;

    public ControladorBienYMal controlador; // NUEVO: arrastrá el otro script en el Inspector

    private void OnMouseDown()
    {
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
                controlador.IniciarSecuencia(); // <- ACÁ se delega todo lo demás
        }
        else
        {
            SoundManager.Instance.PlayErrorSound();
        }
    }
}
