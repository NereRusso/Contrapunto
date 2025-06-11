using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class FadeScreen : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;

    public List<VideoPlayer> videosToPause;
    public List<VideoPlayer> videosToGlitch;

    public float glitchInterval = 0.3f; // tiempo entre frames "congelados"

    private List<Coroutine> glitchCoroutines = new List<Coroutine>();

    public IEnumerator FadeOut()
    {
        float t = 0f;
        Color startColor = new Color(0, 0, 0, 1);
        Color targetColor = new Color(0, 0, 0, 0);

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, t / fadeDuration);
            yield return null;
        }

        fadeImage.color = targetColor;
        //gameObject.SetActive(false);

        // Esperamos 1 segundo más
        yield return new WaitForSeconds(2f);

        // ?? Pausamos ciertos videos
        foreach (var vp in videosToPause)
        {
            vp.Pause();
        }

        // ?? Simulamos "fps bajos" en otros
        foreach (var vp in videosToGlitch)
        {
            vp.Pause(); // detenemos primero
            Coroutine c = StartCoroutine(PlaySteppedVideo(vp));
            glitchCoroutines.Add(c);
        }
    }

    private IEnumerator PlaySteppedVideo(VideoPlayer vp)
    {
        while (true)
        {
            vp.StepForward(); // avanza un frame
            yield return new WaitForSeconds(glitchInterval); // pausa entre frames
        }
    }

    // ?? Por si querés frenar todo después
    public void StopAllGlitching()
    {
        foreach (var c in glitchCoroutines)
        {
            if (c != null)
                StopCoroutine(c);
        }

        glitchCoroutines.Clear();
    }
}
