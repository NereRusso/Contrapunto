using UnityEngine;

public class AmbientZone : MonoBehaviour
{
    public AudioClip ambientClip;
    [Range(0f, 1f)]
    public float targetVolume = 1f;

    private AudioClip previousClip;
    private float previousVolume;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Guardamos el clip anterior y su volumen para restaurarlo después
            previousClip = AmbientManager.Instance.GetCurrentClip();
            previousVolume = AmbientManager.Instance.GetCurrentTargetVolume();

            AmbientManager.Instance.ChangeAmbientSound(ambientClip, targetVolume);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Cuando salimos, restauramos el sonido anterior
            AmbientManager.Instance.ChangeAmbientSound(previousClip, previousVolume);
        }
    }
}
