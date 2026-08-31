Shader "AIMMobile/AIM - Light Projector"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _Light ("Light Intensity", Range(0,10)) = 0
        _DayScale ("Day Scale", Range(0,1)) = 0
        _DayNight ("Day / Night", Range(0,1)) = 0
        _ShadowTex ("Cookie", 2D) = "" {}
        _FalloffTex ("FallOff", 2D) = "" { TexGen ObjectLinear }
    }

    SubShader
    {
        Pass
        {
            Tags { "Queue"="Background" }
            ZWrite Off
            Fog { Color (0,0,0,1) }
            Blend DstColor One
            ColorMask RGB
            Offset -1, -1

            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // Unity's shader upgrader renamed the legacy engine matrices to these
            // built-in Unity 6 identifiers; the recovered projection math is unchanged.
            float4x4 unity_Projector;
            float4x4 unity_ProjectorClip;
            sampler2D _ShadowTex;
            sampler2D _FalloffTex;
            fixed4 _Color;
            half _Light;
            half _DayScale;
            half _DayNight;

            struct appdata { float4 vertex : POSITION; };
            struct v2f
            {
                float4 vertex : SV_POSITION;
                float4 uvCookie : TEXCOORD0;
                float4 uvFalloff : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uvCookie = mul(unity_Projector, input.vertex);
                output.uvFalloff = mul(unity_ProjectorClip, input.vertex);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 cookie = tex2Dproj(_ShadowTex, UNITY_PROJ_COORD(input.uvCookie));
                fixed4 falloff = tex2D(_FalloffTex, input.uvFalloff.xy);
                fixed overlap = min(cookie.a, falloff.a);
                fixed4 projected = (cookie / exp(cookie)) * _Light * _Color * overlap;
                fixed4 result = fixed4(projected.rgb, cookie.a);
                return result * lerp(_DayScale, 1.0h, _DayNight);
            }
            ENDCG
        }
    }
}
