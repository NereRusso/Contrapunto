using UnityEngine;

public class Audio2Trigger : MonoBehaviour
{
    [Header("Clip que se va a reproducir")]
    public AudioClip audio2;

    bool played = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Audio2Trigger] Colisión con: {other.name}");
        if (played) return;
        if (!other.CompareTag("Player")) return;
        if (audio2 == null)
        {
            Debug.LogWarning("[Audio2Trigger] Falta asignar audio2");
            return;
        }
        if (NarrationManager.Instance == null)
        {
            Debug.LogError("[Audio2Trigger] NarrationManager.Instance es null");
            return;
        }

        NarrationManager.Instance.PlayNarration(audio2);
        played = true;

        // Destruye todos los triggers de este tipo en la escena
        Audio2Trigger[] allTriggers = FindObjectsOfType<Audio2Trigger>();
        foreach (Audio2Trigger trigger in allTriggers)
        {
            Destroy(trigger.gameObject);
        }
    }
}
