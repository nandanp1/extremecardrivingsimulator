Shader "AIMMobile/Billboard" {
Properties {
 _MainTex ("Texture Image", 2D) = "white" {}
}
 SubShader {
  Pass {
   Blend One One

   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"

   sampler2D _MainTex;

   struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 pos : SV_POSITION;
    float2 texcoord : TEXCOORD0;
   };

   v2f vert(appdata input) {
    v2f output;
    float4 viewCenter = mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0));
    viewCenter.xy -= input.vertex.xy;
    output.pos = mul(UNITY_MATRIX_P, viewCenter);
    output.texcoord = input.texcoord;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    return tex2D(_MainTex, input.texcoord);
   }
   ENDCG
  }
 }
}
