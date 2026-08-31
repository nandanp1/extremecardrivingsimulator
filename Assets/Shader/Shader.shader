Shader "Skidmarks" {
Properties {
 _Color ("Main Color", Color) = (1,1,1,1)
 _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
SubShader { 
 Tags { "QUEUE"="Geometry+5" "RenderType"="Transparent" }
 Pass {
  Tags { "QUEUE"="Geometry+5" "RenderType"="Transparent" }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  AlphaTest Greater 0
  ColorMaterial AmbientAndDiffuse
  Offset -4, -4
  SetTexture [_MainTex] { combine texture, texture alpha * primary alpha }
  SetTexture [_MainTex] { ConstantColor [_Color] combine constant * previous }
 }
}
Fallback "Transparent/VertexLit"
}