Shader "VRM/MToon"
{
    Properties
    {
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _Color ("Lit Color + Alpha", Color) = (1,1,1,1)
        _ShadeColor ("Shade Color", Color) = (0.97, 0.81, 0.86, 1)
        [NoScaleOffset] _MainTex ("Lit Texture + Alpha", 2D) = "white" {}
        [NoScaleOffset] _ShadeTexture ("Shade Texture", 2D) = "white" {}
        _BumpScale ("Normal Scale", Float) = 1.0
        [Normal] _BumpMap ("Normal Texture", 2D) = "bump" {}
        _ReceiveShadowRate ("Receive Shadow", Range(0, 1)) = 1
        [NoScaleOffset] _ReceiveShadowTexture ("Receive Shadow Texture", 2D) = "white" {}
        _ShadeShift ("Shade Shift", Range(-1, 1)) = 0
        _ShadeToony ("Shade Toony", Range(0, 1)) = 0.9
        _LightColorAttenuation ("Light Color Attenuation", Range(0, 1)) = 0
        [NoScaleOffset] _SphereAdd ("Sphere Texture(Add)", 2D) = "black" {}
        _EmissionColor ("Color", Color) = (0,0,0)
        [NoScaleOffset] _EmissionMap ("Emission", 2D) = "white" {}
        [NoScaleOffset] _OutlineWidthTexture ("Outline Width Tex", 2D) = "white" {}
        _OutlineWidth ("Outline Width", Range(0, 1)) = 0.5
        _OutlineScaledMaxDistance ("Outline Scaled Max Distance", Range(1, 10)) = 1
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        _OutlineLightingMix ("Outline Lighting Mix", Range(0, 1)) = 1

        [HideInInspector] _DebugMode ("_DebugMode", Float) = 0.0
        [HideInInspector] _BlendMode ("_BlendMode", Float) = 0.0
        [HideInInspector] _OutlineWidthMode ("_OutlineWidthMode", Float) = 0.0
        [HideInInspector] _OutlineColorMode ("_OutlineColorMode", Float) = 0.0
        [HideInInspector] _CullMode ("_CullMode", Float) = 2.0
        [HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
        [HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
        [HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0
        [HideInInspector] _IsFirstSetup ("_IsFirstSetup", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque"  "Queue" = "Geometry" }
        Cull [_CullMode]

        Pass 
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite [_ZWrite]
            ZTest LEqual

            CGPROGRAM
            #pragma target 4.0
            #pragma shader_feature MTOON_DEBUG_NORMAL
            #pragma shader_feature _ MTOON_OUTLINE_WIDTH_WORLD MTOON_OUTLINE_WIDTH_SCREEN
            #pragma shader_feature _ MTOON_OUTLINE_COLOR_FIXED MTOON_OUTLINE_COLOR_MIXED
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #include "MToonCore.cginc"
            #pragma vertex vert_with_geom
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #pragma multi_compile_fog
            #pragma multi_compile_instancing
            ENDCG
        }

        Pass 
        {
            Name "FORWARD_DELTA"
            Tags { "LightMode" = "ForwardAdd" }

            Fog { Color (0,0,0,0) }
            Blend [_SrcBlend] One
            ZWrite Off
            ZTest LEqual

            CGPROGRAM
            #pragma target 3.0
            #pragma shader_feature MTOON_DEBUG_NORMAL
            #pragma shader_feature _NORMALMAP
            #pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
            #define MTOON_FORWARD_ADD
            #include "MToonCore.cginc"
            #pragma vertex vert_without_geom
            #pragma fragment frag
            #pragma multi_compile_fwdadd_fullshadows
            #pragma multi_compile_fog
            ENDCG
        }
        
        // Cast transparent shadow
        UsePass "Standard/SHADOWCASTER"
    }
    Fallback "Unlit/Texture"
    CustomEditor "MToonInspector"
}
