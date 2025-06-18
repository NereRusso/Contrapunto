using UnityEngine;

public class PovInteractionTwo : MonoBehaviour
{
    public Camera visorCamera;
    public GameObject player;
    public GameObject visorController;

    [Header("Sonido al hacer click")]
    public AudioSource audioSource;
    public AudioClip clickSound;

    private bool hasBeenClicked = false;

    void OnMouseDown()
    {
        if (hasBeenClicked) return;
        hasBeenClicked = true;

        // ?? Reproducir sonido de click
        if (audioSource && clickSound)
            audioSource.PlayOneShot(clickSound);

        // Activo el modo visor
        player.SetActive(false);
        visorController.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        this.enabled = false;
    }
}
