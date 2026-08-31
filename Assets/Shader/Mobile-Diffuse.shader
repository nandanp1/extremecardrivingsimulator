Shader "Mobile/Diffuse" {
Properties {
 _MainTex ("Base (RGB)", 2D) = "white" {}
}

 SubShader {
  LOD 150
  Tags { "RenderType"="Opaque" }

  CGPROGRAM
  #pragma surface surf Lambert
  #pragma target 2.0

  sampler2D _MainTex;

  struct Input {
   float2 uv_MainTex;
  };

  void surf(Input input, inout SurfaceOutput output) {
   fixed4 color = tex2D(_MainTex, input.uv_MainTex);
   output.Albedo = color.rgb;
   output.Alpha = color.a;
  }
  ENDCG
 }

 FallBack "Mobile/VertexLit"
}
