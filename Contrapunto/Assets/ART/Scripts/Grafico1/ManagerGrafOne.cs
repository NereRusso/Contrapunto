using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class ManagerGrafOne : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject playerObject;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Video Intro")]
    public RawImage videoImage;
    public VideoPlayer videoPlayer;
    public float videoDelay = 1f;

    [Header("Sonido ambiente")]
    public AudioSource ambientAudioSource;

    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Start()
    {
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        if (movementScript != null) movementScript.enabled = false;
        if (inputScript != null) inputScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        if (fadeCanvas != null)
        {
            fadeCanvas.alpha = 1;
            fadeCanvas.blocksRaycasts = true;
        }

        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = 0f;
            ambientAudioSource.Play();
        }

        StartCoroutine(ReproducirVideoYFade());
    }

    IEnumerator ReproducirVideoYFade()
    {
        yield return new WaitForSeconds(videoDelay);

        if (videoPlayer != null && videoImage != null)
        {
            videoImage.gameObject.SetActive(true);

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoImage.texture = videoPlayer.targetTexture;
            videoPlayer.Play();

            while (videoPlayer.isPlaying)
                yield return null;

            videoImage.gameObject.SetActive(false);
        }

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            fadeCanvas.alpha = 1 - t;

            if (ambientAudioSource != null)
                ambientAudioSource.volume = Mathf.Lerp(0f, 0.08f, t);

            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;

        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }
}
