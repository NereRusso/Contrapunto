using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.Video;

public class TresDManager : MonoBehaviour
{
    [Header("Fade")]
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Jugador")]
    public FirstPersonController playerController;

    [Header("Narraci�n")]
    public AudioClip audio1Reni;

    void Awake()
    {
        // S�lo destruyo el overlay, sin tocar ning�n Clear Flags
        var vp = FindObjectOfType<VideoPlayer>();
        if (vp != null) Destroy(vp.gameObject);
        var ri = FindObjectOfType<RawImage>();
        if (ri != null) Destroy(ri.gameObject);

        // �No cambiamos nada m�s!
    }

    void Start()
    {
        // 3) Esperar un frame para que el primer draw de la c�mara borre todo
        StartCoroutine(DestroyOverlayNextFrame());

        // 4) Iniciar el fade-in
        playerController.enabled = false;
        StartCoroutine(FadeIn());
    }

    private IEnumerator DestroyOverlayNextFrame()
    {
        // espera un frame completo
        yield return null;

        // en caso que algo haya quedado, lo borramos de nuevo
        var vp = FindObjectOfType<VideoPlayer>();
        if (vp != null) Destroy(vp.gameObject);

        var ri = FindObjectOfType<RawImage>();
        if (ri != null) Destroy(ri.gameObject);
    }

    private IEnumerator FadeIn()
    {
        // pantalla 100% negra
        fadeCanvas.alpha = 1f;
        fadeCanvas.blocksRaycasts = true;

        // delay antes de empezar a destapar
        yield return new WaitForSeconds(delayBeforeFade);

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeCanvas.alpha = 1f - (t / fadeDuration);
            yield return null;
        }

        fadeCanvas.alpha = 0f;
        fadeCanvas.blocksRaycasts = false;
        playerController.enabled = true;

        if (audio1Reni != null)
            NarrationManager.Instance.PlayNarration(audio1Reni);
    }
}
