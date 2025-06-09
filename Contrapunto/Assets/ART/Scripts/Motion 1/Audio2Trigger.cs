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
            // Usamos el sistema de narraci�n
            NarrationManager.Instance.PlayNarration(audioMili2);

            alreadyTriggered = true;
            GetComponent<Collider>().enabled = false;

            // Destruimos este objeto despu�s de la duraci�n del audio
            Destroy(gameObject, audioMili2.length);
        }
    }
}
