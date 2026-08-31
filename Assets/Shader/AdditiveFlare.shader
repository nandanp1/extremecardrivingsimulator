Shader "AIMMobile/Additive Flare"
{
    Properties
    {
        _MainTex ("Base (RGB)", 2D) = "white" {}
        _Color ("Color Emisivo", Color) = (1,1,1,1)
        _Light ("Intensidad Emisivo", Range(0,1)) = 1
        _DayNight ("Day/Night", Range(0,1)) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent+2" "RenderType"="Transparent" }

        Pass
        {
            Tags { "Queue"="Transparent+2" "RenderType"="Transparent" }
            Blend DstColor One

            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            half _Light;
            half _DayNight;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 textureColor = tex2D(_MainTex, input.uv) * _Color;
                fixed4 color = lerp(fixed4(0.0h, 0.0h, 0.0h, 0.0h), textureColor * _Light, textureColor.aaaa);
                return color * lerp(0.4h, 1.0h, _DayNight);
            }
            ENDCG
        }
    }
}
