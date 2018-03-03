Shader "MToon"
{
	Properties
	{
		_Alpha ("Alpha", Range(0, 1)) = 1.0
        _Cutoff ("Alpha Cutoff", Range(0, 1)) = 0.5
        _AlphaTexture ("Alpha Texture", 2D) = "white" {}
		_Color ("Lit Color", Color) = (1,1,1,1)
		_ShadeColor ("Shade Color", Color) = (1,1,1,1)
		[NoScaleOffset] _MainTex ("Lit Texture", 2D) = "white" {}
		[NoScaleOffset] _ShadeTexture ("Shade Texture", 2D) = "white" {}
		_BumpScale ("Normal Scale", Float) = 1.0
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
		[HideInInspector] _CullMode ("_CullMode", Float) = 2.0
		[HideInInspector] _SrcBlend ("_SrcBlend", Float) = 1.0
		[HideInInspector] _DstBlend ("_DstBlend", Float) = 0.0
		[HideInInspector] _ZWrite ("_ZWrite", Float) = 1.0
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

			CGPROGRAM
			#pragma shader_feature MTOON_DEBUG_NORMAL
			#pragma shader_feature MTOON_OUTLINE_COLORED
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

			Blend [_SrcBlend] One
			ZWrite [_ZWrite]

			CGPROGRAM
			#pragma shader_feature MTOON_DEBUG_NORMAL
			#pragma shader_feature _NORMALMAP
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
			#define MTOON_FORWARD_ADD
			#include "MToonCore.cginc"
			#pragma vertex vert_without_geom
			#pragma fragment frag
			#pragma multi_compile_fwdadd_fullshadows
			#pragma multi_compile_fog
            #pragma multi_compile_instancing
			ENDCG
		}
		
		Pass
		{
		    Tags { "LightMode" = "ShadowCaster" }
		    
		    CGPROGRAM
			#pragma shader_feature _ _ALPHATEST_ON _ALPHABLEND_ON _ALPHAPREMULTIPLY_ON
		    #pragma vertex vert
		    #pragma fragment frag
		    #pragma multi_compile_shadowcaster
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            
            sampler3D _DitherMaskLOD;
            
            half _Alpha;
            half _Cutoff;
            sampler2D _AlphaTexture; float4 _AlphaTexture_ST;
            
            struct v2f
            {
                V2F_SHADOW_CASTER;
                float2 uv0 : TEXCOORD1;
            };
            
            v2f vert(appdata_base v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)
                o.uv0 = v.texcoord;
                return o;
            }
            
            float4 frag(UNITY_POSITION(vpos), v2f i) : SV_Target
            {
            #ifdef _ALPHATEST_ON
                half alpha = _Alpha * tex2D(_AlphaTexture, TRANSFORM_TEX(i.uv0, _AlphaTexture));
                clip(alpha - _Cutoff);
            #endif
            #ifdef _ALPHABLEND_ON
                half alpha = _Alpha * tex2D(_AlphaTexture, TRANSFORM_TEX(i.uv0, _AlphaTexture));
                half alphaRef = tex3D(_DitherMaskLOD, float3(vpos.xy * 0.25, alpha * 0.9375)).a;
                clip (alphaRef - 0.01);
            #endif
                SHADOW_CASTER_FRAGMENT(i)
            }
            ENDCG
		}
	}
	Fallback "Unlit/Texture"
	CustomEditor "MToonInspector"
}