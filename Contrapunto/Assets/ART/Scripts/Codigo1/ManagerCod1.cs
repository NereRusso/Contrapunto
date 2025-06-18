using UnityEngine;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class ManagerCod1 : MonoBehaviour
{
    [Header("Jugador")]
    public GameObject playerObject; // ? arrastrá el objeto raíz

    [Header("Glitch URP Feature")]
    public ScriptableRendererFeature glitchFeature;

    [Header("Control de rampa de glitch")]
    public GlitchController glitchController;
    public float delayBeforeGlitch = 1f;
    public float glitchDuration = 1.5f;

    [Header("Valores iniciales de glitch")]
    [Range(0f, 100f)] public float startNoiseAmount = 50f;
    [Range(0f, 100f)] public float startGlitchStrength = 100f;
    [Range(0f, 1f)] public float startScanLinesStrength = 1f;

    [Header("Narración")]
    public AudioClip audio1Nere;

    // Scripts del jugador
    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Start()
    {
        // Buscar los scripts de control
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        // Desactivar controles del jugador
        if (movementScript != null) movementScript.enabled = false;
        if (inputScript != null) inputScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

        // Iniciar glitch
        StartCoroutine(IntroGlitch());
    }

    IEnumerator IntroGlitch()
    {
        yield return new WaitForSeconds(delayBeforeGlitch);

        if (glitchFeature != null)
            glitchFeature.SetActive(true);

        // Seteo valores iniciales
        glitchController.noiseAmount = startNoiseAmount;
        glitchController.glitchStrength = startGlitchStrength;
        glitchController.scanLinesStrength = startScanLinesStrength;

        float elapsed = 0f;
        while (elapsed < glitchDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / glitchDuration;

            glitchController.noiseAmount = Mathf.Lerp(startNoiseAmount, 0f, t);
            glitchController.glitchStrength = Mathf.Lerp(startGlitchStrength, 0f, t);
            glitchController.scanLinesStrength = Mathf.Lerp(startScanLinesStrength, 0f, t);

            yield return null;
        }

        // Asegurar cero y desactivar glitch
        glitchController.noiseAmount = 0f;
        glitchController.glitchStrength = 0f;
        glitchController.scanLinesStrength = 0f;

        if (glitchFeature != null)
            glitchFeature.SetActive(false);

        // Reproducir narración, controles se activan al final
        if (audio1Nere != null)
            NarrationManager.Instance.PlayNarration(audio1Nere, OnNarrationEnded);
    }

    void OnNarrationEnded()
    {
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }
}
