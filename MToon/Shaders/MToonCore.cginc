// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#include "Lighting.cginc"
#include "AutoLight.cginc"

half _Cutoff;
fixed4 _Color;
fixed4 _ShadeColor;
sampler2D _MainTex; float4 _MainTex_ST;
sampler2D _ShadeTexture; float4 _ShadeTexture_ST;
half _BumpScale;
sampler2D _BumpMap; float4 _BumpMap_ST;
sampler2D _ReceiveShadowTexture; float4 _ReceiveShadowTexture_ST;
half _ReceiveShadowRate;
half _ShadeShift;
half _ShadeToony;
half _LightColorAttenuation;
sampler2D _SphereAdd;
fixed4 _EmissionColor;
sampler2D _EmissionMap; float4 _EmissionMap_ST;
sampler2D _OutlineWidthTexture; float4 _OutlineWidthTexture_ST;
half _OutlineWidth;
fixed4 _OutlineColor;
half _OutlineLightingMix;

//UNITY_INSTANCING_BUFFER_START(Props)
//UNITY_INSTANCING_BUFFER_END(Props)

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
	LIGHTING_COORDS(7,8)
	UNITY_FOG_COORDS(9)
	//UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if any instanced properties are going to be accessed in the fragment Shader.
};

inline v2f InitializeV2F(appdata_full v, float3 positionOffset, float isOutline)
{
	v2f o;
	UNITY_SETUP_INSTANCE_ID(v);
	//UNITY_TRANSFER_INSTANCE_ID(v, o);
	
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
	TRANSFER_VERTEX_TO_FRAGMENT(o);
	UNITY_TRANSFER_FOG(o, o.pos);
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
	
	IN[0].normal = normalize(IN[0].normal);
	IN[1].normal = normalize(IN[1].normal);
	IN[2].normal = normalize(IN[2].normal);

#ifdef MTOON_OUTLINE_WIDTH_WORLD
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

float4 frag(v2f i, fixed facing : VFACE) : SV_TARGET
{
	//UNITY_TRANSFER_INSTANCE_ID(v, o);
	
	// main tex
	half4 mainTex = tex2D(_MainTex, TRANSFORM_TEX(i.uv0, _MainTex));
	
    // alpha
    half alpha = 1;
#ifdef _ALPHATEST_ON
    alpha = _Color.a * mainTex.a;
    clip(alpha - _Cutoff);
#endif
#ifdef _ALPHABLEND_ON
    alpha = _Color.a * mainTex.a;
#endif
    
    // normal
#ifdef _NORMALMAP
	half3 tangentNormal = UnpackScaleNormal(tex2D(_BumpMap, TRANSFORM_TEX(i.uv0, _BumpMap)), _BumpScale);
	half3 worldNormal;
	worldNormal.x = dot(i.tspace0, tangentNormal);
	worldNormal.y = dot(i.tspace1, tangentNormal);
	worldNormal.z = dot(i.tspace2, tangentNormal);
#else
    half3 worldNormal = half3(i.tspace0.z, i.tspace1.z, i.tspace2.z);
#endif
    worldNormal *= facing;
    worldNormal = normalize(worldNormal);

#ifdef MTOON_DEBUG_NORMAL
	#ifdef MTOON_FORWARD_ADD
		return float4(0, 0, 0, 0);
	#else
		return float4(worldNormal * 0.5 + 0.5, 1);
	#endif
#endif

	// information for lighting
	half3 lightDir = lerp(_WorldSpaceLightPos0.xyz, normalize(_WorldSpaceLightPos0.xyz - i.posWorld.xyz), _WorldSpaceLightPos0.w);
	float atten = LIGHT_ATTENUATION(i);
	half receiveShadow = 1 - _ReceiveShadowRate * tex2D(_ReceiveShadowTexture, TRANSFORM_TEX(i.uv0, _ReceiveShadowTexture)).a;
#ifdef MTOON_FORWARD_ADD
    // FIXME atten is distance function when tranparent && point light
    half shadow = atten;
#else
	half shadow = max(atten, receiveShadow);
#endif

	// ambient
	half3 indirect = ShadeSH9(half4(worldNormal, 1));
	half indirectLighting = max(0.001, max(indirect.x, max(indirect.y, indirect.z)));
	half3 indirectColor = indirect;
	
	// direct lighting
	half directLighting = dot(lightDir, worldNormal); // neutral
	directLighting = lerp(0, directLighting, shadow); // receive shadow
	directLighting = smoothstep(_ShadeShift, _ShadeShift + (1.0 - _ShadeToony), directLighting); // shade & tooned
	
	// brightness
	half brightness = directLighting + indirectLighting;
	brightness = smoothstep(_ShadeShift, _ShadeShift + (1.0 - _ShadeToony), brightness); // shade & tooned
	brightness = lerp(0, brightness, shadow); // receive shadow
	
	// colored
	half3 colorShift = lerp(indirectColor, _LightColor0.rgb, saturate(directLighting / indirectLighting));
	half colorShiftBrightness = max(0.001, max(colorShift.x, max(colorShift.y, colorShift.z)));
	half3 lighting = brightness * lerp(colorShift, colorShiftBrightness.xxx, _LightColorAttenuation); // color atten
	
	// color lerp
	half4 shade = _ShadeColor * tex2D(_ShadeTexture, TRANSFORM_TEX(i.uv0, _ShadeTexture));
	half4 lit = _Color * mainTex;
#ifdef MTOON_FORWARD_ADD
	half3 col = lerp(half3(0,0,0), saturate(lit.rgb - shade.rgb), lighting);
#else
	half3 col = lerp(shade.rgb, lit.rgb, lighting);
#endif

    // rim
#ifdef MTOON_FORWARD_ADD
#else
	half3 worldCameraUp = normalize(UNITY_MATRIX_V[1].xyz);
	half3 worldView = normalize(_WorldSpaceCameraPos.xyz - i.posWorld.xyz);
	half3 worldViewUp = normalize(worldCameraUp - worldView * dot(worldView, worldCameraUp));
	half3 worldViewRight = normalize(cross(worldView, worldViewUp));
	half2 rimUv = half2(dot(worldViewRight, worldNormal), dot(worldViewUp, worldNormal)) * 0.5 + 0.5;
	half3 rimLighting = tex2D(_SphereAdd, rimUv);
	col += lerp(rimLighting, half3(0, 0, 0), i.isOutline);
#endif

	// energy conservation
	half3 energy = ShadeSH9(half4(0, 1, 0, 1)) + _LightColor0.rgb;
	half energyV = max(0.001, max(energy.r, max(energy.g, energy.b)));
	half colV = max(0.001, max(col.r, max(col.g, col.b)));
	half tint = min(energyV, colV) / colV;
	col *= tint;
	
	// Emission
#ifdef MTOON_FORWARD_ADD
#else
	half3 emission = tex2D(_EmissionMap, TRANSFORM_TEX(i.uv0, _EmissionMap)).rgb * _EmissionColor.rgb;
	col += lerp(emission, half3(0, 0, 0), i.isOutline);
#endif

	// outline
#ifdef MTOON_OUTLINE_COLOR_FIXED
	col = lerp(col, _OutlineColor, i.isOutline);
#elif MTOON_OUTLINE_COLOR_MIXED
	col = lerp(col, _OutlineColor * lerp(tint.xxx, col, _OutlineLightingMix), i.isOutline);
#else
#endif

	half4 result = half4(col, alpha);
	UNITY_APPLY_FOG(i.fogCoord, result);
	return result;
}