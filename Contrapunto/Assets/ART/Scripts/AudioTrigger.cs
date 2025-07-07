using UnityEngine;

public class AudioTrigger : MonoBehaviour
{
    [Header("Clip que se va a reproducir")]
    public AudioClip audioTrigger;

    bool played = false;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Audio2Trigger] Colisión con: {other.name}");
        if (played) return;
        if (!other.CompareTag("Player")) return;
        if (audioTrigger == null)
        {
            Debug.LogWarning("[Audio2Trigger] Falta asignar audio2");
            return;
        }
        if (NarrationManager.Instance == null)
        {
            Debug.LogError("[Audio2Trigger] NarrationManager.Instance es null");
            return;
        }

        NarrationManager.Instance.PlayNarration(audioTrigger);
        played = true;

        // Solo se destruye este trigger, no todos
        Destroy(gameObject);
    }
}
