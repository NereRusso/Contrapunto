using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeScreen : MonoBehaviour
{
    public Image fadeImage;
    public float fadeDuration = 1.5f;

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
        gameObject.SetActive(false); // Opcional: ocultar el objeto cuando termina el fade out
    }
}
