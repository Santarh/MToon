Shader "MToon"
{
	Properties
	{
		_Alpha ("Alpha", Range(0, 1)) = 1.0
		_Color ("Lit Color", Color) = (1,1,1,1)
		_ShadeColor ("Shade Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Lit Texture", 2D) = "white" {}
		[NoScaleOffset] _ShadeTexture ("Shade Texture", 2D) = "white" {}
		[Normal] _BumpMap ("Normal Texture", 2D) = "bump" {}
		_ReceiveShadowRate ("Receive Shadow", Range(0, 1)) = 1
		[NoScaleOffset] _ReceiveShadowTexture ("Receive Shadow Texture", 2D) = "white" {}
		_ShadeShift ("Shade Shift", Range(0, 1)) = 0.5
		_ShadeToony ("Shade Toony", Range(0, 1)) = 0.5
		_LightColorAttenuation ("Light Color Attenuation", Range(0, 1)) = 0
		[NoScaleOffset] _SphereAdd ("Sphere Texture(Add)", 2D) = "black" {}
		[NoScaleOffset] _OutlineWidthTexture ("Outline Width Tex", 2D) = "white" {}
		_OutlineWidth ("Outline Width", Range(0, 1)) = 0.5
		_OutlineColor ("Outline Color", Color) = (0,0,0,1)

		[HideInInspector] _DebugMode ("_DebugMode", Float) = 0.0
		[HideInInspector] _BlendMode ("_BlendMode", Float) = 0.0
		[HideInInspector] _OutlineMode ("_OutlineMode", Float) = 0.0
		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType" = "Opaque"  "Queue" = "Geometry" }
		Cull Back

		Pass 
		{
			Name "FORWARD"
			Tags { "LightMode" = "ForwardBase" }

			Blend [_SrcBlend] [_DstBlend]
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma shader_feature MTOON_DEBUG_NONE MTOON_DEBUG_NORMAL
			#pragma shader_feature MTOON_OUTLINE_NONE MTOON_OUTLINE_COLORED
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#include "MToonCore.cginc"
			#pragma vertex vert_with_geom
			#pragma geometry geom
			#pragma fragment frag
			#pragma multi_compile_fwdbase
			#pragma multi_compile_fog
			ENDCG
		}

		Pass 
		{
			Name "FORWARD_DELTA"
			Tags { "LightMode" = "ForwardAdd" }

			Blend [_SrcBlend] One
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma shader_feature MTOON_DEBUG_NONE MTOON_DEBUG_NORMAL
			#pragma shader_feature MTOON_OUTLINE_NONE MTOON_OUTLINE_COLORED
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#define MTOON_FORWARD_ADD
			#include "MToonCore.cginc"
			#pragma vertex vert_without_geom
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
			ENDCG
		}
	}
	Fallback "Diffuse"
	CustomEditor "MToonInspector"
}