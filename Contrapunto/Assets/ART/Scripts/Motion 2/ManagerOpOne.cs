using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro;

public class ManagerOpOne : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoFondo;
    public RawImage rawImageFondo;

    [Header("UI")]
    public GameObject canvasJuego;
    public GameObject canvasMenu;
    public GameObject flechasFijasContainer;
    public TextMeshProUGUI contadorTexto;

    public RawImage rawImageFade; // NUEVO: RawImage negro para el fade

    public ManagerDance managerDance; // Asignalo desde el Inspector

    public FlechaSpawner arrowSpawner;
    public static ManagerOpOne Instance;

    [Header("Efectos de sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoFallo;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        canvasJuego.SetActive(false);

        if (rawImageFade != null)
        {
            Color c = rawImageFade.color;
            c.a = 0f;
            rawImageFade.color = c;
            rawImageFade.gameObject.SetActive(true);
        }
    }

    public void ReproducirSonidoAcierto()
    {
        if (sonidoAcierto != null)
            audioSource.PlayOneShot(sonidoAcierto);
    }

    public void ReproducirSonidoFallo()
    {
        if (sonidoFallo != null)
            audioSource.PlayOneShot(sonidoFallo);
    }

    public void IniciarJuego()
    {
        StartCoroutine(FadeTransitionAndStart());
    }

    IEnumerator FadeTransitionAndStart()
    {
        float fadeDuration = 1f;
        float elapsed = 0f;

        Color c = rawImageFade.color;

        // Volumen inicial del VideoPlayer
        float startVolume = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            // Fade visual
            c.a = t;
            rawImageFade.color = c;

            // Fade de volumen si videoFondo está activo
            if (managerDance != null && managerDance.videoFondo != null)
            {
                managerDance.videoFondo.SetDirectAudioVolume(0, Mathf.Lerp(startVolume, 0f, t));
            }

            yield return null;
        }

        if (managerDance != null && managerDance.videoFondo != null)
        {
            managerDance.videoFondo.SetDirectAudioVolume(0, 0f);
        }

        canvasMenu.SetActive(false);
        canvasJuego.SetActive(true);

        rawImageFondo.texture = videoFondo.targetTexture;
        videoFondo.Play();

        flechasFijasContainer.SetActive(true);
        rawImageFade.gameObject.SetActive(false);

        StartCoroutine(ContadorInicio());
    }


    IEnumerator ContadorInicio()
    {
        string[] cuenta = { "3", "2", "1", "¡A sincronizar!" };

        foreach (string texto in cuenta)
        {
            contadorTexto.text = texto;
            contadorTexto.gameObject.SetActive(true);
            yield return new WaitForSeconds(1.5f);
            contadorTexto.gameObject.SetActive(false);
        }

        arrowSpawner.IniciarSpawner();
    }
}
