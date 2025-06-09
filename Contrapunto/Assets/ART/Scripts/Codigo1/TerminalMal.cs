using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using StarterAssets;

public class TerminalMal : MonoBehaviour
{
    [Header("Pantalla y Cámaras")]
    public GameObject screenCanvas;
    public Camera playerCamera;
    public Camera eventCamera;
    public Transform eventCameraTarget;

    [Header("UI Gameplay")]
    public TextMeshProUGUI fullTextDisplay;
    public TMP_InputField inputField;
    public Button enterButton;
    [Tooltip("Arrastrá aquí el Scroll Rect que contiene el TextMeshProUGUI")]
    public ScrollRect scrollRect;

    [Header("Video y Narración")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public AudioClip audio2Nere;   // Narración

    [Header("Texto Completo")]
    [TextArea(5, 20)]
    public string fullText;

    [Header("Glitch Words")]
    public string[] glitchWords;
    public AudioSource errorSound;

    [Header("Animaciones")]
    public float interactionDistance = 5f;
    public float screenOpenSpeed = 2f;
    public float screenCloseSpeed = 2f;
    public float cameraMoveSpeed = 2f;
    public float cameraReturnSpeed = 2f;

    [Header("Scroll Animation")]
    [Tooltip("Duración en segundos del smooth scroll")]
    public float scrollAnimDuration = 0.3f;

    [Header("Glitch Scramble Settings")]
    [Tooltip("Tiempo total del ciclo scramble ? correcto")]
    public float glitchCycleDuration = 0.4f;
    [Tooltip("Proporción de tiempo que dura el scramble vs el texto correcto")]
    [Range(0, 1)]
    public float scrambleRatio = 0.25f;

    [Header("Extra")]
    public GameObject objectToDisable;

    // Internals
    private Coroutine glitchCoroutine;
    private bool hasActivated = false;
    private bool screenOpening = false;
    private bool movingCamera = false;
    private Vector3 screenTargetScale;
    private FirstPersonController playerController;
    private int currentWordIndex = 0;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;

    void Start()
    {
        // Preparamos la pantalla cerrada
        screenTargetScale = screenCanvas.transform.localScale;
        screenCanvas.transform.localScale = Vector3.zero;
        screenCanvas.SetActive(false);

        playerController = FindObjectOfType<FirstPersonController>();
        eventCamera.gameObject.SetActive(false);

        videoPanel.SetActive(false);
        enterButton.onClick.AddListener(CheckWord);
    }

    void Update()
    {
        // 1) Click en el cubo para abrir terminal
        if (!hasActivated && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.transform == transform)
            {
                if (Vector3.Distance(Camera.main.transform.position, transform.position) <= interactionDistance)
                    ActivateScreen();
            }
        }

        // 2) Animar apertura de pantalla
        if (screenOpening)
        {
            screenCanvas.transform.localScale = Vector3.Lerp(
                screenCanvas.transform.localScale,
                screenTargetScale,
                Time.deltaTime * screenOpenSpeed
            );
            if (Vector3.Distance(screenCanvas.transform.localScale, screenTargetScale) < 0.01f)
            {
                screenCanvas.transform.localScale = screenTargetScale;
                screenOpening = false;
            }
        }

        // 3) Mover cámara de evento
        if (movingCamera)
        {
            eventCamera.transform.position = Vector3.Lerp(
                eventCamera.transform.position,
                eventCameraTarget.position,
                Time.deltaTime * cameraMoveSpeed
            );
            eventCamera.transform.rotation = Quaternion.Lerp(
                eventCamera.transform.rotation,
                eventCameraTarget.rotation,
                Time.deltaTime * cameraMoveSpeed
            );
            if (Vector3.Distance(eventCamera.transform.position, eventCameraTarget.position) < 0.05f &&
                Quaternion.Angle(eventCamera.transform.rotation, eventCameraTarget.rotation) < 1f)
            {
                movingCamera = false;
            }
        }

        // 4) Enviar con ENTER
        if (screenCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
            CheckWord();
    }

    void ActivateScreen()
    {
        hasActivated = true;
        screenOpening = true;
        screenCanvas.SetActive(true);

        // Guardar posición/rotación original de la cámara de jugador
        originalCamPosition = playerCamera.transform.position;
        originalCamRotation = playerCamera.transform.rotation;

        // Desactivar jugador y su cámara
        if (playerController != null) playerController.enabled = false;
        playerCamera.gameObject.SetActive(false);

        // Preparar y activar cámara de evento
        eventCamera.transform.position = originalCamPosition;
        eventCamera.transform.rotation = originalCamRotation;
        eventCamera.gameObject.SetActive(true);
        movingCamera = true;

        // Ocultar cursor y enfocar input
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        EventSystem.current.SetSelectedGameObject(inputField.gameObject);
        inputField.ActivateInputField();

        // Deshabilitar la tecla E de repetición global
        NarrationManager.Instance.repeatEnabled = false;

        ShowCurrentGlitchWord();
    }

    void ShowCurrentGlitchWord()
    {
        if (currentWordIndex >= glitchWords.Length)
        {
            // Si ya no hay más palabras, mostramos todo el texto sin highlight
            fullTextDisplay.text = fullText;
            return;
        }

        // 1) Busca el índice de la palabra objetivo
        string target = glitchWords[currentWordIndex];
        string[] words = fullText.Split(' ');
        int n = words.Length;
        int targetIdx = -1;
        for (int i = 0; i < n; i++)
        {
            var clean = words[i].Trim(',', '.', '!', '?', ':', ';');
            if (clean.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                targetIdx = i;
                break;
            }
        }

        // 2) Resaltado estático en BLANCO
        if (targetIdx >= 0)
            words[targetIdx] = $"<color=#FFFFFF><b>{words[targetIdx]}</b></color>";

        fullTextDisplay.text = string.Join(" ", words);

        // 3) Scroll suave hasta la palabra
        if (targetIdx >= 0 && scrollRect != null)
            StartCoroutine(ScrollToGlitchWord(targetIdx, n));

        // 4) Arrancamos el coroutine de scramble glitch con colores rojo, verde o azul
        if (glitchCoroutine != null)
            StopCoroutine(glitchCoroutine);
        glitchCoroutine = StartCoroutine(AnimateScramble(targetIdx, target, n));
    }

    IEnumerator ScrollToGlitchWord(int targetIndex, int totalWords)
    {
        // Esperar un frame para que el layout se actualice
        yield return null;

        float targetNormalized = 1f - (targetIndex / (float)(totalWords - 1));
        float startNormalized = scrollRect.verticalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < scrollAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scrollAnimDuration);
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(startNormalized, targetNormalized, t);
            yield return null;
        }

