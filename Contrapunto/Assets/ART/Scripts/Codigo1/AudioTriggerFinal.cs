using UnityEngine;
using System.Collections;

public class AudioTriggerFinal : MonoBehaviour
{
    [Header("Audio objetivo")]
    public AudioSource targetSource;

    [Header("Nuevo clip")]
    public AudioClip finalClip;

    [Header("Duración del crossfade")]
    public float fadeDuration = 1.5f;

    public void CambiarYReproducir()
    {
        if (targetSource != null && finalClip != null)
        {
            StartCoroutine(CrossfadeToClip());
        }
    }

    private IEnumerator CrossfadeToClip()
    {
        float originalVolume = targetSource.volume;
        float t = 0f;

        // Fade out actual
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            targetSource.volume = Mathf.Lerp(originalVolume, 0f, t / fadeDuration);
            yield return null;
        }

        targetSource.Stop();
        targetSource.clip = finalClip;
        targetSource.Play();

        // Fade in nuevo
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            targetSource.volume = Mathf.Lerp(0f, originalVolume, t / fadeDuration);
            yield return null;
        }

        targetSource.volume = originalVolume;
    }
}
