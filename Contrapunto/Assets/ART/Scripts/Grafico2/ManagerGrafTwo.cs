using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;

public class ManagerGrafTwo : MonoBehaviour
{
    public FirstPersonController playerController;
    public CanvasGroup fadeCanvas;
    public float fadeDuration = 1.5f;
    public float delayBeforeFade = 1.0f;

    public AudioClip audio1Marti; // solo el sonido de narración

    private void Start()
    {
        playerController.enabled = false;
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

        playerController.enabled = true;

        if (audio1Marti != null)
        {
            NarrationManager.Instance.PlayNarration(audio1Marti);
        }
    }
}
