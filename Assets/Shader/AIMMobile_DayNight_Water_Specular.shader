Shader "AIMMobile/AIM - Day Night Water Specular" {
Properties {
 _MainTex ("Texture 1", 2D) = "white" {}
 _WaveSpeed ("WaveSpeed", Range(0,0.02)) = 0.05
 _Shininess ("Shininess", Float) = 10
 _SpecularIntensity ("Specular Intensity", Float) = 10
 _DayNight ("Day / Night", Range(0,1)) = 0
}
 SubShader {
  Tags { "LIGHTMODE"="ForwardBase" }
  Pass {
   Tags { "LIGHTMODE"="ForwardBase" }

   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #pragma target 2.0

   #include "UnityCG.cginc"
   #include "Lighting.cginc"

   sampler2D _MainTex;
   float4 _MainTex_ST;
   half _WaveSpeed;
   half _Shininess;
   half _SpecularIntensity;
   half _DayNight;

   struct appdata {
    float4 vertex : POSITION;
    float3 normal : NORMAL;
    float4 texcoord : TEXCOORD0;
   };

   struct v2f {
    float4 pos : SV_POSITION;
    float2 uv0 : TEXCOORD0;
    float2 uv1 : TEXCOORD1;
    half3 specular : TEXCOORD2;
   };

   v2f vert(appdata input) {
    v2f output;
    float3 worldPosition = mul(unity_ObjectToWorld, input.vertex).xyz;
    half3 worldNormal = normalize(UnityObjectToWorldNormal(input.normal));
    half3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - worldPosition);
    half3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);
    half diffuse = max(0.0, dot(worldNormal, lightDirection));
    half reflectedSpecular = max(0.0, dot(reflect(-lightDirection, worldNormal), viewDirection));

    output.pos = UnityObjectToClipPos(input.vertex);
    output.specular = _LightColor0.rgb * diffuse
                    * pow(reflectedSpecular, _Shininess)
                    * _SpecularIntensity;

    float4 time1;
    time1.x = _Time.x * _WaveSpeed;
    time1.y = _Time.y * _WaveSpeed * 0.2;
    time1.zw = _Time.zw * _WaveSpeed;

    float4 movingUV0 = -input.texcoord + time1 * _WaveSpeed;
    float4 movingUV1 = input.texcoord - (_Time * 0.02) * _WaveSpeed;
    output.uv0 = movingUV0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    output.uv1 = movingUV1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
    return output;
   }

   fixed4 frag(v2f input) : SV_Target {
    fixed4 wave0 = tex2D(_MainTex, input.uv0);
    fixed4 wave1 = tex2D(_MainTex, input.uv1);
    fixed waveLight = clamp(1.0 - _DayNight, 0.1, 0.4);
    fixed3 color = waveLight * (wave0.rgb + wave1.rgb)
                 + input.specular * (wave0.a + wave1.a);
    return fixed4(color, 1.0);
   }
   ENDCG
  }
 }
}
