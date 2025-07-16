using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoTeles : MonoBehaviour
{
    [Header("Asigná en el Inspector")]
    public VideoPlayer videoPlayer;      // tu VideoPlayer central
    public Renderer[] screenRenderers;   // todos los Renderers que muestran este video
    public string texturePropertyName = "_MainTex";
    // en URP Lit suele ser "_BaseMap"

    void Start()
    {
        // Asegurate de desactivar Play On Awake en el VideoPlayer
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = false; // no necesitamos el primer frame

        // Prepará el video en background
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        // 1) Asigná la textura del video a todos los materiales
        var vidTex = vp.texture;
        foreach (var rend in screenRenderers)
            rend.material.SetTexture(texturePropertyName, vidTex);

        // 2) Suscribite al seekCompleted para pausar justo cuando termine de saltar
        vp.seekCompleted += OnSeekCompleted;

        // 3) Pedí el salto: ponemos el time a la mitad del clip y arrancamos
        vp.time = vp.length / 2.0;
        vp.Play();

        // Ya no necesitamos el prepareCompleted
        vp.prepareCompleted -= OnVideoPrepared;
    }

    void OnSeekCompleted(VideoPlayer vp)
    {
        // Pausamos en ese fotograma del medio
        vp.Pause();
        vp.seekCompleted -= OnSeekCompleted;
    }
}
