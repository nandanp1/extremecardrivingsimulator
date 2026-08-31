Shader "Particles/Additive (Soft)" {
Properties {
 _MainTex ("Particle Texture", 2D) = "white" {}
 _InvFade ("Soft Particles Factor", Range(0.01,3)) = 1
}
 SubShader {
  Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
  Pass {
   Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
   ZWrite Off
   Cull Off
   Fog { Color (0,0,0,0) }
   Blend One OneMinusSrcColor
   ColorMask RGB

   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0
   #pragma multi_compile_particles

   #include "UnityCG.cginc"

   sampler2D _MainTex;
   float4 _MainTex_ST;
   sampler2D_float _CameraDepthTexture;
   float _InvFade;

   struct appdata {
    float4 vertex : POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 pos : SV_POSITION;
    fixed4 color : COLOR;
    float2 texcoord : TEXCOORD0;
    #ifdef SOFTPARTICLES_ON
    float4 projectedPosition : TEXCOORD1;
    #endif
   };

   v2f vert(appdata input) {
    v2f output;
    output.pos = UnityObjectToClipPos(input.vertex);
    output.color = input.color;
    output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);

    #ifdef SOFTPARTICLES_ON
    output.projectedPosition = ComputeScreenPos(output.pos);
    COMPUTE_EYEDEPTH(output.projectedPosition.z);
    #endif

    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed4 vertexColor = input.color;

    #ifdef SOFTPARTICLES_ON
    float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(
     _CameraDepthTexture, UNITY_PROJ_COORD(input.projectedPosition)));
    float fade = saturate(_InvFade * (sceneDepth - input.projectedPosition.z));
    vertexColor.a *= fade;
    #endif

    fixed4 color = vertexColor * tex2D(_MainTex, input.texcoord);
    color.rgb *= color.a;
    return color;
   }
   ENDCG
  }
 }
}
