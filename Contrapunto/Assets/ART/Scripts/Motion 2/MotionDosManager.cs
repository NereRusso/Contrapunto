// Assets/ART/Scripts/Motion 2/MotionDosManager.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class MotionDosManager : MonoBehaviour
{
    [Header("Jugador y Fade")]
    public GameObject playerObject;         // <-- arrastrá el GameObject raíz del jugador
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;
    public AudioClip audio1Mili;

    [Header("Videos que se deben pausar al iniciar")]
    public List<VideoPlayer> videosPausados;

    [Header("Videos que deben arrancar en glitch (bajo fps)")]
    public List<VideoPlayer> videosConGlitch;
    public float glitchInterval = 0.3f;

    private List<Coroutine> glitchCoroutines = new List<Coroutine>();

    // Scripts del jugador
    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Start()
    {
        // Buscar scripts del jugador
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        // Desactivar controles
        if (movementScript != null) movementScript.enabled = false;
        if (inputScript != null) inputScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Pausar videos normales
        foreach (var vp in videosPausados)
            if (vp != null) vp.Pause();

        // Iniciar glitch en videos
        foreach (var vp in videosConGlitch)
            if (vp != null)
            {
                vp.Pause();
                var c = StartCoroutine(PlaySteppedVideo(vp));
                glitchCoroutines.Add(c);
            }

        // Empezar fade in
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        fadeCanvas.alpha = 1;
        fadeCanvas.blocksRaycasts = true;

        yield return new WaitForSeconds(delayBeforeFade);

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeCanvas.alpha = 1 - (timer / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0;
        fadeCanvas.blocksRaycasts = false;

        // Reproducir narración, callback y sonido normal
        if (audio1Mili != null)
        {
            NarrationManager.Instance
                .PlayNarration(audio1Mili, OnNarrationEnded, false);
        }
    }

    void OnNarrationEnded()
    {
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }

    private IEnumerator PlaySteppedVideo(VideoPlayer vp)
    {
        while (true)
        {
            vp.StepForward();
            yield return new WaitForSeconds(glitchInterval);
        }
    }

    public void RestaurarTodosVideos()
    {
        foreach (var c in glitchCoroutines)
            if (c != null) StopCoroutine(c);

        glitchCoroutines.Clear();

        foreach (var vp in videosPausados)
            if (vp != null) vp.Play();

        foreach (var vp in videosConGlitch)
            if (vp != null) vp.Play();
    }
}
