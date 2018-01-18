// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#include "Lighting.cginc"
#include "AutoLight.cginc"
#pragma target 3.0

half _Alpha;
fixed4 _LitColor;
fixed4 _ShadeColor;
sampler2D _LitTexture; float4 _LitTexture_ST;
sampler2D _ShadeTexture; float4 _ShadeTexture_ST;
sampler2D _NormalTexture; float4 _NormalTexture_ST;
sampler2D _ReceiveShadowTexture; float4 _ReceiveShadowTexture_ST;
half _ReceiveShadowRate;
half _ShadeShift;
half _ShadeToony;
half _LightColorAttenuation;
half _NormalFromVColorRate;
half _NormalCylinderizeRate;
float3 _NormalCylinderizePos;
float3 _NormalCylinderizeAxis;
sampler2D _SphereAdd;
sampler2D _OutlineWidthTexture; float4 _OutlineWidthTexture_ST;
half _OutlineWidth;
fixed4 _OutlineColor;

struct v2g
{
	float4 posLocal : TEXCOORD0;
	float3 normalLocal : TEXCOORD1;
	float4 tangentLocal : TEXCOORD2;
	float2 uv0 : TEXCOORD3;
	float2 uv1 : TEXCOORD4;
	float outlineWidth : TEXCOORD5;
	fixed4 color : TEXCOORD6;
};

struct v2f
{
	float4 pos : SV_POSITION;
	float4 posWorld : TEXCOORD0;
	float3 normal : TEXCOORD1;
	float3 tangent : TEXCOORD2;
	float3 bitangent : TEXCOORD3;
	float2 uv0 : TEXCOORD4;
	float2 uv1 : TEXCOORD5;
	float isOutline : TEXCOORD6;
	fixed4 color : TEXCOORD7;
	SHADOW_COORDS(8)
	UNITY_FOG_COORDS(9)
};

v2f vert_without_geom(appdata_full v)
{
	v2f o;
	o.pos = UnityObjectToClipPos(v.vertex);
	o.posWorld = mul(unity_ObjectToWorld, v.vertex);
	o.uv0 = v.texcoord;
	o.uv1 = v.texcoord1;
	o.normal = normalize(UnityObjectToWorldNormal(v.normal));
	o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangent.xyz, 0.0)).xyz);
	o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangent.w);
	o.isOutline = 0;
	o.color = v.color;
	TRANSFER_SHADOW(o);
	UNITY_TRANSFER_FOG(o, o.pos);
	return o;
}

v2g vert_with_geom(appdata_full v)
{
	v2g o;
	o.posLocal = v.vertex;
	o.normalLocal = v.normal;
	o.tangentLocal = v.tangent;
	o.uv0 = v.texcoord;
	o.uv1 = v.texcoord1;
	o.outlineWidth = 0.01 * _OutlineWidth * tex2Dlod(_OutlineWidthTexture, float4(TRANSFORM_TEX(v.texcoord, _OutlineWidthTexture), 0, 0)).r;
	o.color = v.color;
	return o;
}

[maxvertexcount(6)]
void geom(triangle v2g IN[3], inout TriangleStream<v2f> stream)
{
	v2f o;

#ifdef MTOON_OUTLINE_COLORED
	for (int i = 2; i >= 0; --i)
	{
		v2g v = IN[i];
		o.pos = UnityObjectToClipPos(v.posLocal + normalize(v.normalLocal) * v.outlineWidth);
		o.posWorld = mul(unity_ObjectToWorld, o.pos);
		o.uv0 = v.uv0;
		o.uv1 = v.uv1;
		o.normal = normalize(UnityObjectToWorldNormal(v.normalLocal));
		o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangentLocal.xyz, 0.0)).xyz);
		o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangentLocal.w);
		o.isOutline = 1;
		o.color = 0;
		TRANSFER_SHADOW(o);
		UNITY_TRANSFER_FOG(o, o.pos);
		stream.Append(o);
	}
	stream.RestartStrip();
