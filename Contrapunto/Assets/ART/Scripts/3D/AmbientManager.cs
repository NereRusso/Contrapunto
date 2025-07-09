using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager Instance;

    public AudioSource ambientSourceA;
    public AudioSource ambientSourceB;
    public float fadeDuration = 1.5f;
    [Range(0f, 1f)] public float initialTargetVolume = 0.5f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private float currentTargetVolume = 1f;
    private float nextTargetVolume = 1f;

    private Dictionary<AudioClip, int> clipPositions = new Dictionary<AudioClip, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        currentSource = ambientSourceA;
        nextSource = ambientSourceB;

        // ? Comentamos esto para que no suene solo al inicio
        /*
        if (currentSource != null && currentSource.clip != null)
        {
            currentSource.volume = 0f;
            currentTargetVolume = initialTargetVolume;
            currentSource.Play();
            StartCoroutine(FadeInFirstAmbient());
        }
        */
    }

    public void FadeInGlobalAmbient()
    {
        if (currentSource != null && currentSource.clip != null)
        {
            currentSource.UnPause(); // si venía en pausa
            StartCoroutine(FadeIn(currentSource, initialTargetVolume));
        }
    }

    private IEnumerator FadeIn(AudioSource source, float targetVolume)
    {
        float timer = 0f;
        source.volume = 0f;

        if (!source.isPlaying) source.Play();

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, timer / fadeDuration);
            yield return null;
        }

        source.volume = targetVolume;
    }

    public void ChangeAmbientSound(AudioClip newClip, float newVolume)
    {
        if (currentSource.clip == newClip && Mathf.Approximately(currentTargetVolume, newVolume))
            return;

        if (currentSource.clip != null)
            clipPositions[currentSource.clip] = currentSource.timeSamples;

        nextSource.clip = newClip;
        nextSource.volume = 0f;

        if (clipPositions.ContainsKey(newClip))
            nextSource.timeSamples = clipPositions[newClip];

        nextSource.Play();
        nextTargetVolume = newVolume;

        StartCoroutine(CrossfadeAmbients());
    }

    private IEnumerator CrossfadeAmbients()
    {
        float timer = 0f;
        float startVolumeCurrent = currentSource.volume;
        float startVolumeNext = nextSource.volume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            currentSource.volume = Mathf.Lerp(startVolumeCurrent, 0f, timer / fadeDuration);
            nextSource.volume = Mathf.Lerp(startVolumeNext, nextTargetVolume, timer / fadeDuration);
            yield return null;
        }

        currentSource.Stop();
        nextSource.volume = nextTargetVolume;

        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }

    public void ReplaceCurrentZoneAmbient(AudioClip newClip, float newVolume)
    {
        StartCoroutine(FadeToZoneGoodAmbient(newClip, newVolume));
    }

    private IEnumerator FadeToZoneGoodAmbient(AudioClip newClip, float targetVolume)
    {
        if (ambientSourceB.clip == newClip)
            yield break;

        float duration = fadeDuration;
        float startVolume = ambientSourceB.volume;

        if (ambientSourceB.clip != null)
            clipPositions[ambientSourceB.clip] = ambientSourceB.timeSamples;

        ambientSourceB.clip = newClip;

        if (clipPositions.ContainsKey(newClip))
            ambientSourceB.timeSamples = clipPositions[newClip];

        ambientSourceB.Play();

        float time = 0f;
        while (time < duration)
        {
            time += Time.deltaTime;
            ambientSourceB.volume = Mathf.Lerp(startVolume, targetVolume, time / duration);
            yield return null;
        }

        ambientSourceB.volume = targetVolume;
    }

    public void FadeOutGlobalAmbient()
    {
        if (ambientSourceA != null && ambientSourceA.isPlaying)
            StartCoroutine(FadeOut(ambientSourceA));
    }

    private IEnumerator FadeOut(AudioSource source)
    {
        float startVolume = source.volume;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
            yield return null;
        }

        source.Pause();
        source.volume = 0f;
    }

    public AudioClip GetCurrentClip() => currentSource.clip;
    public float GetCurrentTargetVolume() => currentTargetVolume;

    public void StopAllAmbients()
    {
        if (ambientSourceA != null)
        {
            ambientSourceA.Stop();
            ambientSourceA.volume = 0f;
        }

        if (ambientSourceB != null)
        {
            ambientSourceB.Stop();
            ambientSourceB.volume = 0f;
        }

        currentTargetVolume = 0f;
        nextTargetVolume = 0f;
    }
}
