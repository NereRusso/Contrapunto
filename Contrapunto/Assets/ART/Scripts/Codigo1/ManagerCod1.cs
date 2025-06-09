using UnityEngine;
using System.Collections;
using StarterAssets;
using UnityEngine.Rendering.Universal;

public class ManagerCod1 : MonoBehaviour
{
    [Header("Jugador")]
    public FirstPersonController playerController;

    [Header("Glitch URP Feature")]
    public ScriptableRendererFeature glitchFeature;    // tu GlitchPass

    [Header("Control de rampa de glitch")]
    public GlitchController glitchController;         // arrastrar aquí tu objeto con GlitchController
    [Tooltip("Retraso antes de empezar a bajar el glitch")]
    public float delayBeforeGlitch = 1f;
    [Tooltip("Tiempo que tarda en ir de intensidad ? 0")]
    public float glitchDuration = 1.5f;

    [Header("Valores iniciales de glitch")]
    [Range(0f, 100f)] public float startNoiseAmount = 50f;
    [Range(0f, 100f)] public float startGlitchStrength = 100f;
    [Range(0f, 1f)] public float startScanLinesStrength = 1f;

    [Header("Narración")]
    public AudioClip audio1Nere; // sólo el sonido de narración

    private void Start()
    {
        // 1) Bloqueamos movimiento hasta que termine la rampa
        playerController.enabled = false;
        // 2) Iniciamos la corutina de “intro glitch”
        StartCoroutine(IntroGlitch());
    }

    IEnumerator IntroGlitch()
    {
        // Espera opcional antes de arrancar
        yield return new WaitForSeconds(delayBeforeGlitch);

        // Activa el feature de glitch
        if (glitchFeature != null)
            glitchFeature.SetActive(true);

        // Ponemos los valores iniciales (los mismos con los que terminaste la escena anterior)
        glitchController.noiseAmount = startNoiseAmount;
        glitchController.glitchStrength = startGlitchStrength;
        glitchController.scanLinesStrength = startScanLinesStrength;

        // Rampa de intensidad ? 0
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

        // Aseguramos 0 y desactivamos
        glitchController.noiseAmount = 0f;
        glitchController.glitchStrength = 0f;
        glitchController.scanLinesStrength = 0f;
        if (glitchFeature != null)
            glitchFeature.SetActive(false);

        // Ya habilitamos el jugador y lanzamos la narración
        playerController.enabled = true;
        if (audio1Nere != null)
            NarrationManager.Instance.PlayNarration(audio1Nere);
    }
}
