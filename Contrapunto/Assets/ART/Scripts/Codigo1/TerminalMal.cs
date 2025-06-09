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

    [Header("Video y Narración")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public AudioClip audio2Nere;   // AudioSource para la narración

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
    public int contextWindow = 10;

    [Header("Extra")]
    public GameObject objectToDisable;

    // Internals
    bool hasActivated = false;
    bool screenOpening = false;
    bool movingCamera = false;
    Vector3 screenTargetScale;
    FirstPersonController playerController;
    int currentWordIndex = 0;

    Vector3 originalCamPosition;
    Quaternion originalCamRotation;

    void Start()
    {
        // Preparar pantalla cerrada
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

        ShowCurrentGlitchWord();
    }

    void ShowCurrentGlitchWord()
    {
        var words = fullText.Split(' ');
        int n = words.Length;
        int target = -1;

        // Buscar la palabra glitch
        for (int i = 0; i < n; i++)
        {
            var clean = words[i].Trim(',', '.', '!', '?', ':', ';');
            if (currentWordIndex < glitchWords.Length &&
                clean.Equals(glitchWords[currentWordIndex], StringComparison.OrdinalIgnoreCase))
            {
                target = i;
                break;
            }
        }

        var sb = new System.Text.StringBuilder();
        if (target < 0)
        {
            fullTextDisplay.text = fullText;
            return;
        }

        int start = Mathf.Max(0, target - contextWindow);
        int end = Mathf.Min(n, target + contextWindow + 1);

        if (start > 0) sb.Append("... ");
        for (int i = start; i < end; i++)
        {
            if (i == target)
                sb.Append($"<color=#FF00FF><b>{words[i]}</b></color> ");
            else
                sb.Append(words[i] + " ");
        }
        if (end < n) sb.Append("...");

        fullTextDisplay.text = sb.ToString();
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
        NarrationManager.Instance.PlayNarration(audio2Nere);                               // ? Reproduce la narración simultánea
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
        eventCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        if (playerController != null)
            playerController.enabled = true;

        // 4) Desactivar el objeto extra en escena
        if (objectToDisable != null)
            objectToDisable.SetActive(true);
    }
}
