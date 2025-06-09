using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchController : MonoBehaviour
{
    public Material mat;
    public float noiseAmount;
    public float glitchStrength;
    public float scanLinesStrength;

    void Update()
    {
        mat.SetFloat("_NoiseAmount", noiseAmount);
        mat.SetFloat("_GlitchStreight", glitchStrength);
        mat.SetFloat("_ScanLinesSDtrength", scanLinesStrength);
    }
}
