Shader "PaintBlend/UVPaintStamp"
{
    Properties
    {
        _MainTex ("Source", 2D) = "white" {}
        _BrushUV ("Brush UV", Vector) = (0.5, 0.5, 0, 0)
        _BrushColor ("Brush Colour", Color) = (1, 0, 1, 1)
        _BrushRadius ("Brush Radius", Float) = 0.035
        _BrushOpacity ("Brush Opacity", Float) = 1
        _BrushHardness ("Brush Hardness", Float) = 0.8
        _BrushAspect ("Brush Aspect", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float2 _BrushUV;
            float4 _BrushColor;
            float _BrushRadius;
            float _BrushOpacity;
            float _BrushHardness;
            float _BrushAspect;

            fixed4 frag(v2f_img input) : SV_Target
            {
                float2 delta = (input.uv - _BrushUV) * float2(_BrushAspect, 1.0);
                float distanceFromBrush = length(delta);
                float radius = max(_BrushRadius, 0.0001);
                float innerRadius = radius * saturate(_BrushHardness);
                float coverage = 1.0 - smoothstep(innerRadius, radius, distanceFromBrush);
                fixed4 original = tex2D(_MainTex, input.uv);
                float blend = saturate(coverage * _BrushOpacity * _BrushColor.a);
                return lerp(original, _BrushColor, blend);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
