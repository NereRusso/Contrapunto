using UnityEngine;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class VideoVisibilityManagerAdvanced : MonoBehaviour
{
    [Header("Asigná en el Inspector")]
    public VideoPlayer videoPlayer;      // tu VideoPlayer central
    public Renderer[] screenRenderers;   // todos los Renderers que usan este video
    public string texturePropertyName = "_MainTex";
    // en URP/Lit suele ser "_BaseMap"

    [Tooltip("Chequea visibilidad cada X segundos (0 = cada frame)")]
    public float checkInterval = 0.2f;

    Camera mainCam;
    Plane[] planes;
    float timer;

    void Awake()
    {
        mainCam = Camera.main;
        videoPlayer = videoPlayer ?? GetComponent<VideoPlayer>();
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.skipOnDrop = true;

        enabled = false;
    }

    void Start()
    {
        // Prepara y pausa en el primer frame listo
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += vp =>
        {
            // asignar textura a todos los materiales
            foreach (var rend in screenRenderers)
                rend.material.SetTexture(texturePropertyName, vp.texture);
            vp.Pause();
        };
    }

    void Update()
    {
        // control de frecuencia de chequeo
        if (checkInterval > 0f)
        {
            timer += Time.unscaledDeltaTime;
            if (timer < checkInterval) return;
            timer = 0f;
        }

        // calcular los planos del frustum en cada chequeo
        planes = GeometryUtility.CalculateFrustumPlanes(mainCam);

        // ¿alguno de los renderers está dentro del cono de visión?
        bool anyVisible = false;
        foreach (var rend in screenRenderers)
        {
            if (rend != null && GeometryUtility.TestPlanesAABB(planes, rend.bounds))
            {
                anyVisible = true;
                break;
            }
        }

        // disparar Play/Pause sólo cuando cambia el estado  
        if (anyVisible && !videoPlayer.isPlaying)
            videoPlayer.Play();
        else if (!anyVisible && videoPlayer.isPlaying)
            videoPlayer.Pause();
    }
}
