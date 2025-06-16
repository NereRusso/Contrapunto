using UnityEngine;

public class Audio2Trigger : MonoBehaviour
{
    [Header("Clip que se va a reproducir y guardar")]
    public AudioClip audioMili2;

    private bool alreadyTriggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyTriggered) return;

        if (other.CompareTag("Player") && audioMili2 != null)
        {
            // Usamos el sistema de narración
            NarrationManager.Instance.PlayNarration(audioMili2);

            alreadyTriggered = true;
            GetComponent<Collider>().enabled = false;

            // Destruimos este objeto después de la duración del audio
            Destroy(gameObject, audioMili2.length);
        }
    }
}
