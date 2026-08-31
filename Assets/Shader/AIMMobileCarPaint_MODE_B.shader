Shader "AIMMobile/CarPaint_MODE_B"
{
    Properties
    {
        _Color ("Main Color", Color) = (1,1,1,1)
        _MetalVal ("MetalVal", Float) = 0.5
        _PlasticVal ("PlasticVal", Float) = 0.5
        _MainTex ("MainTex (RGBA)", 2D) = "white" {}
        _Cubemap ("Cubemap", CUBE) = "_Skybox" {}
        _CubemapIntensity ("Cubemap Intensity", Float) = 1
        _LightIntensity ("Light Intensity", Float) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 150

        Pass
        {
            Name "FORWARD"
            Tags { "LIGHTMODE"="ForwardBase" "SHADOWSUPPORT"="true" "RenderType"="Opaque" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            samplerCUBE _Cubemap;
            fixed4 _Color;
            half _MetalVal;
            half _PlasticVal;
            half _CubemapIntensity;
            half _LightIntensity;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                half3 worldReflection : TEXCOORD1;
                half3 worldNormal : TEXCOORD2;
                fixed3 ambient : TEXCOORD3;
                SHADOW_COORDS(4)
            };

            v2f vert(appdata v)
            {
                v2f output;
                float3 worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
                half3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));

                output.pos = UnityObjectToClipPos(v.vertex);
                output.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                output.worldReflection = reflect(
                    worldPosition - _WorldSpaceCameraPos.xyz,
                    worldNormal);
                output.worldNormal = worldNormal;
                output.ambient = glstate_lightmodel_ambient.rgb;
                TRANSFER_SHADOW(output);
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 mask = tex2D(_MainTex, input.uv);
                fixed4 mixedColor = mask.r * _MetalVal;
                mixedColor += mask.g * _PlasticVal;
                mixedColor += mask.b * _Color;
                mixedColor += mask.a
                            * texCUBE(_Cubemap, input.worldReflection)
                            * _CubemapIntensity;

                fixed3 albedo = mixedColor.rgb * _LightIntensity;
                half diffuse = max(0.0, dot(
                    normalize(input.worldNormal),
                    _WorldSpaceLightPos0.xyz));
                half shadow = SHADOW_ATTENUATION(input);
                fixed3 color = albedo * _LightColor0.rgb
                             * diffuse * (shadow * 2.0)
                             + albedo * input.ambient;
                return fixed4(color, 0.0);
            }
            ENDCG
        }
    }

    FallBack "Diffuse"
}
