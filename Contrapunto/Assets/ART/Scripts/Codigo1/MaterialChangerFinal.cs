using UnityEngine;

public class MaterialChangerFinal : MonoBehaviour
{
    [Header("Renderer objetivo")]
    public Renderer targetRenderer;

    [Header("Material nuevo")]
    public Material nuevoMaterial;

    public void CambiarMaterial()
    {
        if (targetRenderer != null && nuevoMaterial != null)
        {
            targetRenderer.material = nuevoMaterial;
        }
    }
}
