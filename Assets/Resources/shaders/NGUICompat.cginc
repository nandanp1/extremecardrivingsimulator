#ifndef AIM_NGUI_COMPAT_INCLUDED
#define AIM_NGUI_COMPAT_INCLUDED

#include "UnityCG.cginc"

sampler2D _MainTex;
float4 _MainTex_ST;
#if defined(NGUI_TEXTURE_CLIP)
sampler2D _ClipTex;
#endif

#if NGUI_CLIP_COUNT >= 1 || defined(NGUI_TEXTURE_CLIP)
float4 _ClipRange0;
#endif
#if NGUI_CLIP_COUNT >= 1
float4 _ClipArgs0;
#endif
#if NGUI_CLIP_COUNT >= 2
float4 _ClipRange1;
float4 _ClipArgs1;
#endif
#if NGUI_CLIP_COUNT >= 3
float4 _ClipRange2;
float4 _ClipArgs2;
#endif

struct appdata_ngui
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
    half4 color : COLOR;
};

struct v2f_ngui
{
    float4 vertex : SV_POSITION;
    float2 texcoord : TEXCOORD0;
    half4 color : COLOR0;
#if NGUI_CLIP_COUNT > 0 || defined(NGUI_TEXTURE_CLIP)
    float2 localPosition : TEXCOORD1;
#endif
};

v2f_ngui vert(appdata_ngui input)
{
    v2f_ngui output;
    output.vertex = UnityObjectToClipPos(input.vertex);
    output.texcoord = TRANSFORM_TEX(input.texcoord, _MainTex);
    output.color = input.color;
#if NGUI_CLIP_COUNT > 0 || defined(NGUI_TEXTURE_CLIP)
    output.localPosition = input.vertex.xy;
#endif
    return output;
}

float2 RotateForClip(float2 localPosition, float2 sineCosine)
{
    return float2(
        localPosition.x * sineCosine.y - localPosition.y * sineCosine.x,
        localPosition.x * sineCosine.x + localPosition.y * sineCosine.y);
}

float ClipFactor(float2 localPosition, float4 clipRange, float4 clipArgs)
{
    float2 rotated = RotateForClip(localPosition, clipArgs.zw);
    float2 distanceFromEdge = (1.0 - abs(rotated * clipRange.zw + clipRange.xy)) * clipArgs.xy;
    return saturate(min(distanceFromEdge.x, distanceFromEdge.y));
}

fixed4 frag(v2f_ngui input) : SV_Target
{
    half4 textureColor = tex2D(_MainTex, input.texcoord);

#if defined(NGUI_TEXT_MODE)
    half4 outputColor = input.color;
    outputColor.a *= textureColor.a;
#elif defined(NGUI_PACKED_MODE)
    half4 mixed = saturate(ceil(input.color - 0.5));
    half4 outputColor = saturate((mixed * 0.51 - input.color) / -0.49);
    half4 selectedMask = textureColor * mixed;
    outputColor.a *= selectedMask.r + selectedMask.g + selectedMask.b + selectedMask.a;
#else
    half4 outputColor = textureColor * input.color;
#endif

#if NGUI_CLIP_COUNT > 0
    float combinedClip = ClipFactor(input.localPosition, _ClipRange0, _ClipArgs0);
#if NGUI_CLIP_COUNT >= 2
    combinedClip = min(combinedClip, ClipFactor(input.localPosition, _ClipRange1, _ClipArgs1));
#endif
#if NGUI_CLIP_COUNT >= 3
    combinedClip = min(combinedClip, ClipFactor(input.localPosition, _ClipRange2, _ClipArgs2));
#endif
    outputColor.a *= combinedClip;
#if defined(NGUI_PREMULTIPLIED_MODE)
    outputColor.rgb *= combinedClip;
#endif
#endif
#if defined(NGUI_TEXTURE_CLIP)
    float2 clipUV = (input.localPosition * _ClipRange0.zw + _ClipRange0.xy) * 0.5 + 0.5;
    fixed maskAlpha = tex2D(_ClipTex, clipUV).a;
    outputColor.a *= maskAlpha;
#if defined(NGUI_PREMULTIPLIED_MODE)
    outputColor.rgb *= maskAlpha;
#endif
#endif

    return outputColor;
}

#endif
