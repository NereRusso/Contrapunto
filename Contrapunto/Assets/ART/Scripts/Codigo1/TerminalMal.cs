using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using StarterAssets;

public class TerminalMal : MonoBehaviour
{
    [Header("Pantalla y C�maras")]
    public GameObject screenCanvas;
    public Camera playerCamera;
    public Camera eventCamera;
    public Transform eventCameraTarget;

    [Header("UI Gameplay")]
    public TextMeshProUGUI fullTextDisplay;
    public TMP_InputField inputField;
    public Button enterButton;
    [Tooltip("Arrastr� aqu� el Scroll Rect que contiene el TextMeshProUGUI")]
    public ScrollRect scrollRect;

    [Header("Video y Narraci�n")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public AudioClip audio2Nere;

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
    [Tooltip("Duraci�n en segundos del smooth scroll")]
    public float scrollAnimDuration = 0.3f;

    [Header("Glitch Settings")]
    [Tooltip("Duraci�n total de un ciclo glitch (correcto + scramble)")]
    public float glitchCycleDuration = 1f;
    [Range(0, 1)]
    [Tooltip("Porci�n del ciclo dedicada al scramble de la palabra actual")]
    public float scrambleRatio = 0.5f;
    [Tooltip("Colores usados en el efecto glitch para las dem�s palabras")]
    public Color[] glitchColors = new Color[] { Color.red, Color.green, Color.blue };

    [Header("Glitch Pendientes")]
    [Tooltip("Intervalo m�nimo entre glitches de palabras pendientes")]
    public float minPendingInterval = 0.05f;
    [Tooltip("Intervalo m�ximo entre glitches de palabras pendientes")]
    public float maxPendingInterval = 0.15f;

    [Header("Cambio de material y luz")]
    [Tooltip("Renderer del objeto original (material actual)")]
    public Renderer objetoOriginalRenderer;

    [Tooltip("Renderer del objeto con el material nuevo")]
    public Renderer objetoNuevoRenderer;

    public Light luzACambiar;
    public Color nuevoColorLuz;
    public float duracionTransicion = 2f;

    [Header("Extra")]
    public GameObject objectToDisable;

    // Internals
    private string[] baseWords;
    private List<int>[] glitchPositions;    // cada glitchWords[k] ? lista de �ndices en baseWords
    private List<int> writtenPositions = new List<int>();
    private Dictionary<int, string> pendingScramble = new Dictionary<int, string>();
    private Coroutine currentGlitchCoroutine;
    private bool hasActivated = false;
    private bool screenOpening = false;
    private bool movingCamera = false;
    private Vector3 screenTargetScale;
    private FirstPersonController playerController;
    private int currentWordIndex = 0;
    private Vector3 originalCamPosition;
    private Quaternion originalCamRotation;
    private string currentMarkup;

    void Start()
    {
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
        if (!hasActivated && Input.GetMouseButtonDown(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.transform == transform &&
                Vector3.Distance(Camera.main.transform.position, transform.position) <= interactionDistance)
            {
                ActivateScreen();
            }
        }

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

        if (screenCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
            CheckWord();
    }

    void ActivateScreen()
    {
        hasActivated = true;
        screenOpening = true;
        screenCanvas.SetActive(true);

        originalCamPosition = playerCamera.transform.position;
        originalCamRotation = playerCamera.transform.rotation;

        if (playerController != null)
            playerController.enabled = false;
        playerCamera.gameObject.SetActive(false);

        eventCamera.transform.position = originalCamPosition;
        eventCamera.transform.rotation = originalCamRotation;
        eventCamera.gameObject.SetActive(true);
        movingCamera = true;

        NarrationManager.Instance.repeatEnabled = false;

        // Partimos baseWords y calculamos todas las ocurrencias
        baseWords = fullText.Split(' ');
        glitchPositions = new List<int>[glitchWords.Length];
        for (int k = 0; k < glitchWords.Length; k++)
        {
            glitchPositions[k] = new List<int>();
            for (int i = 0; i < baseWords.Length; i++)
            {
                if (baseWords[i].Trim(',', '.', '!', '?', ':', ';')
                    .Equals(glitchWords[k], StringComparison.OrdinalIgnoreCase))
                {
                    glitchPositions[k].Add(i);
                }
            }
        }

        writtenPositions.Clear();
        pendingScramble.Clear();

        // Iniciamos GlitchPending para todas las ocurrencias >= current, incluidas duplicadas
        for (int k = currentWordIndex; k < glitchWords.Length; k++)
        {
            for (int j = 0; j < glitchPositions[k].Count; j++)
            {
                int pos = glitchPositions[k][j];
                // si es la primera ocurrencia de la actual, la maneja GlitchCurrent
                if (k == currentWordIndex && j == 0) continue;
                StartCoroutine(GlitchPending(pos, k));
            }
        }

        // Inicia GlitchCurrent (solo primera ocurrencia)
        StopCoroutineIfNeeded(ref currentGlitchCoroutine);
        currentGlitchCoroutine = StartCoroutine(GlitchCurrent());

        ScrollToCurrentWord();
        inputField.ActivateInputField();
    }

    IEnumerator GlitchCurrent()
    {
        var rng = new System.Random();
        float scrambleTime = glitchCycleDuration * scrambleRatio;
        float correctTime = glitchCycleDuration - scrambleTime;

        while (currentWordIndex < glitchWords.Length)
        {
            string orig = glitchWords[currentWordIndex];

            // fase blanco
            currentMarkup = $"<color=#FFFFFF><b>{orig}</b></color>";
            UpdateDisplay();
            yield return new WaitForSeconds(correctTime);

            // fase glitch
            char[] arr = orig.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                int j = rng.Next(i, arr.Length);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            string scr = new string(arr);
            Color c = glitchColors[UnityEngine.Random.Range(0, glitchColors.Length)];
            string hex = ColorUtility.ToHtmlStringRGB(c);
            currentMarkup = $"<color=#{hex}><b>{scr}</b></color>";
            UpdateDisplay();
            yield return new WaitForSeconds(scrambleTime);
        }

        currentMarkup = null;
        UpdateDisplay();
    }

    IEnumerator GlitchPending(int pos, int wordIndex)
    {
        var rng = new System.Random();
        // <-- aqu� cambiamos a >= para que las duplicadas sigan glitcheando incluso al seleccionar
        while (wordIndex >= currentWordIndex)
        {
            char[] arr = glitchWords[wordIndex].ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                int j = rng.Next(i, arr.Length);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            string scr = new string(arr);
            Color c = glitchColors[UnityEngine.Random.Range(0, glitchColors.Length)];
            string hex = ColorUtility.ToHtmlStringRGB(c);

            pendingScramble[pos] = $"<color=#{hex}><b>{scr}</b></color>";
            UpdateDisplay();

            float wait = UnityEngine.Random.Range(minPendingInterval, maxPendingInterval);
            yield return new WaitForSeconds(wait);
        }

        pendingScramble.Remove(pos);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        var disp = (string[])baseWords.Clone();
        for (int i = 0; i < disp.Length; i++)
        {
            if (writtenPositions.Contains(i))
            {
                disp[i] = baseWords[i];
            }
            else if (currentMarkup != null &&
         currentWordIndex < glitchPositions.Length &&
         glitchPositions[currentWordIndex].Count > 0 &&
         i == glitchPositions[currentWordIndex][0])
            {
                disp[i] = currentMarkup;
            }

            else if (pendingScramble.ContainsKey(i))
            {
                disp[i] = pendingScramble[i];
            }
        }
        fullTextDisplay.text = string.Join(" ", disp);
    }

    void ScrollToCurrentWord()
    {
        if (currentWordIndex < glitchPositions.Length &&
            glitchPositions[currentWordIndex].Count > 0)
        {
            int idx = glitchPositions[currentWordIndex][0];
            if (scrollRect != null)
                StartCoroutine(SmoothScroll(idx, baseWords.Length));
        }
    }

    IEnumerator SmoothScroll(int targetIdx, int total)
    {
        yield return null;
        float from = scrollRect.verticalNormalizedPosition;
        float to = 1f - (targetIdx / (float)(total - 1));
        float t = 0f;
        while (t < scrollAnimDuration)
        {
            t += Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Lerp(from, to, t / scrollAnimDuration);
            yield return null;
        }
        scrollRect.verticalNormalizedPosition = to;
    }

    void CheckWord()
    {
        string entered = inputField.text.Trim();
        if (entered.Equals(glitchWords[currentWordIndex], StringComparison.OrdinalIgnoreCase))
        {
            foreach (int pos in glitchPositions[currentWordIndex])
                writtenPositions.Add(pos);

            currentMarkup = null;
            UpdateDisplay();

            currentWordIndex++;
            inputField.text = "";

            if (currentWordIndex >= glitchWords.Length)
            {
                PlayVideoAndClose();
                return;
            }
            ScrollToCurrentWord();
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

        // ?? Detener glitch en textos flotantes
        FloatingTextGlitcher[] glitchers = FindObjectsOfType<FloatingTextGlitcher>();
        foreach (var g in glitchers)
        {
            g.StopGlitchAndType();
        }

        videoPlayer.Play();
        NarrationManager.Instance.PlayNarration(audio2Nere);
        videoPlayer.loopPointReached += _ => StartCoroutine(CloseCanvasCoroutine());
    }


    IEnumerator CloseCanvasCoroutine()
    {
        Vector3 from = screenCanvas.transform.localScale;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * screenCloseSpeed;
            screenCanvas.transform.localScale = Vector3.Lerp(from, Vector3.zero, t);
            yield return null;
        }

        screenCanvas.SetActive(false);
        videoPanel.SetActive(false);
        videoPlayer.Stop();

        Vector3 camFrom = eventCamera.transform.position;
        Quaternion rotFrom = eventCamera.transform.rotation;
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * cameraReturnSpeed;
            eventCamera.transform.position = Vector3.Lerp(camFrom, originalCamPosition, t);
            eventCamera.transform.rotation = Quaternion.Slerp(rotFrom, originalCamRotation, t);
            yield return null;
        }

        NarrationManager.Instance.repeatEnabled = true;
        eventCamera.gameObject.SetActive(false);
        playerCamera.gameObject.SetActive(true);
        if (playerController != null) playerController.enabled = true;

        if (objectToDisable != null)
            objectToDisable.SetActive(true);

        // Transici�n de opacidad entre objetos
        if (objetoOriginalRenderer != null && objetoNuevoRenderer != null)
            StartCoroutine(CruceMaterialObjetos(objetoOriginalRenderer, objetoNuevoRenderer, duracionTransicion));

        // Transici�n suave de luz
        if (luzACambiar != null)
            StartCoroutine(TransicionarLuzColor(luzACambiar, nuevoColorLuz, duracionTransicion));
    }


    IEnumerator CruceMaterialObjetos(Renderer original, Renderer nuevo, float duracion)
    {
        Material matOriginal = original.material;
        Material matNuevo = nuevo.material;

        Color colorOrig = matOriginal.color;
        Color colorNuevo = matNuevo.color;

        // Asegurarse que ambos objetos est�n activos durante la transici�n
        original.gameObject.SetActive(true);
        nuevo.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            float alphaOrig = Mathf.Lerp(1f, 0f, t);
            float alphaNuevo = Mathf.Lerp(0f, 1f, t);

            matOriginal.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, alphaOrig);
            matNuevo.color = new Color(colorNuevo.r, colorNuevo.g, colorNuevo.b, alphaNuevo);

            yield return null;
        }

        // Dejar solo el nuevo visible
        matOriginal.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, 0f);
        matNuevo.color = new Color(colorNuevo.r, colorNuevo.g, colorNuevo.b, 1f);

        original.gameObject.SetActive(false);  // ?? Desactivar el viejo
        nuevo.gameObject.SetActive(true);      // ?? Asegurar que el nuevo quede activo
    }




    IEnumerator TransicionarLuzColor(Light luz, Color destino, float duracion)
    {
        Color inicio = luz.color;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            luz.color = Color.Lerp(inicio, destino, t);
            yield return null;
        }
    }


    void StopCoroutineIfNeeded(ref Coroutine coroutine)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
    }
}
