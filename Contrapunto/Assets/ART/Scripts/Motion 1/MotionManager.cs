// MotionManager.cs
using UnityEngine;

public class MotionManager : MonoBehaviour
{
    public static MotionManager Instance;

    [Header("Configuración")]
    public int totalCubes = 5;

    [Header("Sonidos")]
    public AudioClip pickupSound;              // Sonido al recoger
    public AudioClip[] narracionesPorOrden;    // 4 audios, en el orden deseado

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
        UIMoManager.Instance.UpdateCubesRemaining(totalCubes - collected);
        PlayPickupSound();

        // Reproducir narración según el orden de recogida
        if (narracionesPorOrden != null && collected - 1 < narracionesPorOrden.Length)
        {
            AudioClip narracion = narracionesPorOrden[collected - 1];
            if (narracion != null && !NarrationManager.Instance.IsNarrationPlaying())
            {
                // Sólo la 2ª y la 4ª van a lo lejos
                bool isDistant = (collected == 2 || collected == 4);
                NarrationManager.Instance.PlayNarration(narracion, isDistant);
            }
        }
    }

    void PlayPickupSound()
    {
        if (pickupSound != null && audioSource != null)
            audioSource.PlayOneShot(pickupSound);
    }

    public int GetRemainingCubes() => totalCubes - collected;
    public bool AllCubesCollected() => collected >= totalCubes;
}
