using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AmbientZone : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioBad;
    public AudioSource audioGood;

    [Range(0f, 1f)] public float targetVolume = 1f;
    public float fadeDuration = 2f;

    private bool goodSoundActivated = false;
    private bool isPlayerInside = false;

    // Guardar la posición actual de los clips
    private Dictionary<AudioSource, int> savedTimeSamples = new Dictionary<AudioSource, int>();

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (isPlayerInside) return;

        isPlayerInside = true;

        AmbientManager.Instance.FadeOutGlobalAmbient();

        AudioSource sourceToPlay = goodSoundActivated ? audioGood : audioBad;
        if (sourceToPlay != null)
        {
            ResumeAudio(sourceToPlay);
            StartCoroutine(FadeIn(sourceToPlay, targetVolume));
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (!isPlayerInside) return;

        isPlayerInside = false;

        AmbientManager.Instance.FadeInGlobalAmbient();

        AudioSource sourceToStop = goodSoundActivated ? audioGood : audioBad;
        if (sourceToStop != null)
        {
            StartCoroutine(FadeOutAndSave(sourceToStop));
        }
    }

    public void ActivateGoodAmbient()
    {
        if (goodSoundActivated || audioGood == null || audioBad == null) return;

        goodSoundActivated = true;
        StartCoroutine(CrossfadeToGoodAmbient());
    }

    private IEnumerator CrossfadeToGoodAmbient()
    {
        // Guardar posición del audioBad
        if (audioBad.isPlaying)
            savedTimeSamples[audioBad] = audioBad.timeSamples;

        // Restaurar posición del audioGood si ya sonó
        if (savedTimeSamples.ContainsKey(audioGood))
            audioGood.timeSamples = savedTimeSamples[audioGood];

        audioGood.Play();
        audioGood.volume = 0f;

        float timer = 0f;
        float startVolume = audioBad.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;

            audioBad.volume = Mathf.Lerp(startVolume, 0f, t);
            audioGood.volume = Mathf.Lerp(0f, targetVolume, t);

            yield return null;
        }

        audioBad.Stop();
        savedTimeSamples[audioBad] = audioBad.timeSamples;
        audioGood.volume = targetVolume;
    }

    private IEnumerator FadeOutAndSave(AudioSource source)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        savedTimeSamples[source] = source.timeSamples;
        source.Pause();
        source.volume = 0f;
    }

    private IEnumerator FadeIn(AudioSource source, float targetVol)
    {
        source.volume = 0f;

        // Restaurar posición si la guardamos
        if (savedTimeSamples.ContainsKey(source))
            source.timeSamples = savedTimeSamples[source];

        source.Play();

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVol, timer / fadeDuration);
            yield return null;
        }

        source.volume = targetVol;
    }

    private void ResumeAudio(AudioSource source)
    {
        if (savedTimeSamples.ContainsKey(source))
            source.timeSamples = savedTimeSamples[source];
    }
}
