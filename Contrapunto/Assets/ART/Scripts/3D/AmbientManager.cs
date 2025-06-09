using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class AmbientManager : MonoBehaviour
{
    public static AmbientManager Instance;

    public AudioSource ambientSourceA;
    public AudioSource ambientSourceB;
    public float fadeDuration = 1.5f;
    [Range(0f, 1f)]
    public float initialTargetVolume = 0.5f;

    private AudioSource currentSource;
    private AudioSource nextSource;
    private float currentTargetVolume = 1f;
    private float nextTargetVolume = 1f;

    // >>> Mapa para guardar en qué momento está cada AudioClip
    private Dictionary<AudioClip, int> clipPositions = new Dictionary<AudioClip, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        currentSource = ambientSourceA;
        nextSource = ambientSourceB;

        if (currentSource != null && currentSource.clip != null)
        {
            currentSource.volume = 0f;
            currentTargetVolume = initialTargetVolume;
            currentSource.Play();
            StartCoroutine(FadeInFirstAmbient());
        }
    }

    IEnumerator FadeInFirstAmbient()
    {
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            currentSource.volume = Mathf.Lerp(0f, currentTargetVolume, timer / fadeDuration);
            yield return null;
        }
        currentSource.volume = currentTargetVolume;
    }

    public void ChangeAmbientSound(AudioClip newClip, float newVolume)
    {
        if (currentSource.clip == newClip && Mathf.Approximately(currentTargetVolume, newVolume))
            return;

        // Guardamos la posición del clip actual
        if (currentSource.clip != null)
        {
            clipPositions[currentSource.clip] = currentSource.timeSamples;
        }

        nextSource.clip = newClip;
        nextSource.volume = 0f;

        // Si ya lo habíamos reproducido antes, restauramos su tiempo
        if (clipPositions.ContainsKey(newClip))
        {
            nextSource.timeSamples = clipPositions[newClip];
        }

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

        // Swap
        var temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }

    public AudioClip GetCurrentClip()
    {
        return currentSource.clip;
    }

    public float GetCurrentTargetVolume()
    {
        return currentTargetVolume;
    }

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
