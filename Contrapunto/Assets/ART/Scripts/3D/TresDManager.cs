using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class TresDManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject playerObject;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Video Intro")]
    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;

    [Header("Narración")]
    public AudioClip audio1Reni;

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

        StartCoroutine(ReproducirVideoYFade());
    }

    IEnumerator ReproducirVideoYFade()
    {
        if (videoPlayer != null && videoRawImage != null)
        {
            videoRawImage.gameObject.SetActive(true);

            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();

            while (videoPlayer.isPlaying)
                yield return null;

            videoRawImage.gameObject.SetActive(false);
        }

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeCanvas.alpha = 1;
        fadeCanvas.blocksRaycasts = true;

        yield return new WaitForSeconds(delayBeforeFade);

        // Activamos el fade de sonido ambiente justo cuando empieza el visual
        if (AmbientManager.Instance != null)
        {
            AmbientManager.Instance.FadeInGlobalAmbient();
        }

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            fadeCanvas.alpha = 1 - t;
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;

        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;

        if (audio1Reni != null && NarrationManager.Instance != null)
        {
            NarrationManager.Instance.PlayNarration(audio1Reni);
        }
    }
}
