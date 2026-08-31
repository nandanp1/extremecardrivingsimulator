Shader "Car Paint Reflective" {
Properties {
 _Color ("Main Color (RGB)", Color) = (1,1,1,1)
 _SpecColor ("Specular Color", Color) = (0.5,0.5,0.5,1)
 _Shininess ("Shininess", Range(0.03,1)) = 0.078125
 _ReflectColor ("Reflection Color (RGB) RefStrength (A)", Color) = (1,1,1,0.5)
 _LightIntensity ("Light Intensity", Float) = 1
 _MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {}
 _ReflMask ("Reflection Mask (A)", 2D) = "white" {}
 _Cube ("Reflection Cubemap", CUBE) = "_Skybox" { TexGen CubeReflect }
}
	//DummyShaderTextExporter
	
	SubShader{
		Tags { "RenderType" = "Opaque" }
		LOD 200
		CGPROGRAM
#pragma surface surf Lambert
#pragma target 3.0
		sampler2D _MainTex;
		struct Input
		{
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutput o)
		{
			float4 c = tex2D(_MainTex, IN.uv_MainTex);
			o.Albedo = c.rgb;
		}
		ENDCG
	}
}