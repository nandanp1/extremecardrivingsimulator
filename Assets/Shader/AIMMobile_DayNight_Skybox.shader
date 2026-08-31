Shader "AIMMobile/AIM - Day Night Skybox" {
Properties {
 _Cube1 ("Day Skybox", CUBE) = "white" {}
 _Cube2 ("Night Skybox", CUBE) = "white" {}
 _DayNight ("Day / Night", Range(0,1)) = 0
}

 SubShader {
  Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
  Cull Off
  ZWrite Off

  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"

   struct appdata {
    float4 vertex : POSITION;
    float3 direction : TEXCOORD0;
   };

   struct v2f {
    float4 position : SV_POSITION;
    float3 direction : TEXCOORD0;
   };

   samplerCUBE _Cube1;
   samplerCUBE _Cube2;
   fixed _DayNight;

   v2f vert(appdata input) {
    v2f output;
    output.position = UnityObjectToClipPos(input.vertex);
    output.direction = input.direction;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed4 dayColor = texCUBE(_Cube1, input.direction);
    fixed4 nightColor = texCUBE(_Cube2, input.direction);
    return lerp(dayColor, nightColor, saturate(_DayNight));
   }
   ENDCG
  }
 }

 Fallback Off
}
