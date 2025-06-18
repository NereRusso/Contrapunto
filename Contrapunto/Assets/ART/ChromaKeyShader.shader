Shader "Custom/ChromaKeyShader"
{
    Properties
    {
        _MainTex ("Video Texture", 2D) = "white" {}
        _ChromaColor ("Chroma Key Color", Color) = (0,1,0,1) // Color verde por defecto
        _Threshold ("Chroma Key Threshold", Range(0,1)) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _ChromaColor;
            float _Threshold;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Calcular la diferencia entre el color actual y el color clave
                float diff = distance(texColor.rgb, _ChromaColor.rgb);

                // Si está dentro del umbral, hacerlo transparente
                if (diff < _Threshold)
                {
                    texColor.a = 0;
                }

                return texColor;
            }
            ENDCG
        }
    }
}
