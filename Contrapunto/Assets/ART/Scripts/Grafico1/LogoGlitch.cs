using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;  // ? para ScriptableRendererFeature

public class LogoGlitch : MonoBehaviour
{
    [Header("Nombre de la escena a cargar")]
    public string sceneName;

    [Header("Jugador (para bloquear movimiento)")]
    public GameObject player;

    [Header("Referencias de sonido")]
    public GameObject sonidoAmbiente;
    public GameObject logoAmbiente;

    [Header("Glitch URP Feature")]
    public ScriptableRendererFeature glitchFeature;  // ? arrastrá aquí tu GlitchPass
    [Header("Control de rampa de glitch")]
    public GlitchController glitchController;     // arrastrar aquí tu GameObject con GlitchController
    public float glitchDuration = 2f;      // tiempo total de la rampa
    [Range(0f, 100f)] public float targetNoiseAmount = 50f;
    [Range(0f, 100f)] public float targetGlitchStrength = 100f;
    [Range(0f, 1f)] public float targetScanLinesStrength = 1f;
    [Header("Narración de glitch")]
    public AudioClip audio4Marti;

    private FirstPersonController fpsController;
    private StarterAssetsInputs starterInputs;
    private PlayerInput playerInput;
    private AudioSource sonidoAmbienteSource;
    private AudioSource logoAmbienteSource;
    private bool clicked = false;

    void Start()
    {
        if (player != null)
        {
            fpsController = player.GetComponent<FirstPersonController>();
            starterInputs = player.GetComponent<StarterAssetsInputs>();
            playerInput = player.GetComponent<PlayerInput>();
        }

        if (sonidoAmbiente != null) sonidoAmbienteSource = sonidoAmbiente.GetComponent<AudioSource>();
        if (logoAmbiente != null) logoAmbienteSource = logoAmbiente.GetComponent<AudioSource>();
    }

    void OnMouseDown()
    {
        if (clicked) return;
        clicked = true;
        // Espera 5 segundos antes de arrancar todo
        StartCoroutine(DelayedExecution());
    }

    IEnumerator DelayedExecution()
    {
        // 1) Delay inicial (puedes ajustar este WaitForSeconds)
        yield return new WaitForSeconds(1f);

        // 2) Bloquear movimiento y input
        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // 3) Detener sonidos
        if (sonidoAmbienteSource != null && sonidoAmbienteSource.isPlaying)
            sonidoAmbienteSource.Stop();
        if (logoAmbienteSource != null && logoAmbienteSource.isPlaying)
            logoAmbienteSource.Stop();

        if (audio4Marti != null)
            NarrationManager.Instance.PlayNarration(audio4Marti);

        // ? Esperar 1 segundo antes de activar el efecto de glitch
        yield return new WaitForSeconds(2f);

        // 4) Activar el feature URP
        if (glitchFeature != null)
            glitchFeature.SetActive(true);

        // 5) Arrancar la rampa de glitch
        yield return StartCoroutine(RampGlitch());


        // 7) Cargar la siguiente escena
        LoadScene();
    }

    IEnumerator RampGlitch()
    {
        float elapsed = 0f;
        // asegurate de empezar en cero
        glitchController.noiseAmount = 0f;
        glitchController.glitchStrength = 0f;
        glitchController.scanLinesStrength = 0f;

        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glitchDuration;

            // interpola de 0 ? target en función de t (0…1)
            glitchController.noiseAmount = Mathf.Lerp(0f, targetNoiseAmount, t);
            glitchController.glitchStrength = Mathf.Lerp(0f, targetGlitchStrength, t);
            glitchController.scanLinesStrength = Mathf.Lerp(0f, targetScanLinesStrength, t);

            yield return null;
        }
    }


    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}