        scrollRect.verticalNormalizedPosition = targetNormalized;
    }

    void CheckWord()
    {
        string entered = inputField.text.Trim();
        if (entered.Equals(glitchWords[currentWordIndex], StringComparison.OrdinalIgnoreCase))
        {
            currentWordIndex++;
            inputField.text = "";

            if (currentWordIndex >= glitchWords.Length)
            {
                PlayVideoAndClose();
                return;
            }

            ShowCurrentGlitchWord();
            inputField.ActivateInputField();
        }
        else
        {
            errorSound.Play();
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    void PlayVideoAndClose()
    {
        videoPanel.SetActive(true);
        videoPlayer.Play();
        NarrationManager.Instance.PlayNarration(audio2Nere);
        videoPlayer.loopPointReached += _ => StartCoroutine(CloseCanvasCoroutine());
    }

    IEnumerator CloseCanvasCoroutine()
    {
        // 1) Cerrar Canvas con animación inversa
        Vector3 fromScale = screenCanvas.transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * screenCloseSpeed;
            screenCanvas.transform.localScale = Vector3.Lerp(fromScale, Vector3.zero, t);
            yield return null;
        }
        screenCanvas.SetActive(false);
        videoPanel.SetActive(false);
        videoPlayer.Stop();

        // 2) Animar retorno de cámara
        Vector3 camFromPos = eventCamera.transform.position;
        Quaternion camFromRot = eventCamera.transform.rotation;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraReturnSpeed;
            eventCamera.transform.position = Vector3.Lerp(camFromPos, originalCamPosition, t);
            eventCamera.transform.rotation = Quaternion.Slerp(camFromRot, originalCamRotation, t);
            yield return null;
        }

        // 3) Restaurar jugador y cámara original
        NarrationManager.Instance.repeatEnabled = true;
        eventCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        if (playerController != null)
            playerController.enabled = true;

        // 4) Desactivar el objeto extra en escena
        if (objectToDisable != null)
            objectToDisable.SetActive(true);
    }

    IEnumerator AnimateScramble(int targetIndex, string originalWord, int totalWords)
    {
        var baseWords = fullText.Split(' ');
        float scrambleTime = glitchCycleDuration * scrambleRatio;
        float correctTime = glitchCycleDuration - scrambleTime;
        // Solo rojo, verde y azul
        // Ejemplo con dos tonos propios
        Color[] glitchColors = new Color[] {
    new Color(1f, 0.2f, 0.2f),    // rojo fuerte
    new Color(0.1f, 0.8f, 0.5f),  // verde-agua
    new Color(0.8f, 0.3f, 1f)     // violeta
};

        System.Random rng = new System.Random();

        while (currentWordIndex < glitchWords.Length &&
               glitchWords[currentWordIndex].Equals(originalWord, StringComparison.OrdinalIgnoreCase))
        {
            // 1) Scramble de letras
            char[] arr = originalWord.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                int j = rng.Next(i, arr.Length);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            string scrambled = new string(arr);

            // 2) Color glitch (RGB puro)
            Color randC = glitchColors[UnityEngine.Random.Range(0, glitchColors.Length)];
            string hex = ColorUtility.ToHtmlStringRGB(randC);
            baseWords[targetIndex] = $"<color=#{hex}><b>{scrambled}</b></color>";
            fullTextDisplay.text = string.Join(" ", baseWords);
            yield return new WaitForSeconds(scrambleTime);

            // 3) Restaurar en blanco
            baseWords[targetIndex] = $"<color=#FFFFFF><b>{originalWord}</b></color>";
            fullTextDisplay.text = string.Join(" ", baseWords);
            yield return new WaitForSeconds(correctTime);
        }

        // Al salir, asegurar el blanco final
        baseWords[targetIndex] = $"<color=#FFFFFF><b>{originalWord}</b></color>";
        fullTextDisplay.text = string.Join(" ", baseWords);
    }
}
