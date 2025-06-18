using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;
using UnityEngine.InputSystem;

public class TresDManager : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject playerObject; // el GameObject raíz del jugador
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    [Header("Narración")]
    public AudioClip audio1Reni;

    // Scripts a desactivar temporalmente
    private FirstPersonController movementScript;
    private StarterAssetsInputs inputScript;
    private PlayerInput playerInput;

    private void Start()
    {
        // Obtener los scripts del jugador
        movementScript = playerObject.GetComponent<FirstPersonController>();
        inputScript = playerObject.GetComponent<StarterAssetsInputs>();
        playerInput = playerObject.GetComponent<PlayerInput>();

        // Desactivar todos los controles
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

        // Empieza la narración y esperamos que termine para activar controles
        if (audio1Reni != null)
        {
            NarrationManager.Instance.PlayNarration(audio1Reni, OnNarrationEnded);
        }
    }

    void OnNarrationEnded()
    {
        if (movementScript != null) movementScript.enabled = true;
        if (inputScript != null) inputScript.enabled = true;
        if (playerInput != null) playerInput.enabled = true;
    }
}
