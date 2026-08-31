Shader "AIMMobile/AIM - Day Night Specular"
{
    Properties
    {
        _MainTex ("Texture 1", 2D) = "white" {}
        _Shininess ("Shininess", Float) = 10
        _SpecularIntensity ("Specular Intensity", Float) = 10
        _Lightmap1 ("Day LightMap", 2D) = "unity_Lightmap" {}
        _Lightmap2 ("Night LightMap", 2D) = "unity_Lightmap" {}
        _DayNight ("Day / Night", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "LightMode"="ForwardBase" }

        Pass
        {
            Tags { "LightMode"="ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _MainTex;
            sampler2D _Lightmap1;
            sampler2D _Lightmap2;
            fixed _Shininess;
            fixed _SpecularIntensity;
            fixed _DayNight;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                float2 uv2 : TEXCOORD1;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed3 specular : COLOR0;
                float2 uv : TEXCOORD0;
                float2 lightmapUV : TEXCOORD1;
            };

            v2f vert(appdata input)
            {
                v2f output;
                float3 worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
                fixed3 normalDirection = normalize(UnityObjectToWorldNormal(input.normal));
                fixed3 viewDirection = normalize(_WorldSpaceCameraPos - worldPosition);
                fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
                fixed diffuse = saturate(dot(normalDirection, lightDirection));
                fixed3 reflectedDirection = reflect(-lightDirection, normalDirection);
                fixed specularPower = pow(saturate(dot(reflectedDirection, viewDirection)), _Shininess);

                output.vertex = UnityObjectToClipPos(input.vertex);
                output.specular = _LightColor0.rgb * diffuse * specularPower * _SpecularIntensity;
                output.uv = input.uv;
                output.lightmapUV = input.uv2 * unity_LightmapST.xy + unity_LightmapST.zw;
                return output;
            }

            fixed4 frag(v2f input) : SV_Target
            {
                fixed4 textureColor = tex2D(_MainTex, input.uv);
                fixed3 lighting = 2.0h * lerp(
                    tex2D(_Lightmap1, input.lightmapUV).rgb,
                    tex2D(_Lightmap2, input.lightmapUV).rgb,
                    _DayNight);
                fixed3 color = lighting * textureColor.rgb;
                color += input.specular * lighting * textureColor.a;
                return fixed4(color, 1.0h);
            }
            ENDCG
        }
    }
}
