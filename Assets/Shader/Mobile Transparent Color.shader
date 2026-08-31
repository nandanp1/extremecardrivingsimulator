Shader "AxesInMotion/Mobile Transparent Color" {
Properties {
 _Color ("Main Color", Color) = (0.3,0.3,0.3,0.5)
}
SubShader { 
 LOD 100
 Tags { "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
 Pass {
  Tags { "LIGHTMODE"="Vertex" "QUEUE"="Transparent" "IGNOREPROJECTOR"="true" "RenderType"="Transparent" }
  Lighting On
  SeparateSpecular On
  Material {
   Ambient [_Color]
   Diffuse [_Color]
   Emission [_Color]
   Specular [_Color]
   Shininess [_Color]
  }
  ZWrite Off
  Blend SrcAlpha OneMinusSrcAlpha
  AlphaTest Greater 0
  ColorMask RGB
 }
}
}