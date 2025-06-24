using UnityEngine;
using System.Collections;

public class ActivarDance : MonoBehaviour
{
    public ManagerDance videoManager;
    public Camera playerCamera;
    public float maxDistance = 3f;

    [Header("Audio")]
    public AudioSource audioSource; // Asigná el AudioSource desde el inspector
    public float fadeDuration = 1f;

    private bool activado = false;

    void OnMouseDown()
    {
        if (activado) return;

        float distancia = Vector3.Distance(transform.position, playerCamera.transform.position);

        if (distancia <= maxDistance)
        {
            activado = true;
            StartCoroutine(FadeOutAudio());
            videoManager.ActivarDDR();
        }
        else
        {
            Debug.Log("Estás demasiado lejos para activar el cubo.");
        }
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop(); // Opcional, si querés detener la reproducción
    }
}
