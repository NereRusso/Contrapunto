using UnityEngine;

public class AudioTriggerTodos : MonoBehaviour
{
    [Header("Clip que se va a reproducir")]
    public AudioClip audio3Reni;

    bool played = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Audio2Trigger] Colisión con: {other.name}");
        if (played) return;
        if (!other.CompareTag("Player")) return;
        if (audio3Reni == null)
        {
            Debug.LogWarning("[Audio2Trigger] Falta asignar audio2");
            return;
        }
        if (NarrationManager.Instance == null)
        {
            Debug.LogError("[Audio2Trigger] NarrationManager.Instance es null");
            return;
        }

        NarrationManager.Instance.PlayNarration(audio3Reni);
        played = true;

        // Destruye todos los triggers de este tipo en la escena
        AudioTriggerTodos[] allTriggers = FindObjectsOfType<AudioTriggerTodos>();
        foreach (AudioTriggerTodos trigger in allTriggers)
        {
            Destroy(trigger.gameObject);
        }
    }
}
