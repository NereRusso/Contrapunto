// Assets/ART/Scripts/Grafico2/ManagerGrafTwo.cs
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;

public class ManagerGrafTwo : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject playerObject; // arrastrá el objeto raíz del jugador
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Narración")]
    public AudioClip audio1Marti;

    // Scripts del jugador
    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Start()
    {
        // Obtener los scripts del jugador
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        // Desactivar control completo
        if (movementScript != null) movementScript.enabled = false;
        if (inputScript != null) inputScript.enabled = false;
        if (playerInput != null) playerInput.enabled = false;

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

        // Iniciar narración con callback; false = sonido normal (no lejano)
        if (audio1Marti != null)
        {
            NarrationManager.Instance
                .PlayNarration(audio1Marti, OnNarrationEnded, false);
        }
    }

    void OnNarrationEnded()
    {
        // Habilitar controles
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }
}
