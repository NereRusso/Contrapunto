using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

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
    public ScriptableRendererFeature glitchFeature;

    [Header("Control de rampa de glitch")]
    public GlitchController glitchController;
    public float glitchDuration = 2f;
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
        StartCoroutine(DelayedExecution());
    }

    IEnumerator DelayedExecution()
    {
        yield return new WaitForSeconds(2f);

        if (fpsController != null) fpsController.enabled = false;
        if (starterInputs != null) starterInputs.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        if (sonidoAmbienteSource != null && sonidoAmbienteSource.isPlaying)
            sonidoAmbienteSource.Stop();
        if (logoAmbienteSource != null && logoAmbienteSource.isPlaying)
            logoAmbienteSource.Stop();

        if (glitchFeature != null)
            glitchFeature.SetActive(true);

        yield return StartCoroutine(RampGlitch());

        LoadScene();
    }

    IEnumerator RampGlitch()
    {
        // ?? Reproducir narración al iniciar el glitch
        if (audio4Marti != null)
            NarrationManager.Instance.PlayNarration(audio4Marti);

        float elapsed = 0f;
        glitchController.noiseAmount = 0f;
        glitchController.glitchStrength = 0f;
        glitchController.scanLinesStrength = 0f;

        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glitchDuration;

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
