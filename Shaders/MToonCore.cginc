// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#include "Lighting.cginc"
#include "AutoLight.cginc"
#pragma target 3.0

half _Alpha;
fixed4 _Color;
fixed4 _ShadeColor;
sampler2D _MainTex; float4 _MainTex_ST;
sampler2D _ShadeTexture; float4 _ShadeTexture_ST;
sampler2D _BumpMap; float4 _BumpMap_ST;
sampler2D _ReceiveShadowTexture; float4 _ReceiveShadowTexture_ST;
half _ReceiveShadowRate;
half _ShadeShift;
half _ShadeToony;
half _LightColorAttenuation;
sampler2D _SphereAdd;
sampler2D _OutlineWidthTexture; float4 _OutlineWidthTexture_ST;
half _OutlineWidth;
fixed4 _OutlineColor;

UNITY_INSTANCING_BUFFER_START(Props)
UNITY_INSTANCING_BUFFER_END(Props)

struct v2f
{
	float4 pos : SV_POSITION;
	float4 posWorld : TEXCOORD0;
	half3 tspace0 : TEXCOORD1;
	half3 tspace1 : TEXCOORD2;
	half3 tspace2 : TEXCOORD3;
	float2 uv0 : TEXCOORD4;
	float isOutline : TEXCOORD5;
	fixed4 color : TEXCOORD6;
	SHADOW_COORDS(7)
	UNITY_FOG_COORDS(8)
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

inline v2f InitializeV2F(appdata_full v, float3 positionOffset, float isOutline)
{
	v2f o;
	float4 vertex = v.vertex + float4(positionOffset, 0);
	o.pos = UnityObjectToClipPos(vertex);
	o.posWorld = mul(unity_ObjectToWorld, vertex);
	o.uv0 = v.texcoord;
	half3 worldNormal = UnityObjectToWorldNormal(v.normal);
	half3 worldTangent = UnityObjectToWorldDir(v.tangent);
	half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	half3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
	o.tspace0 = half3(worldTangent.x, worldBitangent.x, worldNormal.x);
	o.tspace1 = half3(worldTangent.y, worldBitangent.y, worldNormal.y);
	o.tspace2 = half3(worldTangent.z, worldBitangent.z, worldNormal.z);
	o.isOutline = isOutline;
	o.color = v.color;
	TRANSFER_SHADOW(o);
	UNITY_TRANSFER_FOG(o, o.pos);
	UNITY_SETUP_INSTANCE_ID(v);
	UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if any instanced properties are going to be accessed in the fragment Shader.
	return o;
}

v2f vert_without_geom(appdata_full v)
{
    return InitializeV2F(v, float3(0, 0, 0), 0);
}

appdata_full vert_with_geom(appdata_full v)
{
    return v;
}

[maxvertexcount(6)]
void geom(triangle appdata_full IN[3], inout TriangleStream<v2f> stream)
{
	v2f o;

#ifdef MTOON_OUTLINE_COLORED
	for (int i = 2; i >= 0; --i)
	{
		appdata_full v = IN[i];
		float outlineTex = tex2Dlod(_OutlineWidthTexture, float4(TRANSFORM_TEX(v.texcoord, _OutlineWidthTexture), 0, 0)).r;
        float3 outlineOffset = 0.01 * _OutlineWidth * outlineTex * v.normal;
        v2f o = InitializeV2F(v, outlineOffset, 1);
		stream.Append(o);
	}
	stream.RestartStrip();
#endif

	for (int j = 0; j < 3; ++j)
	{
		appdata_full v = IN[j];
		v2f o = InitializeV2F(v, float3(0, 0, 0), 0);
		stream.Append(o);
	}
	stream.RestartStrip();
}

float4 frag(v2f i) : SV_TARGET
{
    UNITY_SETUP_INSTANCE_ID(i); // necessary only if any instanced properties are going to be accessed in the fragment Shader.
    
	half3 tangentNormal = UnpackNormal(tex2D(_BumpMap, TRANSFORM_TEX(i.uv0, _BumpMap)));
	half3 worldNormal;
	worldNormal.x = dot(i.tspace0, tangentNormal);
	worldNormal.y = dot(i.tspace1, tangentNormal);
	worldNormal.z = dot(i.tspace2, tangentNormal);

#ifdef MTOON_DEBUG_NORMAL
	#ifndef _M_FORWARD_ADD
		return float4(worldNormal * 0.5 + 0.5, 1);
	#else
		return float4(0, 0, 0, 0);
	#endif
#endif

	float3 view = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	float3 viewReflect = reflect(-view, worldNormal);
	half nv = dot(worldNormal, view);
	UNITY_LIGHT_ATTENUATION(atten, i, i.posWorld.xyz);

	// Receive Shadow Rate
	atten = lerp(1, atten, _ReceiveShadowRate * tex2D(_ReceiveShadowTexture, TRANSFORM_TEX(i.uv0, _ReceiveShadowTexture)).r);

	// lighting
	half3 lightDir = lerp(_WorldSpaceLightPos0.xyz, normalize(_WorldSpaceLightPos0.xyz - i.posWorld.xyz), _WorldSpaceLightPos0.w);
	half3 directLighting = saturate(dot(lightDir, worldNormal) * 0.5 + 0.5) * _LightColor0.rgb * atten;
	half3 indirectLighting = ShadeSH9(half4(worldNormal, 1));
	half3 rimLighting = tex2D(_SphereAdd, mul(UNITY_MATRIX_V, half4(worldNormal, 0)).xy * 0.5 + 0.5);
	half3 lighting = indirectLighting + directLighting + rimLighting;

	// tooned
	half toony = lerp(0.5, 0, _ShadeToony);
	lighting = smoothstep(_ShadeShift - toony, _ShadeShift + toony, lighting);

	// light color attenuation
	half illum = max(lighting.x, max(lighting.y, lighting.z));
	lighting = lerp(lighting, half3(illum, illum, illum), _LightColorAttenuation * illum);

	// color lerp
	half3 shade = _ShadeColor.rgb * tex2D(_ShadeTexture, TRANSFORM_TEX(i.uv0, _ShadeTexture)).rgb;
	half3 lit = _Color.rgb * tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex)).rgb;
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