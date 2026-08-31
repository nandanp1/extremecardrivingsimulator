Shader "Hidden/Hardsurface Pro Front Transparent" {
 SubShader {
  CGINCLUDE
  #include "UnityCG.cginc"
  #include "Lighting.cginc"
  #include "AutoLight.cginc"

  fixed4 _Color;
  half _Shininess;
  half _Gloss;
  half _Reflection;
  samplerCUBE _Cube;
  half _FrezPow;
  half _FrezFalloff;
  half _EdgeAlpha;
  half _Metalics;

  struct HardSurfaceTerms {
   half3 baseColor;
   half3 reflectionColor;
   half alpha;
   half fresnel;
  };

  HardSurfaceTerms EvaluateHardSurface(float3 worldPosition, half3 worldNormal) {
   HardSurfaceTerms terms;
   half3 normalDirection = normalize(worldNormal);
   half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition);
   half3 reflectionDirection = normalize(reflect(-viewDirection, normalDirection));

   half edge = 1.0 - abs(dot(reflectionDirection, normalDirection));
   half fresnelBase = pow(edge, _FrezFalloff);
   terms.alpha = _Color.a * (1.0 - fresnelBase * _EdgeAlpha);
   terms.fresnel = fresnelBase * _FrezPow;

   half reflectionStrength = _Reflection + terms.fresnel;
   half3 cubeColor = texCUBE(_Cube, reflectionDirection).rgb * reflectionStrength;
   half cubeLuminance = saturate(dot(cubeColor, half3(0.22, 0.707, 0.071)));
   half reflectedAlpha = cubeLuminance * cubeLuminance;

   terms.alpha = min(terms.alpha + reflectedAlpha, 1.0);
   terms.baseColor = _Color.rgb * (1.0 - saturate(reflectionStrength));
   terms.reflectionColor = lerp(
    cubeColor,
    cubeColor * _Color.rgb,
    (1.0 - reflectedAlpha) * _Metalics);
   return terms;
  }

  half HardSurfaceNdotL(half3 normalDirection, half3 lightDirection, half alpha) {
   half ndotl = dot(normalDirection, lightDirection);
   return max(ndotl, abs(ndotl) * (1.0 - alpha));
  }

  half HardSurfaceSpecular(
   half3 normalDirection,
   half3 lightDirection,
   half3 viewDirection,
   half fresnel)
  {
   half3 halfDirection = normalize(lightDirection + viewDirection);
   return pow(max(0.0, dot(normalDirection, halfDirection)), _Shininess * 128.0)
        * (_Gloss + fresnel);
  }

  struct v2fBase {
   float4 pos : SV_POSITION;
   float3 worldPosition : TEXCOORD0;
   half3 worldNormal : TEXCOORD1;
   #ifdef LIGHTMAP_ON
   float2 lightmapUV : TEXCOORD2;
   #endif
   LIGHTING_COORDS(3,4)
  };

  v2fBase vertBase(appdata_full v) {
   v2fBase output;
   output.pos = UnityObjectToClipPos(v.vertex);
   output.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
   output.worldNormal = UnityObjectToWorldNormal(v.normal);
   #ifdef LIGHTMAP_ON
   output.lightmapUV = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
   #endif
   TRANSFER_VERTEX_TO_FRAGMENT(output);
   return output;
  }

  fixed4 fragBase(v2fBase input) : SV_Target {
   half3 normalDirection = normalize(input.worldNormal);
   half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
   HardSurfaceTerms terms = EvaluateHardSurface(input.worldPosition, normalDirection);
   half attenuation = LIGHT_ATTENUATION(input);
   half3 color;
   half outputAlpha = terms.alpha;

   #ifdef LIGHTMAP_ON
   half3 bakedLight = DecodeLightmap(UNITY_SAMPLE_TEX2D(unity_Lightmap, input.lightmapUV));
   #if defined(SHADOWS_SCREEN)
   bakedLight = min(bakedLight, attenuation * 2.0);
   #endif
   color = terms.baseColor * bakedLight + terms.reflectionColor;
   #else
   half3 lightDirection = normalize(UnityWorldSpaceLightDir(input.worldPosition));
   half diffuse = HardSurfaceNdotL(normalDirection, lightDirection, terms.alpha);
   half specular = HardSurfaceSpecular(
    normalDirection, lightDirection, viewDirection, terms.fresnel);
   half3 directLight = (terms.baseColor * _LightColor0.rgb * diffuse
                      + _LightColor0.rgb * _SpecColor.rgb * specular)
                     * (attenuation * 2.0);
   half3 ambientLight = ShadeSH9(half4(normalDirection, 1.0));
   #ifdef VERTEXLIGHT_ON
   ambientLight += Shade4PointLights(
    unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
    unity_LightColor[0].rgb, unity_LightColor[1].rgb,
    unity_LightColor[2].rgb, unity_LightColor[3].rgb,
    unity_4LightAtten0, input.worldPosition, normalDirection);
   #endif
   color = directLight + terms.baseColor * ambientLight + terms.reflectionColor;
   outputAlpha += (_LightColor0.a * _SpecColor.a) * specular * attenuation;
   #endif

   return fixed4(color, outputAlpha);
  }

  struct v2fAdd {
   float4 pos : SV_POSITION;
   float3 worldPosition : TEXCOORD0;
   half3 worldNormal : TEXCOORD1;
   LIGHTING_COORDS(2,3)
  };

  v2fAdd vertAdd(appdata_full v) {
   v2fAdd output;
   output.pos = UnityObjectToClipPos(v.vertex);
   output.worldPosition = mul(unity_ObjectToWorld, v.vertex).xyz;
   output.worldNormal = UnityObjectToWorldNormal(v.normal);
   TRANSFER_VERTEX_TO_FRAGMENT(output);
   return output;
  }

  fixed4 fragAdd(v2fAdd input) : SV_Target {
   half3 normalDirection = normalize(input.worldNormal);
   half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - input.worldPosition);
   half3 lightDirection = normalize(UnityWorldSpaceLightDir(input.worldPosition));
   HardSurfaceTerms terms = EvaluateHardSurface(input.worldPosition, normalDirection);
   half attenuation = LIGHT_ATTENUATION(input);
   half diffuse = HardSurfaceNdotL(normalDirection, lightDirection, terms.alpha);
   half specular = HardSurfaceSpecular(
    normalDirection, lightDirection, viewDirection, terms.fresnel);
   half3 color = (terms.baseColor * _LightColor0.rgb * diffuse
                + _LightColor0.rgb * _SpecColor.rgb * specular)
               * (attenuation * 2.0);
   return fixed4(color, 0.0);
  }
  ENDCG

  Pass {
   Name "FORWARD"
   Tags { "LIGHTMODE"="ForwardBase" }
   ZWrite Off
   Blend SrcAlpha OneMinusSrcAlpha

   CGPROGRAM
   #pragma vertex vertBase
   #pragma fragment fragBase
   #pragma target 2.0
   #pragma multi_compile_fwdbase
   ENDCG
  }

  Pass {
   Name "FORWARD"
   Tags { "LIGHTMODE"="ForwardAdd" }
   ZWrite Off
   Fog { Color (0,0,0,0) }
   Blend One One

   CGPROGRAM
   #pragma vertex vertAdd
   #pragma fragment fragAdd
   #pragma target 2.0
   #pragma multi_compile_fwdadd_fullshadows
   ENDCG
  }
 }
 Fallback "Mobile/Particles/Multiply"
}
