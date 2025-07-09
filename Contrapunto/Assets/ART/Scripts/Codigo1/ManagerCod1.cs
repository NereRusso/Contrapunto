using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.UI;
using StarterAssets;
using UnityEngine.InputSystem;

public class ManagerCod1 : MonoBehaviour
{
    [Header("Jugador")]
    public GameObject playerObject;

    [Header("Video Intro")]
    public RawImage videoImage;
    public VideoPlayer videoPlayer;
    public float videoDelay = 1f;

    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Sonido ambiente")]
    public AudioSource ambientAudioSource;

    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    void Start()
    {
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        movementScript.enabled = false;
        inputScript.enabled = false;
        playerInput.enabled = false;

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

        videoImage.gameObject.SetActive(false);
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

        movementScript.enabled = true;
        inputScript.enabled = true;
        playerInput.enabled = true;
    }
}
