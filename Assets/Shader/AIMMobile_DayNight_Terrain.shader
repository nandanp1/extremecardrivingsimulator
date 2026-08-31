Shader "AIMMobile/AIM - Day Night Terrain"
{
    Properties
    {
        _MainTex ("Mask Texture", 2D) = "white" {}
        _Texture1 ("Texture 1", 2D) = "white" {}
        _Texture2 ("Texture 2", 2D) = "white" {}
        _Texture3 ("Texture 3", 2D) = "white" {}
        _Texture4 ("Texture 4", 2D) = "white" {}
        _Lightmap1 ("Day LightMap", 2D) = "unity_Lightmap" {}
        _Lightmap2 ("Night LightMap", 2D) = "unity_Lightmap" {}
        _DayNight ("Day / Night", Range(0,1)) = 0
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _Texture1;
            sampler2D _Texture2;
            sampler2D _Texture3;
            sampler2D _Texture4;
            sampler2D _Lightmap1;
            sampler2D _Lightmap2;
            float4 _MainTex_ST;
            fixed _DayNight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 maskUV : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
                float2 textureUV : TEXCOORD2;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.maskUV = input.uv;
                output.lightmapUV = input.uv2 * unity_LightmapST.xy + unity_LightmapST.zw;
                output.textureUV = TRANSFORM_TEX(input.uv, _MainTex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, input.maskUV);
                fixed4 terrainColor =
                    mask.r * tex2D(_Texture1, input.textureUV) +
                    mask.g * tex2D(_Texture2, input.textureUV) +
                    mask.b * tex2D(_Texture3, input.textureUV) +
                    mask.a * tex2D(_Texture4, input.textureUV);
                fixed3 lighting = 2.0h * lerp(
                    tex2D(_Lightmap1, input.lightmapUV).rgb,
                    tex2D(_Lightmap2, input.lightmapUV).rgb,
                    _DayNight);
                return fixed4(terrainColor.rgb * lighting, terrainColor.a);
            }
            ENDCG
        }
    }
}
