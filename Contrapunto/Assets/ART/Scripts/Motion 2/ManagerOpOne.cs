using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using TMPro;

public class GameManagerDDR : MonoBehaviour
{
    [Header("Video")]
    public VideoPlayer videoFondo;
    public RawImage rawImageFondo;

    [Header("UI")]
    public GameObject canvasJuego;
    public GameObject canvasMenu;
    public GameObject flechasFijasContainer;
    public TextMeshProUGUI contadorTexto; // O TextMeshProUGUI si usás TMP

    public FlechaSpawner arrowSpawner;
    public static GameManagerDDR Instance;

    [Header("Efectos de sonido")]
    public AudioSource audioSource;
    public AudioClip sonidoAcierto;
    public AudioClip sonidoFallo;

    void Awake()
    {
        Instance = this;
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


    void Start()
    {
        canvasJuego.SetActive(false);
    }

    public void IniciarJuego()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        canvasJuego.SetActive(true);
        canvasMenu.SetActive(false);

        rawImageFondo.texture = videoFondo.targetTexture;
        videoFondo.Play();

        flechasFijasContainer.SetActive(true);

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
