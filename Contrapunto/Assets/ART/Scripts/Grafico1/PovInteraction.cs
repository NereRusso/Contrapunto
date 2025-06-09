using UnityEngine;

public class PovInteraction : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;

    private bool hasBeenClicked = false;

    void OnMouseDown()
    {
        if (hasBeenClicked) return;   // si ya lo clickeaste, no hace nada
        hasBeenClicked = true;

        // Activo el modo visor
        player.SetActive(false);
        visorController.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Opcional: desactivar este script para no procesar nada más
        this.enabled = false;
    }
}
