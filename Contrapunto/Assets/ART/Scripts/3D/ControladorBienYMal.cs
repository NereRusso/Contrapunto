using UnityEngine;
using System.Collections;
using UnityEngine.Video;

public class ControladorBienYMal : MonoBehaviour
{
    public GameObject canvasVideo;
    public VideoPlayer videoPlayer;

    public GameObject objetoAReemplazar;
    public GameObject objetoReemplazo;

    public void IniciarSecuencia()
    {
        StartCoroutine(SecuenciaVideoYReemplazo());
    }

    private IEnumerator SecuenciaVideoYReemplazo()
    {
        yield return new WaitForSeconds(0.5f);

        if (canvasVideo != null)
            canvasVideo.SetActive(true);

        if (videoPlayer != null)
        {
            videoPlayer.Play();

            // Esperamos a que tenga duración válida
            while (videoPlayer.length == 0)
                yield return null;

            double mitad = videoPlayer.length / 2.0;

            yield return new WaitForSeconds((float)mitad);

            if (objetoAReemplazar != null)
                objetoAReemplazar.SetActive(false);

            if (objetoReemplazo != null)
                objetoReemplazo.SetActive(true);
        }
    }
}
