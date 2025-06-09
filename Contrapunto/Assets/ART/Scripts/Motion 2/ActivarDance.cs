using UnityEngine;

public class ActivarDance : MonoBehaviour
{
    public ManagerDance videoManager;
    public Camera playerCamera;
    public float maxDistance = 3f;

    private bool activado = false;

    void OnMouseDown()
    {
        if (activado) return; // Ya se activó, no seguir

        float distancia = Vector3.Distance(transform.position, playerCamera.transform.position);

        if (distancia <= maxDistance)
        {
            activado = true;
            videoManager.ActivarDDR();
        }
        else
        {
            Debug.Log("Estás demasiado lejos para activar el cubo.");
        }
    }
}
