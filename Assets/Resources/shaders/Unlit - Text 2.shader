Shader "Hidden/Unlit/Text 2"
{
    Properties { _MainTex ("Alpha (A)", 2D) = "white" {} }
    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "DisableBatching"="True" }
        Cull Off
        Lighting Off
        ZWrite Off
        Fog { Mode Off }
        Offset -1, -1
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            CGPROGRAM
            #pragma target 2.0
            #pragma vertex vert
            #pragma fragment frag
            #define NGUI_CLIP_COUNT 2
            #define NGUI_TEXT_MODE 1
            #include "NGUICompat.cginc"
            ENDCG
        }
    }
}
