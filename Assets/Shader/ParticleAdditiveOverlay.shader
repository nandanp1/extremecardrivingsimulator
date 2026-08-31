Shader "Particles/AdditiveWithOverlay" {
Properties {
 _MainTex ("Particle Texture", 2D) = "white" {}
 _Color ("Color", Color) = (1,1,1,1)
}
SubShader { 
 Tags { "QUEUE"="Overlay+100" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Overlay+100" "RenderType"="Transparent" }
  ZTest Always
  ZWrite Off
  Cull Front
  Blend SrcAlpha One
  SetTexture [_MainTex] { ConstantColor [_Color] combine texture * constant, texture alpha }
 }
}
}