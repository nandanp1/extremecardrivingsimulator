Shader "AIMMobile/AIM - Day Night Props" {
Properties {
 _MainTex ("Texture 1", 2D) = "white" {}
 _DayNight ("Day / Night", Range(0,1)) = 0
}

 SubShader {
  Tags { "RenderType"="Opaque" }

  Pass {
   Tags { "LightMode"="ForwardBase" }

   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"

   struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
   };

   sampler2D _MainTex;
   float4 _MainTex_ST;
   fixed _DayNight;

   v2f vert(appdata input) {
    v2f output;
    output.position = UnityObjectToClipPos(input.vertex);
    output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed3 albedo = tex2D(_MainTex, input.texcoord).rgb;
    fixed brightness = lerp(0.8h, 0.4h, saturate(_DayNight));
    return fixed4(albedo * brightness, 1.0h);
   }
   ENDCG
  }
 }

 Fallback Off
}
