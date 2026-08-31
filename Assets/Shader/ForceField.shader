Shader "AIMMobile/ForceField" {
Properties {
 _MainTex ("Texture 1", 2D) = "white" {}
}
 SubShader {
  Tags { "QUEUE"="Transparent" }
  Pass {
   Tags { "QUEUE"="Transparent" }
   ZWrite Off
   Cull Off
   Blend SrcAlpha One

   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"

   sampler2D _MainTex;
   float4 _MainTex_ST;

   struct appdata {
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    float2 uv2 : TEXCOORD2;
   };

   v2f vert(appdata input) {
    v2f output;
    output.pos = UnityObjectToClipPos(input.vertex);
    output.uv0 = input.texcoord;
    output.uv1 = input.texcoord + _Time.xy;
    output.uv2 = input.texcoord * 5.0 - (_Time * 0.3).xy;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    float2 uv0 = input.uv0 * _MainTex_ST.xy + _MainTex_ST.zw;
    float2 uv1 = input.uv1 * _MainTex_ST.xy + _MainTex_ST.zw;
    float2 uv2 = input.uv2 * _MainTex_ST.xy + _MainTex_ST.zw;

    fixed4 tex0 = tex2D(_MainTex, uv0);
    fixed4 tex1 = tex2D(_MainTex, uv1);
    fixed4 tex2 = tex2D(_MainTex, uv2);
    fixed force = tex0.g * tex1.r + tex0.g * 0.2 - tex2.b;
    return fixed4(force, force * 0.3, 0.0, 1.0);
   }
   ENDCG
  }
 }
}
