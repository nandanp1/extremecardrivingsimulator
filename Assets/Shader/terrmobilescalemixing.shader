Shader "Mobile/Terrain/Scale Mixing" {
Properties {
 _MainTex ("Diffuse Texture for FallBack", 2D) = "white" {}
 _Control ("Control (RGBA)", 2D) = "white" {}
 _Splat3 ("Layer 3 (A)", 2D) = "white" {}
 _Splat2 ("Layer 2 (B)", 2D) = "white" {}
 _Splat1 ("Layer 1 (G)", 2D) = "white" {}
 _Splat0 ("Layer 0 (R)", 2D) = "white" {}
}

 SubShader {
  Tags { "RenderType"="Opaque" "SplatCount"="4" }

  CGPROGRAM
  #pragma surface surf Lambert
  #pragma target 2.0

  sampler2D _Control;
  sampler2D _Splat0;
  sampler2D _Splat1;
  sampler2D _Splat2;
  sampler2D _Splat3;

  struct Input {
   float2 uv_Control;
   float2 uv_Splat0;
  };

  void surf(Input input, inout SurfaceOutput output) {
   fixed4 control = tex2D(_Control, input.uv_Control);
   fixed4 color =
    control.r * tex2D(_Splat0, input.uv_Splat0) +
    control.g * tex2D(_Splat1, input.uv_Splat0) +
    control.b * tex2D(_Splat2, input.uv_Splat0) +
    control.a * tex2D(_Splat3, input.uv_Splat0);
   output.Albedo = color.rgb;
  }
  ENDCG
 }

 FallBack "Mobile/Diffuse"
}
