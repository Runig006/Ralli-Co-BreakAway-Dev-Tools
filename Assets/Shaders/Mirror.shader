Shader "Custom/Mirror"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _TextureScale ("Texture Scale", Float) = 1.0 // Escala para cada textura dentro de cada isla
        _TextureRotation ("Texture Rotation (Degrees)", Float) = 0.0 // Ángulo de rotación de la textura en grados
        _StaticOffset ("Static Offset", Vector) = (0.1, 0.1, 0, 0) // Offset estático en cada eje
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float2 screenUV : TEXCOORD1; // Coordenadas de pantalla para el offset
            };

            sampler2D _MainTex;
            float4 _Color;
            float _TextureScale;
            float _TextureRotation;
            float2 _StaticOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                
                // Escalar las UVs para ajustar el tamaño de la textura dentro de cada isla
                o.uv = v.uv * _TextureScale;

                // Obtener las coordenadas de pantalla para el offset estático
                o.screenUV = o.pos.xy / o.pos.w; // Normalizar coordenadas de pantalla
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Aplicar fracción para crear repetición de la textura en cada isla
                float2 tiledUV = frac(i.uv);

                // Usar las coordenadas de pantalla como un offset estático
                float2 staticOffset = _StaticOffset * i.screenUV;

                // Convertir el ángulo de rotación a radianes
                float rotationRad = radians(_TextureRotation);
                float cosTheta = cos(rotationRad);
                float sinTheta = sin(rotationRad);

                // Ajustar las UVs al centro de la textura (0.5, 0.5) antes de rotar
                tiledUV -= 0.5;

                // Aplicar la rotación
                float2 rotatedUV;
                rotatedUV.x = tiledUV.x * cosTheta - tiledUV.y * sinTheta;
                rotatedUV.y = tiledUV.x * sinTheta + tiledUV.y * cosTheta;

                // Reajustar las UVs al centro y aplicar el offset estático
                rotatedUV += 0.5 + staticOffset;

                // Obtener el color de la textura usando las UV rotadas y desplazadas
                fixed4 mainColor = tex2D(_MainTex, rotatedUV);

                // Aplicar el tinte de color
                return mainColor * _Color;
            }
            ENDCG
        }
    }
}