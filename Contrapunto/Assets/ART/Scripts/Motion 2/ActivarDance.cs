using UnityEngine;
using System.Collections;

public class ActivarDance : MonoBehaviour
{
    public ManagerDance videoManager;
    public Camera playerCamera;
    public float maxDistance = 3f;

    [Header("Audio")]
    public AudioSource audioSource; // Asign� el AudioSource desde el inspector
    public float fadeDuration = 1f;

    // Flag est�tico: compartido por todas las instancias
    private static bool alreadyActivated = false;

    void OnMouseDown()
    {
        // Si ya se activ� uno, salimos directamente
        if (alreadyActivated) return;

        float distancia = Vector3.Distance(transform.position, playerCamera.transform.position);
        if (distancia <= maxDistance)
        {
            alreadyActivated = true;              // Marcamos la activaci�n global
            StartCoroutine(FadeOutAudio());
            videoManager.ActivarDDR();
        }
        else
        {
            Debug.Log("Est�s demasiado lejos para activar el cubo.");
        }
    }

    private IEnumerator FadeOutAudio()
    {
        float startVolume = audioSource.volume;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop(); // Opcional, si quer�s detener la reproducci�n
    }
}
