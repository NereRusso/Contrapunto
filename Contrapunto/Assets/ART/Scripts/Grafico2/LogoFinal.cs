using UnityEngine;
using System.Collections;

public class LogoFinal : MonoBehaviour
{
    [Header("Configuraciones de sonido")]
    public AudioClip ambientClip;
    public Transform playerTransform;
    public float maxDistance = 20f;
    public float maxVolume = 0.5f;

    [Header("Configuraciones de Fade")]
    public float fadeDuration = 1f; // segundos que tarda el fade

    private AudioSource audioSource;
    private bool isFadingOut = false;
    private Coroutine fadeCoroutine;

    void Start()
    {
        Transform ambientTransform = transform.Find("AudioTodas");
        if (ambientTransform != null)
        {
            audioSource = ambientTransform.GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogError("No se encontró AudioSource en AmbientSound.");
            return;
        }

        audioSource.clip = ambientClip;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
        audioSource.volume = 0f; // arrancamos en volumen 0

        if (ambientClip != null)
        {
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
    }

    void Update()
    {
        if (playerTransform == null || audioSource == null || isFadingOut)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        float t = Mathf.Clamp01(distance / maxDistance);

        float targetVolume = Mathf.Lerp(maxVolume, 0f, t);

        audioSource.volume = targetVolume;
    }

    IEnumerator FadeIn()
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / fadeDuration);

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            float t = Mathf.Clamp01(distance / maxDistance);
            float targetVolume = Mathf.Lerp(maxVolume, 0f, t);

            audioSource.volume = targetVolume * normalized;

            yield return null;
        }
    }

    IEnumerator FadeOut()
    {
        isFadingOut = true;

        float startVolume = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float normalized = Mathf.Clamp01(elapsed / fadeDuration);

            audioSource.volume = Mathf.Lerp(startVolume, 0f, normalized);

            yield return null;
        }

        audioSource.Stop();
    }

    void OnDisable()
    {
        if (audioSource != null && gameObject.activeInHierarchy)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);

            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }
}
