using UnityEngine;

public class Audio2Trigger : MonoBehaviour
{
    [Header("Clip que se va a reproducir y guardar")]
    public AudioClip audio2;

    private static bool alreadyPlayed = false;

    void OnTriggerEnter(Collider other)
    {
        if (alreadyPlayed) return;

        if (other.CompareTag("Player") && audio2 != null)
        {
            // Usamos el sistema de narración
            NarrationManager.Instance.PlayNarration(audio2);

            alreadyPlayed = true;

            // Buscar todos los objetos con este script y destruirlos
            Audio2Trigger[] allTriggers = FindObjectsOfType<Audio2Trigger>();
            foreach (Audio2Trigger trigger in allTriggers)
            {
                // Podés usar DestroyImmediate si querés que se borren sin delay
                Destroy(trigger.gameObject);
            }
        }
    }
}
