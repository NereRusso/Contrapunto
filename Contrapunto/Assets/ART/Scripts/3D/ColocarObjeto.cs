using UnityEngine;

public class ColocarObjeto : MonoBehaviour
{
    public string requiredObjectID; // El ID que se necesita para colocar
    public GameObject placedVisual; // El objeto final que aparece una vez colocado

    private void OnMouseDown()
    {
        var heldObject = HeldInventory.Instance.GetHeldObject(requiredObjectID);
        if (heldObject != null)
        {
            Debug.Log("Objeto correcto colocado en silueta!");

            HeldInventory.Instance.RemoveObject(heldObject);

            gameObject.SetActive(false);

            if (placedVisual != null)
            {
                placedVisual.SetActive(true);
            }

            SoundManager.Instance.PlaySuccessSound(); // <<< Sonido de éxito

            // <<< Contar objetos colocados (lo hacemos más abajo)

            ObjectPlacementTracker.Instance.ObjectPlaced(); // <<< Contador nuevo
        }
        else
        {
            SoundManager.Instance.PlayErrorSound(); // <<< Sonido de error
        }
    }

}
