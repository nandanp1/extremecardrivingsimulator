Shader "AIMMobile/SimpleDirectional" {
Properties {
 _MainTex ("Texture 1", 2D) = "white" {}
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
   #include "Lighting.cginc"

   struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float2 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    fixed4 lighting : COLOR0;
   };

   sampler2D _MainTex;
   float4 _MainTex_ST;

   v2f vert(appdata input) {
    v2f output;
    fixed3 normalDirection = normalize(UnityObjectToWorldNormal(input.normal));
    fixed3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
    fixed diffuse = saturate(dot(normalDirection, lightDirection));
    output.position = UnityObjectToClipPos(input.vertex);
    output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.lighting = fixed4(glstate_lightmodel_ambient.rgb + _LightColor0.rgb * diffuse, 1.0h);
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    return input.lighting * 2.2h * tex2D(_MainTex, input.texcoord);
   }
   ENDCG
  }
 }

 Fallback Off
}
