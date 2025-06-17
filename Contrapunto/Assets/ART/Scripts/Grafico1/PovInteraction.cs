using UnityEngine;

public class PovInteraction : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;

    [Header("Audio al hacer click")]
    public AudioSource clickSound;  // Asigná el AudioSource con el sonido en el Inspector

    private bool hasBeenClicked = false;

    void OnMouseDown()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // Reproducir sonido de click
        if (clickSound != null)
        {
            clickSound.Play();
        }
        else
        {
            Debug.LogWarning("No se asignó ningún AudioSource para el sonido de click.");
        }

        // Activo el modo visor
        player.SetActive(false);
        visorController.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
    }
}
