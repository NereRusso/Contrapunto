using UnityEngine;

public class Audio2TriggerGarf : MonoBehaviour
{
    [Header("Clip que se va a reproducir y guardar")]
    public AudioClip audioMarti2;

    private bool alreadyTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;

        if (other.CompareTag("Player") && audioMarti2 != null)
        {
            // Usamos el sistema de narración
            NarrationManager.Instance.PlayNarration(audioMarti2);

            alreadyTriggered = true;
            GetComponent<Collider>().enabled = false;

            // Destruimos este objeto después de la duración del audio
            Destroy(gameObject, audioMarti2.length);
        }
    }
}
