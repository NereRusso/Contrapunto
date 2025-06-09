using UnityEngine;

public class AgarrarObjeto : MonoBehaviour
{
    public string objectID;
    public GameObject silhouetteToActivate;

    [Header("Ajustes visuales para el inventario")]
    public Vector3 inventoryRotation = Vector3.zero;
    public float inventoryScale = 0.3f;

    private void OnMouseDown()
    {
        if (HeldInventory.Instance != null)
        {
            if (HeldInventory.Instance.AddObject(this))
            {
                if (silhouetteToActivate != null)
                {
                    silhouetteToActivate.SetActive(true);
                }

                SoundManager.Instance.PlayPickupSound();
                Destroy(gameObject);
            }
        }
    }
}
