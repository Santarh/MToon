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
	private MaterialProperty _litColor;
	private MaterialProperty _shadeColor;
	private MaterialProperty _litTexture;
	private MaterialProperty _shadeTexture;
	private MaterialProperty _normalTexture;
	private MaterialProperty _receiveShadowRate;
	private MaterialProperty _receiveShadowTexture;
	private MaterialProperty _shadeShift;
	private MaterialProperty _shadeToony;
	private MaterialProperty _lightColorAttenuation;
	private MaterialProperty _normalFromVColorRate;
	private MaterialProperty _normalCylinderizeRate;
	private MaterialProperty _normalCylinderizePos;
	private MaterialProperty _normalCylinderizeAxis;
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
		_litColor = FindProperty("_LitColor", properties);
		_shadeColor = FindProperty("_ShadeColor", properties);
		_litTexture = FindProperty("_LitTexture", properties);
		_shadeTexture = FindProperty("_ShadeTexture", properties);
		_normalTexture = FindProperty("_NormalTexture", properties);
		_receiveShadowRate = FindProperty("_ReceiveShadowRate", properties);
		_receiveShadowTexture = FindProperty("_ReceiveShadowTexture", properties);
		_shadeShift = FindProperty("_ShadeShift", properties);
		_shadeToony = FindProperty("_ShadeToony", properties);
		_lightColorAttenuation = FindProperty("_LightColorAttenuation", properties);
		_normalFromVColorRate = FindProperty("_NormalFromVColorRate", properties);
		_normalCylinderizeRate = FindProperty("_NormalCylinderizeRate", properties);
		_normalCylinderizePos = FindProperty("_NormalCylinderizePos", properties);
		_normalCylinderizeAxis = FindProperty("_NormalCylinderizeAxis", properties);
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

			EditorGUI.indentLevel++;
			{
				if (bm == BlendMode.Transparent)
				{
					materialEditor.ShaderProperty(_alpha, "Alpha");
				}
			}
			EditorGUI.indentLevel--;

			EditorGUILayout.LabelField("Color");
			EditorGUI.indentLevel++;
			{
				// Color
				materialEditor.TexturePropertySingleLine(new GUIContent("Lit Texture", "Lit Texture (RGB)"), _litTexture, _litColor);
				materialEditor.TexturePropertySingleLine(new GUIContent("Shade Texture", "Shade Texture (RGB)"), _shadeTexture, _shadeColor);
				materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), _normalTexture);
				materialEditor.TexturePropertySingleLine(new GUIContent("Receive Shadow", "Receive Shadow Map (R)"), _receiveShadowTexture, _receiveShadowRate);

				EditorGUI.BeginChangeCheck();
				materialEditor.TextureScaleOffsetProperty(_litTexture);
				if (EditorGUI.EndChangeCheck())
				{
					_shadeTexture.textureScaleAndOffset = _litTexture.textureScaleAndOffset;
					_normalTexture.textureScaleAndOffset = _litTexture.textureScaleAndOffset;
					_receiveShadowTexture.textureScaleAndOffset = _litTexture.textureScaleAndOffset;
				}
			}
			EditorGUI.indentLevel --;

			EditorGUILayout.LabelField("Lighting");
			EditorGUI.indentLevel++;
			{
				// Lighting
				materialEditor.ShaderProperty(_shadeShift, "Shade Shift");
				materialEditor.ShaderProperty(_shadeToony, "Shade Toony");
				materialEditor.ShaderProperty(_lightColorAttenuation, "Light Color Attenuation");
				materialEditor.TexturePropertySingleLine(new GUIContent("Sphere Add", "Sphere Additive Texture (RGB)"), _sphereAdd);
			}
			EditorGUI.indentLevel --;

			EditorGUILayout.LabelField("Normal");
			EditorGUI.indentLevel++;
			{
				materialEditor.ShaderProperty(_normalFromVColorRate, "Normal from Vertex Color Rate");
				materialEditor.ShaderProperty(_normalCylinderizeRate, "Normal Cylinderize Rate");
				materialEditor.ShaderProperty(_normalCylinderizePos, "Normal Cylinderize Pos");
				materialEditor.ShaderProperty(_normalCylinderizeAxis, "Normal Cylinderize Axis");
			}
			EditorGUI.indentLevel --;

			EditorGUILayout.LabelField("Outline");
			EditorGUI.indentLevel++;
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
			EditorGUI.indentLevel--;
		}
		EditorGUI.EndChangeCheck();
	}

	private void SetupDebugMode(Material material, DebugMode debugMode)
	{
		switch (debugMode)
		{
			case DebugMode.None:
				material.EnableKeyword("MTOON_DEBUG_NONE");
				material.DisableKeyword("MTOON_DEBUG_NORMAL");
				break;
			case DebugMode.Normal:
				material.DisableKeyword("MTOON_DEBUG_NONE");
				material.EnableKeyword("MTOON_DEBUG_NORMAL");
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
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = -1;
				break;
			case BlendMode.Transparent:
				material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int) UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int) UnityEngine.Rendering.RenderQueue.Transparent;
				break;
		}
	}

	private void SetupOutlineMode(Material material, OutlineMode outlineMode)
	{
		switch (outlineMode)
		{
			case OutlineMode.None:
				material.EnableKeyword("MTOON_OUTLINE_NONE");
				material.DisableKeyword("MTOON_OUTLINE_COLORED");
				break;
			case OutlineMode.Colored:
				material.DisableKeyword("MTOON_OUTLINE_NONE");
				material.EnableKeyword("MTOON_OUTLINE_COLORED");
				break;
		}
	}
}
