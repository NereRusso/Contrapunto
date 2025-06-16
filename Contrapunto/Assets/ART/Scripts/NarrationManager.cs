using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;        // <-- TextMeshPro
using UnityEngine.InputSystem;

public class NarrationManager : MonoBehaviour
{
    public static NarrationManager Instance;

    [Header("Narration")]
    public AudioSource narrationSource;
    private AudioClip lastNarrationClip;
    private bool isPlayingNarration;

    [Header("Subtítulos UI")]
    public TextMeshProUGUI subtitleText;  // Asignar en el Inspector

    [System.Serializable]
    public class ClipSubtitle
    {
        public AudioClip clip;
        [TextArea] public string subtitle;
    }

    [Header("Definición de subtítulos")]
    public List<ClipSubtitle> subtitles = new List<ClipSubtitle>();

    private Coroutine subtitleCoroutine;

    [HideInInspector]
    public bool repeatEnabled = true;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        // Detectar fin de reproducción
        if (isPlayingNarration && !narrationSource.isPlaying)
            isPlayingNarration = false;

        // Repetir última narración con E
        if (repeatEnabled &&
            Input.GetKeyDown(KeyCode.E) &&
            !isPlayingNarration &&
            lastNarrationClip != null)
        {
            PlayNarration(lastNarrationClip);
        }
    }

    public void PlayNarration(AudioClip clip)
    {
        if (clip == null) return;

        lastNarrationClip = clip;
        narrationSource.Stop();
        narrationSource.clip = clip;
        narrationSource.Play();
        isPlayingNarration = true;

        // Lanzar subtítulo
        ShowSubtitleForClip(clip);
    }

    private void ShowSubtitleForClip(AudioClip clip)
    {
        // Detener corrutina anterior si existe
        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        // Buscar el texto
        var entry = subtitles.Find(s => s.clip == clip);
        if (entry != null && subtitleText != null)
        {
            subtitleCoroutine = StartCoroutine(SubtitleRoutine(entry.subtitle, clip.length));
        }
    }

    private IEnumerator SubtitleRoutine(string text, float duration)
    {
        subtitleText.gameObject.SetActive(false);  // ?? Primero ocultamos
        subtitleText.text = text;                  // ?? Recién ahí seteamos el nuevo texto
        subtitleText.gameObject.SetActive(true);   // ??? Y lo mostramos limpio

        yield return new WaitForSeconds(duration);

        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }

}
