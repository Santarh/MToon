using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

public class MToonInspector : ShaderGUI
{
	public enum DebugMode
	{
		None,
		Normal,
	}

	public enum OutlineMode
	{
		None,
		Colored,
	}

	public enum BlendMode
	{
		Opaque,
		Transparent,
	}

	private MaterialProperty _debugMode;
	private MaterialProperty _outlineMode;
	private MaterialProperty _blendMode;
	private MaterialProperty _alpha;
	private MaterialProperty _color;
	private MaterialProperty _shadeColor;
	private MaterialProperty _mainTex;
	private MaterialProperty _shadeTexture;
	private MaterialProperty _bumpScale;
	private MaterialProperty _bumpMap;
	private MaterialProperty _receiveShadowRate;
	private MaterialProperty _receiveShadowTexture;
	private MaterialProperty _shadeShift;
	private MaterialProperty _shadeToony;
	private MaterialProperty _lightColorAttenuation;
	private MaterialProperty _sphereAdd;
	private MaterialProperty _outlineWidthTexture;
	private MaterialProperty _outlineWidth;
	private MaterialProperty _outlineColor;


	public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
	{
		_debugMode = FindProperty("_DebugMode", properties);
		_outlineMode = FindProperty("_OutlineMode", properties);
		_blendMode = FindProperty("_BlendMode", properties);
		_alpha = FindProperty("_Alpha", properties);
		_color = FindProperty("_Color", properties);
		_shadeColor = FindProperty("_ShadeColor", properties);
		_mainTex = FindProperty("_MainTex", properties);
		_shadeTexture = FindProperty("_ShadeTexture", properties);
		_bumpScale = FindProperty("_BumpScale", properties);
		_bumpMap = FindProperty("_BumpMap", properties);
		_receiveShadowRate = FindProperty("_ReceiveShadowRate", properties);
		_receiveShadowTexture = FindProperty("_ReceiveShadowTexture", properties);
		_shadeShift = FindProperty("_ShadeShift", properties);
		_shadeToony = FindProperty("_ShadeToony", properties);
		_lightColorAttenuation = FindProperty("_LightColorAttenuation", properties);
		_sphereAdd = FindProperty("_SphereAdd", properties);
		_outlineWidthTexture = FindProperty("_OutlineWidthTexture", properties);
		_outlineWidth = FindProperty("_OutlineWidth", properties);
		_outlineColor = FindProperty("_OutlineColor", properties);

		var selfMaterial = materialEditor.target as Material;

		EditorGUI.BeginChangeCheck();
		{
			EditorGUI.showMixedValue = _blendMode.hasMixedValue;
			EditorGUI.BeginChangeCheck();
			var dm = (DebugMode) EditorGUILayout.Popup("DebugType", (int) _debugMode.floatValue, Enum.GetNames(typeof(DebugMode)));
			if (EditorGUI.EndChangeCheck())
			{
				materialEditor.RegisterPropertyChangeUndo("DebugType");
				_debugMode.floatValue = (float) dm;

				foreach (var obj in _debugMode.targets)
				{
					SetupDebugMode((Material) obj, dm);
				}
			}
			EditorGUI.showMixedValue = false;

			EditorGUI.showMixedValue = _blendMode.hasMixedValue;
			EditorGUI.BeginChangeCheck();
			var bm = (BlendMode) EditorGUILayout.Popup("RenderType", (int) _blendMode.floatValue, Enum.GetNames(typeof(BlendMode)));
			if (EditorGUI.EndChangeCheck())
			{
				materialEditor.RegisterPropertyChangeUndo("RenderType");
				_blendMode.floatValue = (float) bm;

				foreach (var obj in _blendMode.targets)
				{
					SetupBlendMode((Material) obj, bm);
				}
			}
			EditorGUI.showMixedValue = false;
			{
				if (bm == BlendMode.Transparent)
				{
					materialEditor.ShaderProperty(_alpha, "Alpha");
				}
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
			{
				// Color
				materialEditor.TexturePropertySingleLine(new GUIContent("Lit Texture", "Lit Texture (RGB)"), _mainTex, _color);
				materialEditor.TexturePropertySingleLine(new GUIContent("Shade Texture", "Shade Texture (RGB)"), _shadeTexture, _shadeColor);
				materialEditor.TexturePropertySingleLine(new GUIContent("Receive Shadow", "Receive Shadow Map (R)"), _receiveShadowTexture, _receiveShadowRate);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
			{
				// Lighting
				materialEditor.ShaderProperty(_shadeShift, "Shade Shift");
				materialEditor.ShaderProperty(_shadeToony, "Shade Toony");
				materialEditor.ShaderProperty(_lightColorAttenuation, "Light Color Attenuation");
				materialEditor.TexturePropertySingleLine(new GUIContent("Sphere Add", "Sphere Additive Texture (RGB)"), _sphereAdd);
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel);
			{
				EditorGUI.showMixedValue = _outlineMode.hasMixedValue;
				EditorGUI.BeginChangeCheck();
				materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), _bumpMap, _bumpScale);
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("BumpEnabledDisabled");
					
					foreach (var obj in _bumpMap.targets)
					{
						var mat = (Material) obj;
						SetupNormalMode(mat, mat.GetTexture(_bumpMap.name));
					}
				}
				EditorGUI.showMixedValue = false;
			}
			EditorGUILayout.Space();

			EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
			{
				// Outline
				EditorGUI.showMixedValue = _outlineMode.hasMixedValue;
				EditorGUI.BeginChangeCheck();
				var om = (OutlineMode) EditorGUILayout.Popup("Outline Mode", (int) _outlineMode.floatValue, Enum.GetNames(typeof(OutlineMode)));
				if (EditorGUI.EndChangeCheck())
				{
					materialEditor.RegisterPropertyChangeUndo("OutlineType");
					_outlineMode.floatValue = (float) om;

					foreach (var obj in _outlineMode.targets)
					{
						SetupOutlineMode((Material) obj, (OutlineMode) selfMaterial.GetFloat("_OutlineMode"));
					}
				}
				EditorGUI.showMixedValue = false;

				if (om != OutlineMode.None)
				{
					materialEditor.TexturePropertySingleLine(new GUIContent("OutlineWidth Tex", "Outline Width Texture (RGB)"), _outlineWidthTexture, _outlineWidth);
					materialEditor.ShaderProperty(_outlineColor, "Outline Color");
				}
			}
			EditorGUILayout.Space();
			
			EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
			{
				EditorGUI.BeginChangeCheck();
				materialEditor.TextureScaleOffsetProperty(_mainTex);
				if (EditorGUI.EndChangeCheck())
				{
					_shadeTexture.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
					_bumpMap.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
					_receiveShadowTexture.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
				}
			}
			EditorGUILayout.Space();
		
            EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
            {
                materialEditor.EnableInstancingField();
                materialEditor.DoubleSidedGIField();
                materialEditor.RenderQueueField();
            }
            EditorGUILayout.Space();
		}
		EditorGUI.EndChangeCheck();
	}

	private void SetupDebugMode(Material material, DebugMode debugMode)
	{
		switch (debugMode)
		{
			case DebugMode.None:
				SetKeyword(material, "MTOON_DEBUG_NORMAL", false);
				break;
			case DebugMode.Normal:
				SetKeyword(material, "MTOON_DEBUG_NORMAL", true);
				break;
		}
	}

	private void SetupBlendMode(Material material, BlendMode blendMode)
	{
		switch (blendMode)
		{
			case BlendMode.Opaque:
				material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", false);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                material.renderQueue = -1;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", true);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
				break;
		}
	}

	private void SetupOutlineMode(Material material, OutlineMode outlineMode)
	{
		switch (outlineMode)
		{
			case OutlineMode.None:
                SetKeyword(material, "MTOON_OUTLINE_COLORED", false);
				break;
			case OutlineMode.Colored:
                SetKeyword(material, "MTOON_OUTLINE_COLORED", true);
				break;
		}
	}

	private void SetupNormalMode(Material material, bool requireNormalMapping)
	{
		SetKeyword(material, "_NORMALMAP", requireNormalMapping);
	}

	private void SetKeyword(Material mat, string keyword, bool required)
	{
		if (required)
		{
			mat.EnableKeyword(keyword);
		}
		else
		{
			mat.DisableKeyword(keyword);
		}
	}
}
