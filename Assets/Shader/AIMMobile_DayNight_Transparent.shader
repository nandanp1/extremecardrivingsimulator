Shader "AIMMobile/AIM - Day Night Transparent" {
Properties {
 _MainTex ("Texture Image", 2D) = "white" {}
 _DayNight ("Day / Night", Range(0,1)) = 0
}

 SubShader {
  Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" "IgnoreProjector"="True" }
  Cull Off
  ZWrite On

  Pass {
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
   fixed _DayNight;

   v2f vert(appdata input) {
    v2f output;
    output.position = UnityObjectToClipPos(input.vertex);
    output.texcoord = input.texcoord;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed brightness = clamp(1.0h - _DayNight, 0.4h, 1.0h);
    fixed4 color = tex2D(_MainTex, input.texcoord) * brightness;
    clip(color.a - 0.1h);
    return color;
   }
   ENDCG
  }
 }

 Fallback Off
}
