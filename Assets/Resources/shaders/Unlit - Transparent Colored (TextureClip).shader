Shader "Hidden/Unlit/Transparent Colored (TextureClip)"
{
    Properties
    {
        _MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
        _ClipTex ("Clip Mask (A)", 2D) = "white" {}
    }
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
            #define NGUI_CLIP_COUNT 0
            #define NGUI_TEXTURE_CLIP 1
            #include "NGUICompat.cginc"
            ENDCG
        }
    }
}
