using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Video;
using StarterAssets;

[Serializable]
public class GlitchEntry
{
    [Tooltip("La palabra que se adivina en este turno")]
    public string glitchWord;

    [Tooltip("Palabras adicionales que se corrigen cuando acierto 'glitchWord'")]
    public string[] extraWords;
}

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
    public ScrollRect scrollRect;

    [Header("Video y Narración")]
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    public AudioClip audio2Nere;
    public AudioClip audio3Nere;
    public AudioClip audio12Nere;
    public AudioClip audio4Nere;
    public AudioClip audio13Nere;

    [Header("Texto Completo")]
    [TextArea(5, 20)]
    public string fullText;

    [Header("Glitch Entries")]
    public GlitchEntry[] glitchEntries;

    [Header("Sonidos Extra")]
    public AudioSource errorSound;
    public AudioClip clickActivacionSound;
    public AudioClip teclaSound;
    public AudioSource audioSource;
    public AudioClip correctoSound;

    [Header("Animaciones")]
    public float interactionDistance = 5f;
    public float screenOpenSpeed = 2f;
    public float screenCloseSpeed = 2f;
    public float cameraMoveSpeed = 2f;
    public float cameraReturnSpeed = 2f;

    [Header("Scroll Animation")]
    public float scrollAnimDuration = 0.3f;

    [Header("Glitch Settings")]
    public float glitchCycleDuration = 1f;
    [Range(0, 1)]
    public float scrambleRatio = 0.5f;
    public Color[] glitchColors = new Color[] { Color.red, Color.green, Color.blue };

    [Header("Current Word Font Scale (%)")]
    public float currentWordFontPercent = 150f;

    [Header("Glitch Pendientes")]
    public float minPendingInterval = 0.05f;
    public float maxPendingInterval = 0.15f;

    [Header("Cambio de material y luz")]
    public Renderer objetoOriginalRenderer;
    public Renderer objetoNuevoRenderer;
    public Light luzACambiar;
    public Color nuevoColorLuz;
    public float duracionTransicion = 2f;

    [Header("Extra")]
    public GameObject objectToDisable;
    public GameObject postProcesoMal;

    [Header("Prompt de click")]
    public GameObject clickCanvas;
    public float pickupRange = 5f;

    // Internals
    private string[] baseWords;
    private List<int>[] glitchPositions;
    private List<int>[] extraPositions;
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
    private Camera mainCameraRef;

    void Start()
    {
        fullTextDisplay.richText = true;
        screenTargetScale = screenCanvas.transform.localScale;
        screenCanvas.transform.localScale = Vector3.zero;
        screenCanvas.SetActive(false);

        playerController = FindObjectOfType<FirstPersonController>();
        eventCamera.gameObject.SetActive(false);
        videoPanel.SetActive(false);

        enterButton.onClick.AddListener(CheckWord);
        inputField.onValueChanged.AddListener(OnInputChanged);

        mainCameraRef = Camera.main;
        if (clickCanvas != null)
            clickCanvas.SetActive(false);
    }

    void Update()
    {
        // === Prompt de click (igual a PalancaJackpot) ===
        if (!hasActivated && mainCameraRef != null && clickCanvas != null)
        {
            float dist = Vector3.Distance(mainCameraRef.transform.position, transform.position);
            if (dist <= pickupRange)
            {
                Ray rayPrompt = new Ray(mainCameraRef.transform.position, mainCameraRef.transform.forward);
                if (Physics.Raycast(rayPrompt, out RaycastHit hitPrompt, pickupRange) && hitPrompt.transform == transform)
                    clickCanvas.SetActive(true);
                else
                    clickCanvas.SetActive(false);
            }
            else
            {
                clickCanvas.SetActive(false);
            }
        }

        // Activación original con clic en mouse
        if (!hasActivated && Input.GetMouseButtonDown(0))
        {
            var ray = mainCameraRef.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out var hit) && hit.transform == transform &&
                Vector3.Distance(mainCameraRef.transform.position, transform.position) <= interactionDistance)
            {
                ActivateScreen();
            }
        }

        // Apertura de pantalla
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

        // Movimiento de cámara de evento
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

        // Envío con ENTER
        if (screenCanvas.activeSelf && Input.GetKeyDown(KeyCode.Return))
            CheckWord();
    }

    void ActivateScreen()
    {
        if (clickCanvas != null)
            clickCanvas.SetActive(false);

        if (audioSource && clickActivacionSound)
            audioSource.PlayOneShot(clickActivacionSound);

        hasActivated = true;
        screenOpening = true;
        screenCanvas.SetActive(true);

        if (audio2Nere != null && NarrationManager.Instance != null)
            NarrationManager.Instance.PlayNarration(audio2Nere);

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

        // Preparar palabras y posiciones
        baseWords = fullText.Split(' ');
        int nEntries = glitchEntries.Length;
        glitchPositions = new List<int>[nEntries];
        extraPositions = new List<int>[nEntries];
        for (int k = 0; k < nEntries; k++)
        {
            glitchPositions[k] = new List<int>();
            extraPositions[k] = new List<int>();
            for (int i = 0; i < baseWords.Length; i++)
            {
                var w = baseWords[i].Trim(',', '.', '!', '?', ':', ';');
                if (w.Equals(glitchEntries[k].glitchWord, StringComparison.OrdinalIgnoreCase))
                    glitchPositions[k].Add(i);
                foreach (var ex in glitchEntries[k].extraWords)
                    if (w.Equals(ex, StringComparison.OrdinalIgnoreCase))
                        extraPositions[k].Add(i);
            }
        }
        writtenPositions.Clear();
        pendingScramble.Clear();

        // Iniciar glitches pendientes
        for (int k = currentWordIndex; k < nEntries; k++)
        {
            for (int j = 0; j < glitchPositions[k].Count; j++)
            {
                if (k == currentWordIndex && j == 0) continue;
                StartCoroutine(GlitchPending(glitchPositions[k][j], k));
            }
            foreach (int posExtra in extraPositions[k])
                StartCoroutine(GlitchPending(posExtra, k));
        }

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
        string sizeTag = currentWordFontPercent + "%";

        while (currentWordIndex < glitchEntries.Length)
        {
            string orig = glitchEntries[currentWordIndex].glitchWord;
            currentMarkup = $"<size={sizeTag}><color=#FFFFFF><b>{orig}</b></color></size>";
            UpdateDisplay();
            yield return new WaitForSeconds(correctTime);

            char[] arr = orig.ToCharArray();
            for (int i = 0; i < arr.Length; i++)
            {
                int j = rng.Next(i, arr.Length);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }
            string scr = new string(arr);
            Color c = glitchColors[UnityEngine.Random.Range(0, glitchColors.Length)];
            string hex = ColorUtility.ToHtmlStringRGB(c);
            currentMarkup = $"<size={sizeTag}><color=#{hex}><b>{scr}</b></color></size>";
            UpdateDisplay();
            yield return new WaitForSeconds(scrambleTime);
        }

        currentMarkup = null;
        UpdateDisplay();
    }

    IEnumerator GlitchPending(int pos, int wordIndex)
    {
        var rng = new System.Random();
        while (wordIndex >= currentWordIndex)
        {
            string word = glitchEntries[wordIndex].glitchWord;
            char[] arr = word.ToCharArray();
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

    void OnInputChanged(string currentText)
    {
        if (audioSource && teclaSound)
            audioSource.PlayOneShot(teclaSound);
    }

    void UpdateDisplay()
    {
        var disp = (string[])baseWords.Clone();
        for (int i = 0; i < disp.Length; i++)
        {
            if (writtenPositions.Contains(i))
                disp[i] = baseWords[i];
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
            glitchPositions[currentWordIndex].Count > 0 &&
            scrollRect != null)
        {
            StartCoroutine(SmoothScroll(glitchPositions[currentWordIndex][0], baseWords.Length));
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
        var entry = glitchEntries[currentWordIndex];
        if (entered.Equals(entry.glitchWord, StringComparison.OrdinalIgnoreCase))
        {
            if (audioSource && correctoSound)
                audioSource.PlayOneShot(correctoSound);

            foreach (int pos in glitchPositions[currentWordIndex])
                if (!writtenPositions.Contains(pos))
                    writtenPositions.Add(pos);

            foreach (var extra in entry.extraWords)
                for (int i = 0; i < baseWords.Length; i++)
                    if (baseWords[i].Trim(',', '.', '!', '?', ':', ';')
                        .Equals(extra, StringComparison.OrdinalIgnoreCase) &&
                        !writtenPositions.Contains(i))
                        writtenPositions.Add(i);

            currentMarkup = null;
            UpdateDisplay();
            currentWordIndex++;
            inputField.text = "";

            if (currentWordIndex == 1 && audio3Nere != null && NarrationManager.Instance != null)
                NarrationManager.Instance.PlayNarration(audio3Nere);

            if (currentWordIndex >= glitchEntries.Length)
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
            if (audio12Nere != null && NarrationManager.Instance != null)
                NarrationManager.Instance.PlayNarration(audio12Nere);
            inputField.text = "";
            inputField.ActivateInputField();
        }
    }

    public void PlayVideoAndClose()
    {
        videoPanel.SetActive(true);
        var glitchers = FindObjectsOfType<FloatingTextGlitcher>();
        foreach (var g in glitchers)
            g.StopGlitchAndType();

        videoPlayer.Play();
        NarrationManager.Instance.PlayNarration(audio4Nere);
        FindObjectOfType<AudioTriggerFinal>()?.CambiarYReproducir();
        FindObjectOfType<MaterialChangerFinal>()?.CambiarMaterial();
        videoPlayer.loopPointReached += _ => StartCoroutine(CloseCanvasCoroutine());
        postProcesoMal.SetActive(false);
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

        if (audio13Nere != null && NarrationManager.Instance != null)
            NarrationManager.Instance.PlayNarration(audio13Nere);

        if (playerController != null) playerController.enabled = true;
        if (objectToDisable != null) objectToDisable.SetActive(true);
        if (objetoOriginalRenderer != null && objetoNuevoRenderer != null)
            StartCoroutine(CruceMaterialObjetos(objetoOriginalRenderer, objetoNuevoRenderer, duracionTransicion));
        if (luzACambiar != null)
            StartCoroutine(TransicionarLuzColor(luzACambiar, nuevoColorLuz, duracionTransicion));
    }

    IEnumerator CruceMaterialObjetos(Renderer original, Renderer nuevo, float duracion)
    {
        Material matOriginal = original.material;
        Material matNuevo = nuevo.material;
        Color colorOrig = matOriginal.color;
        Color colorNuevo = matNuevo.color;
        original.gameObject.SetActive(true);
        nuevo.gameObject.SetActive(true);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duracion;
            float a1 = Mathf.Lerp(1f, 0f, t);
            float a2 = Mathf.Lerp(0f, 1f, t);
            matOriginal.color = new Color(colorOrig.r, colorOrig.g, colorOrig.b, a1);
            matNuevo.color = new Color(colorNuevo.r, colorNuevo.g, colorNuevo.b, a2);
            yield return null;
        }
        original.gameObject.SetActive(false);
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
