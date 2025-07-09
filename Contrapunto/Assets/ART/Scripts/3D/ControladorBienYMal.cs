using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

public class ControladorBienYMal : MonoBehaviour
{
    [Header("Video")]
    public GameObject canvasVideo;
    public VideoPlayer videoPlayer;

    [Header("Objetos a reemplazar")]
    public List<GameObject> objetosADesactivar = new List<GameObject>();
    public List<GameObject> objetosAActivar = new List<GameObject>();

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

            // Esperar a que tenga duración válida
            while (videoPlayer.length == 0)
                yield return null;

            double mitad = videoPlayer.length / 2.0;
            yield return new WaitForSeconds((float)mitad);

            // Desactivar todos los objetos en la lista
            foreach (GameObject obj in objetosADesactivar)
            {
                if (obj != null)
                    obj.SetActive(false);
            }

            // Activar todos los objetos en la otra lista
            foreach (GameObject obj in objetosAActivar)
            {
                if (obj != null)
                    obj.SetActive(true);
            }
        }
    }
}
