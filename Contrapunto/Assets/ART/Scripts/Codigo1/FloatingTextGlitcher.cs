using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingTextGlitcher : MonoBehaviour
{
    public TextMeshProUGUI textMesh;
    public float glitchInterval = 0.2f;
    public int palabrasGlitcheadas = 3;
    public Color[] glitchColors = new Color[] { Color.red, Color.cyan, Color.green };
    public float typingSpeed = 0.02f;

    private string[] originalWords;
    private string originalText;
    private Coroutine glitchRoutine;

    private void Start()
    {
        if (textMesh == null)
            textMesh = GetComponent<TextMeshProUGUI>();

        originalText = textMesh.text;
        originalWords = originalText.Split(' ');
        glitchRoutine = StartCoroutine(GlitchLoop());
    }

    IEnumerator GlitchLoop()
    {
        var rng = new System.Random();

        while (true)
        {
            string[] tempWords = (string[])originalWords.Clone();
            HashSet<int> indices = new HashSet<int>();

            while (indices.Count < palabrasGlitcheadas)
            {
                indices.Add(rng.Next(0, tempWords.Length));
            }

            foreach (int i in indices)
            {
                string word = tempWords[i];
                char[] arr = word.ToCharArray();
                for (int j = 0; j < arr.Length; j++)
                {
                    int k = rng.Next(j, arr.Length);
                    (arr[j], arr[k]) = (arr[k], arr[j]);
                }

                string scrambled = new string(arr);
                Color color = glitchColors[Random.Range(0, glitchColors.Length)];
                string hex = ColorUtility.ToHtmlStringRGB(color);
                tempWords[i] = $"<color=#{hex}><b>{scrambled}</b></color>";
            }

            textMesh.text = string.Join(" ", tempWords);
            yield return new WaitForSeconds(glitchInterval);
        }
    }

    public void StopGlitchAndType()
    {
        if (glitchRoutine != null)
        {
            StopCoroutine(glitchRoutine);
            glitchRoutine = null;
        }

        StartCoroutine(TypeAndEraseLoop(originalText));
    }

    IEnumerator TypeAndEraseLoop(string fullText)
    {
        while (true)
        {
            // Escribir
            textMesh.text = "";
            for (int i = 0; i < fullText.Length; i++)
            {
                textMesh.text += fullText[i];
                yield return new WaitForSeconds(typingSpeed);
            }

            // Esperar 1 segundo con el texto completo
            yield return new WaitForSeconds(1f);

            // Borrar
            for (int i = fullText.Length; i > 0; i--)
            {
                textMesh.text = fullText.Substring(0, i - 1);
                yield return new WaitForSeconds(typingSpeed * 0.75f); // más rápido al borrar
            }

            // Esperar antes de volver a escribir
            yield return new WaitForSeconds(0.3f);
        }
    }

}
