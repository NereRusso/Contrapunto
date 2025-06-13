using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class MaterialChangerOnVideo : MonoBehaviour
{
    [Header("Video que dispara el cambio")]
    public VideoPlayer videoPlayer;

    [Header("Material compartido a reemplazar")]
    public Material originalMaterial;

    [Header("Material nuevo que queremos aplicar")]
    public Material newMaterial;

    [Header("Objetos afectados")]
    public List<Renderer> renderersToUpdate;

    private bool alreadyChanged = false;

    void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.started += OnVideoStarted;
        }
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
        {
            videoPlayer.started -= OnVideoStarted;
        }
    }

    void OnVideoStarted(VideoPlayer vp)
    {
        if (alreadyChanged) return;
        alreadyChanged = true;

        foreach (var rend in renderersToUpdate)
        {
            // Solo cambiar si el objeto todavía tiene el material original
            if (rend.sharedMaterial == originalMaterial || rend.material == originalMaterial)
            {
                rend.material = newMaterial;
            }
        }
    }
}
