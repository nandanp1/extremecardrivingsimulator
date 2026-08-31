Shader "AIMMobile/AIM - Day Night" {
Properties {
 _MainTex ("Texture 1", 2D) = "white" {}
 _Lightmap1 ("Day LightMap", 2D) = "unity_Lightmap" {}
 _Lightmap2 ("Night LightMap", 2D) = "unity_Lightmap" {}
 _DayNight ("Day / Night", Range(0,1)) = 0
}

 SubShader {
  Tags { "RenderType"="Opaque" }

  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"

   struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    float2 lightmapTexcoord : TEXCOORD1;
   };

   struct v2f {
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    float2 lightmapTexcoord : TEXCOORD1;
   };

   sampler2D _MainTex;
   float4 _MainTex_ST;
   sampler2D _Lightmap1;
   sampler2D _Lightmap2;
   fixed _DayNight;

   v2f vert(appdata input) {
    v2f output;
    output.position = UnityObjectToClipPos(input.vertex);
    output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.lightmapTexcoord = input.lightmapTexcoord * unity_LightmapST.xy + unity_LightmapST.zw;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed4 albedo = tex2D(_MainTex, input.texcoord);
    fixed3 dayLightmap = tex2D(_Lightmap1, input.lightmapTexcoord).rgb;
    fixed3 nightLightmap = tex2D(_Lightmap2, input.lightmapTexcoord).rgb;
    fixed3 lighting = lerp(dayLightmap, nightLightmap, saturate(_DayNight));
    return fixed4(albedo.rgb * (2.0h * lighting), albedo.a);
   }
   ENDCG
  }
 }

 Fallback Off
}
