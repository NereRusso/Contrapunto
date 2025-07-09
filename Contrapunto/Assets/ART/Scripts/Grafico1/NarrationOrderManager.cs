using UnityEngine;

public class NarracionOrdenManager : MonoBehaviour
{
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;

    void Awake()
    {
        if (CameraController.ordenNarraciones.Count == 0)
        {
            CameraController.ordenNarraciones.Add(audio1);
            CameraController.ordenNarraciones.Add(audio2);
            CameraController.ordenNarraciones.Add(audio3);
        }
    }
}