#endif

	for (int j = 0; j < 3; ++j)
	{
		v2g v = IN[j];
		o.pos = UnityObjectToClipPos(v.posLocal);
		o.posWorld = mul(unity_ObjectToWorld, v.posLocal);
		o.uv0 = v.uv0;
		o.uv1 = v.uv1;
		o.normal = normalize(UnityObjectToWorldNormal(v.normalLocal));
		o.tangent = normalize(mul(unity_ObjectToWorld, float4(v.tangentLocal.xyz, 0.0)).xyz);
		o.bitangent = normalize(cross(o.normal, o.tangent) * v.tangentLocal.w);
		o.isOutline = 0;
		o.color = v.color;
		TRANSFER_SHADOW(o);
		UNITY_TRANSFER_FOG(o, o.pos);
		stream.Append(o);
	}
	stream.RestartStrip();
}

float4 frag(v2f i) : SV_TARGET
{
	float3x3 tangentTransform = float3x3(i.tangent, i.bitangent, i.normal);
	float3 bump = UnpackNormal(tex2D(_NormalTexture, TRANSFORM_TEX(i.uv0, _NormalTexture)));
	float3 normalDir = normalize(mul(bump, tangentTransform));

	float3 vColorBump = i.color.rgb * 2.0 - 1.0;
	normalDir = normalize(lerp(normalDir, normalize(mul(vColorBump, tangentTransform)), _NormalFromVColorRate));

	// normal cylinderize
	// float3 diffPos = normalize(i.posWorld - mul(unity_ObjectToWorld, float4(_NormalCylinderizePos, 1)).xyz);
	// float3 axis = normalize(_NormalCylinderizeAxis);
	// float3 cylinderNormal = normalize(diffPos - dot(diffPos, mul(unity_ObjectToWorld, float4(axis, 0)) * axis));
	// normalDir = normalize(lerp(normalDir, cylinderNormal, _NormalCylinderizeRate));


#ifdef MTOON_DEBUG_NORMAL
	#ifndef _M_FORWARD_ADD
		return float4(normalDir * 0.5 + 0.5, 1);
	#else
		return float4(0, 0, 0, 0);
	#endif
#endif

	float3 view = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float3 viewReflect = reflect(-view, normalDir);
	half nv = dot(normalDir, view);
	UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld.xyz);

	// Receive Shadow Rate
	atten = lerp(1, atten, _ReceiveShadowRate * tex2D(_ReceiveShadowTexture, TRANSFORM_TEX(i.uv0, _ReceiveShadowTexture)).r);

	// lighting
	half3 lightDir = lerp(_WorldSpaceLightPos0.xyz, normalize(_WorldSpaceLightPos0.xyz - i.posWorld.xyz), _WorldSpaceLightPos0.w);
	half3 directLighting = saturate(dot(lightDir, normalDir) * 0.5 + 0.5) * _LightColor0.rgb * atten;
	half3 indirectLighting = ShadeSH9(half4(normalDir, 1));
	half3 rimLighting = tex2D(_SphereAdd, mul(UNITY_MATRIX_V, half4(normalDir, 0)).xy * 0.5 + 0.5);
	half3 lighting = indirectLighting + directLighting + rimLighting;

	// tooned
	half toony = lerp(0.5, 0, _ShadeToony);
	lighting = smoothstep(_ShadeShift - toony, _ShadeShift + toony, lighting);

	// light color attenuation
	half illum = max(lighting.x, max(lighting.y, lighting.z));
	lighting = lerp(lighting, half3(illum, illum, illum), _LightColorAttenuation * illum);

	// color lerp
	half3 shade = _ShadeColor.rgb * tex2D(_ShadeTexture, TRANSFORM_TEX(i.uv0, _ShadeTexture)).rgb;
	half3 lit = _LitColor.rgb * tex2D(_LitTexture, TRANSFORM_TEX(i.uv0, _LitTexture)).rgb;
#ifndef MTOON_FORWARD_ADD
	half3 col = lerp(shade, lit, lighting);
#else
	half3 col = lerp(half3(0,0,0), saturate(lit - shade), lighting);
#endif

	// light strength tint
	half3 tintCol = ShadeSH9(half4(0, 1, 0, 1)) + _LightColor0.rgb;
	half tint = saturate(max(tintCol.r, max(tintCol.g, tintCol.b)));
	col *= tint;

	// outline
	col = lerp(col, _OutlineColor * tint, i.isOutline);

	half4 result = half4(col, _Alpha);
	UNITY_APPLY_FOG(i.fogCoord, result);
	return result;
}