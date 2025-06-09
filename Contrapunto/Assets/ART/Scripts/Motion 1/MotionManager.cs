using UnityEngine;

public class MotionManager : MonoBehaviour
{
    public static MotionManager Instance;

    [Header("Configuración")]
    public int totalCubes = 5;

    [Header("Sonidos")]
    public AudioClip pickupSound;       // Sonido normal de recoger cubo
    public AudioClip audioMili3;     // Tu narración especial
    private AudioSource audioSource;

    private int collected = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        audioSource = GetComponent<AudioSource>();
    }

    public void CollectCube()
    {
        collected++;

        // Actualizar el texto del contador
        UIMoManager.Instance.UpdateCubesRemaining(totalCubes - collected);

        // Sonar siempre el sonido de pickup
        PlayPickupSound();

        // Si este fue el último cubo, además reproducir tu narración
        if (collected == totalCubes)
        {
            PlayNarration();
        }
    }

    void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(pickupSound);
        }
    }

    void PlayNarration()
    {
        if (audioMili3 != null && audioSource != null)
        {
            NarrationManager.Instance.PlayNarration(audioMili3);
        }
    }

    public int GetRemainingCubes()
    {
        return totalCubes - collected;
    }

    public bool AllCubesCollected()
    {
        return collected >= totalCubes;
    }
}