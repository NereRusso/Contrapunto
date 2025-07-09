using UnityEngine;

public class DesactivarTrigger : MonoBehaviour
{
    [Tooltip("Tag del objeto que debe activar el trigger (ej: Player)")]
    public string tagActivador = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagActivador))
        {
            gameObject.SetActive(false);
        }
    }
}
