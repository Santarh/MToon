using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MToonInspector : ShaderGUI
{
    public enum DebugMode
    {
        None,
        Normal,
        LitShadeRate,
    }

    public enum OutlineColorMode
    {
        FixedColor,
        MixedLighting
    }

    public enum OutlineWidthMode
    {
        None,
        WorldCoordinates,
        ScreenCoordinates
    }

    public enum RenderMode
    {
        Opaque,
        Cutout,
        Transparent,
        TransparentWithZWrite,
    }

    private MaterialProperty _blendMode;
    private MaterialProperty _bumpMap;
    private MaterialProperty _bumpScale;
    private MaterialProperty _color;
    private MaterialProperty _cullMode;
    private MaterialProperty _outlineCullMode;
    private MaterialProperty _cutoff;

    private MaterialProperty _debugMode;
    private MaterialProperty _emissionColor;
    private MaterialProperty _emissionMap;
    private MaterialProperty _lightColorAttenuation;
    private MaterialProperty _indirectLightIntensity;
    private MaterialProperty _mainTex;
    private MaterialProperty _outlineColor;
    private MaterialProperty _outlineColorMode;
    private MaterialProperty _outlineLightingMix;
    private MaterialProperty _outlineWidth;
    private MaterialProperty _outlineScaledMaxDistance;
    private MaterialProperty _outlineWidthMode;
    private MaterialProperty _outlineWidthTexture;
    private MaterialProperty _receiveShadowRate;
    private MaterialProperty _receiveShadowTexture;
    private MaterialProperty _shadingGradeRate;
    private MaterialProperty _shadingGradeTexture;
    private MaterialProperty _shadeColor;
    private MaterialProperty _shadeShift;
    private MaterialProperty _shadeTexture;
    private MaterialProperty _shadeToony;
    private MaterialProperty _sphereAdd;

    public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
    {
        _debugMode = FindProperty("_DebugMode", properties);
        _outlineWidthMode = FindProperty("_OutlineWidthMode", properties);
        _outlineColorMode = FindProperty("_OutlineColorMode", properties);
        _blendMode = FindProperty("_BlendMode", properties);
        _cullMode = FindProperty("_CullMode", properties);
        _outlineCullMode = FindProperty("_OutlineCullMode", properties);
        _cutoff = FindProperty("_Cutoff", properties);
        _color = FindProperty("_Color", properties);
        _shadeColor = FindProperty("_ShadeColor", properties);
        _mainTex = FindProperty("_MainTex", properties);
        _shadeTexture = FindProperty("_ShadeTexture", properties);
        _bumpScale = FindProperty("_BumpScale", properties);
        _bumpMap = FindProperty("_BumpMap", properties);
        _receiveShadowRate = FindProperty("_ReceiveShadowRate", properties);
        _receiveShadowTexture = FindProperty("_ReceiveShadowTexture", properties);
        _shadingGradeRate = FindProperty("_ShadingGradeRate", properties);
        _shadingGradeTexture = FindProperty("_ShadingGradeTexture", properties);
        _shadeShift = FindProperty("_ShadeShift", properties);
        _shadeToony = FindProperty("_ShadeToony", properties);
        _lightColorAttenuation = FindProperty("_LightColorAttenuation", properties);
        _indirectLightIntensity = FindProperty("_IndirectLightIntensity", properties);
        _sphereAdd = FindProperty("_SphereAdd", properties);
        _emissionColor = FindProperty("_EmissionColor", properties);
        _emissionMap = FindProperty("_EmissionMap", properties);
        _outlineWidthTexture = FindProperty("_OutlineWidthTexture", properties);
        _outlineWidth = FindProperty("_OutlineWidth", properties);
        _outlineScaledMaxDistance = FindProperty("_OutlineScaledMaxDistance", properties);
        _outlineColor = FindProperty("_OutlineColor", properties);
        _outlineLightingMix = FindProperty("_OutlineLightingMix", properties);

        var uvMappedTextureProperties = new[]
        {
            _mainTex,
            _shadeTexture,
            _bumpMap,
            _receiveShadowTexture,
            _shadingGradeTexture,
            _emissionMap,
            _outlineWidthTexture,
        };

        var materials = materialEditor.targets.Select(x => x as Material).ToArray();
        Draw(materialEditor, materials, uvMappedTextureProperties);
    }

    private void Draw(MaterialEditor materialEditor, Material[] materials, MaterialProperty[] uvMappedTextureProperties)
    {
        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.LabelField("Rendering", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
                if (PopupEnum<RenderMode>("Rendering Type", _blendMode, materialEditor))
                {
                    ModeChanged(materials, isBlendModeChangedByUser: true);
                }
                if (PopupEnum<CullMode>("Cull Mode", _cullMode, materialEditor))
                {
                    ModeChanged(materials);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Texture", EditorStyles.boldLabel);
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Lit & Alpha", "Lit (RGB), Alpha (A)"),
                        _mainTex, _color);
                    
                    materialEditor.TexturePropertySingleLine(new GUIContent("Shade", "Shade (RGB)"), _shadeTexture,
                        _shadeColor);
                }
                var bm = (RenderMode) _blendMode.floatValue;
                if (bm != RenderMode.Opaque)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Alpha", EditorStyles.boldLabel);
                    {
                        if (bm == RenderMode.Cutout)
                        {
                            materialEditor.ShaderProperty(_cutoff, "Cutoff");
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Lit & Shade Mixing", EditorStyles.boldLabel);
                {
                    materialEditor.ShaderProperty(_shadeShift,
                        new GUIContent("Shading Shift",
                            "Zero is Default. Negative value increase lit area. Positive value increase shade area."));
                    materialEditor.ShaderProperty(_shadeToony,
                        new GUIContent("Shading Toony",
                            "0.0 is Lambert. Higher value get toony shading."));
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Shadow Receive Multiplier",
                            "Texture (A) * Rate. White is Default. Black attenuates shadows."),
                        _receiveShadowTexture,
                        _receiveShadowRate);
                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Lit & Shade Mixing Multiplier",
                            "Texture (R) * Rate. Compatible with UTS2 ShadingGradeMap. White is Default. Black amplifies shade."),
                        _shadingGradeTexture,
                        _shadingGradeRate);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Light Color", EditorStyles.boldLabel);
                {
                    materialEditor.ShaderProperty(_lightColorAttenuation, "LightColor Attenuation");
                    materialEditor.ShaderProperty(_indirectLightIntensity, "GI Intensity");
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Emission", "Emission (RGB)"),
                        _emissionMap,
                        _emissionColor);
                    materialEditor.TexturePropertySingleLine(new GUIContent("MatCap", "MatCap Texture (RGB)"),
                        _sphereAdd);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel);
                {
                    // Normal
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"),
                        _bumpMap,
                        _bumpScale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("BumpEnabledDisabled");
                        ModeChanged(materials);
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                // Outline
                EditorGUILayout.LabelField("Width", EditorStyles.boldLabel);
                {
                    if (PopupEnum<OutlineWidthMode>("Mode", _outlineWidthMode, materialEditor))
                    {
                        ModeChanged(materials);
                    }
                    var widthMode = (OutlineWidthMode) _outlineWidthMode.floatValue;
                    if (widthMode != OutlineWidthMode.None)
                    {
                        materialEditor.TexturePropertySingleLine(
                            new GUIContent("Width", "Outline Width Texture (RGB)"),
                            _outlineWidthTexture, _outlineWidth);
                    }

                    if (widthMode == OutlineWidthMode.ScreenCoordinates)
                    {
                        materialEditor.ShaderProperty(_outlineScaledMaxDistance, "Width Scaled Max Distance");
                    }
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                {
                    var widthMode = (OutlineWidthMode) _outlineWidthMode.floatValue;
                    if (widthMode != OutlineWidthMode.None)
                    {
                        EditorGUI.BeginChangeCheck();

                        if (PopupEnum<OutlineColorMode>("Mode", _outlineColorMode, materialEditor))
                        {
                            ModeChanged(materials);
                        }
                        var colorMode = (OutlineColorMode) _outlineColorMode.floatValue;

                        materialEditor.ShaderProperty(_outlineColor, "Color");
                        if (colorMode == OutlineColorMode.MixedLighting)
                            materialEditor.DefaultShaderProperty(_outlineLightingMix, "Lighting Mix");
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Options", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Texture Options", EditorStyles.boldLabel);
                {
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TextureScaleOffsetProperty(_mainTex);
                    if (EditorGUI.EndChangeCheck())
                        foreach (var textureProperty in uvMappedTextureProperties)
                            textureProperty.textureScaleAndOffset = _mainTex.textureScaleAndOffset;
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Debugging Options", EditorStyles.boldLabel);
                {
                    if (PopupEnum<DebugMode>("Visualize", _debugMode, materialEditor))
                    {
                        ModeChanged(materials);
                    }
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
                {
                #if UNITY_5_6_OR_NEWER
//                    materialEditor.EnableInstancingField();
                    materialEditor.DoubleSidedGIField();
                #endif
                    materialEditor.RenderQueueField();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        EditorGUI.EndChangeCheck();
    }

    public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
    {
        base.AssignNewShaderToMaterial(material, oldShader, newShader);

        ModeChanged(material, isBlendModeChangedByUser: true);
    }

    private static void ModeChanged(Material[] materials, bool isBlendModeChangedByUser = false)
    {
        foreach (var material in materials)
        {
            ModeChanged(material, isBlendModeChangedByUser);
        }
    }
    
    private static void ModeChanged(Material material, bool isBlendModeChangedByUser = false)
    {
        SetupBlendMode(material, (RenderMode) material.GetFloat("_BlendMode"), isBlendModeChangedByUser);
        SetupNormalMode(material, material.GetTexture("_BumpMap"));
        SetupOutlineMode(material,
            (OutlineWidthMode) material.GetFloat("_OutlineWidthMode"),
            (OutlineColorMode) material.GetFloat("_OutlineColorMode"));
        SetupDebugMode(material, (DebugMode) material.GetFloat("_DebugMode"));
        SetupCullMode(material, (CullMode) material.GetFloat("_CullMode"));
        
        var mainTex = material.GetTexture("_MainTex");
        var shadeTex = material.GetTexture("_ShadeTexture");
        if (mainTex != null && shadeTex == null)
        {
            material.SetTexture("_ShadeTexture", mainTex);
        }
    }

    private static bool PopupEnum<T>(string name, MaterialProperty property, MaterialEditor editor) where T : struct
    {
        EditorGUI.showMixedValue = property.hasMixedValue;
        EditorGUI.BeginChangeCheck();
        var ret = EditorGUILayout.Popup(name, (int) property.floatValue, Enum.GetNames(typeof(T)));
        var changed = EditorGUI.EndChangeCheck();
        if (changed)
        {
            editor.RegisterPropertyChangeUndo("EnumPopUp");
            property.floatValue = ret;
        }
        EditorGUI.showMixedValue = false;
        return changed;
    }

    private static void SetupDebugMode(Material material, DebugMode debugMode)
    {
        switch (debugMode)
        {
            case DebugMode.None:
                SetKeyword(material, "MTOON_DEBUG_NORMAL", false);
                SetKeyword(material, "MTOON_DEBUG_LITSHADERATE", false);
                break;
            case DebugMode.Normal:
                SetKeyword(material, "MTOON_DEBUG_NORMAL", true);
                SetKeyword(material, "MTOON_DEBUG_LITSHADERATE", false);
                break;
            case DebugMode.LitShadeRate:
                SetKeyword(material, "MTOON_DEBUG_NORMAL", false);
                SetKeyword(material, "MTOON_DEBUG_LITSHADERATE", true);
                break;
        }
    }

    private static void SetupBlendMode(Material material, RenderMode renderMode, bool isChangedByUser)
    {
        switch (renderMode)
        {
            case RenderMode.Opaque:
                material.SetOverrideTag("RenderType", "Opaque");
                material.SetInt("_SrcBlend", (int) BlendMode.One);
                material.SetInt("_DstBlend", (int) BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", false);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                if (isChangedByUser)
                {
                    material.renderQueue = -1;
                }
                break;
            case RenderMode.Cutout:
                material.SetOverrideTag("RenderType", "TransparentCutout");
                material.SetInt("_SrcBlend", (int) BlendMode.One);
                material.SetInt("_DstBlend", (int) BlendMode.Zero);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", true);
                SetKeyword(material, "_ALPHABLEND_ON", false);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                if (isChangedByUser)
                {
                    material.renderQueue = (int) RenderQueue.AlphaTest;
                }
                break;
            case RenderMode.Transparent:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", true);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                if (isChangedByUser)
                {
                    material.renderQueue = (int) RenderQueue.Transparent;
                }
                break;
            case RenderMode.TransparentWithZWrite:
                material.SetOverrideTag("RenderType", "Transparent");
                material.SetInt("_SrcBlend", (int) BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int) BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 1);
                SetKeyword(material, "_ALPHATEST_ON", false);
                SetKeyword(material, "_ALPHABLEND_ON", true);
                SetKeyword(material, "_ALPHAPREMULTIPLY_ON", false);
                if (isChangedByUser)
                {
                    material.renderQueue = (int) RenderQueue.AlphaTest + 50;
                }
                break;
        }
    }

    private static void SetupOutlineMode(Material material, OutlineWidthMode outlineWidthMode,
        OutlineColorMode outlineColorMode)
    {
        switch (outlineWidthMode)
        {
            case OutlineWidthMode.None:
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_WORLD", false);
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_SCREEN", false);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_FIXED", false);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_MIXED", false);
                break;
            case OutlineWidthMode.WorldCoordinates:
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_WORLD", true);
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_SCREEN", false);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_FIXED", outlineColorMode == OutlineColorMode.FixedColor);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_MIXED", outlineColorMode == OutlineColorMode.MixedLighting);
                break;
            case OutlineWidthMode.ScreenCoordinates:
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_WORLD", false);
                SetKeyword(material, "MTOON_OUTLINE_WIDTH_SCREEN", true);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_FIXED", outlineColorMode == OutlineColorMode.FixedColor);
                SetKeyword(material, "MTOON_OUTLINE_COLOR_MIXED", outlineColorMode == OutlineColorMode.MixedLighting);
                break;
        }
    }

    private static void SetupNormalMode(Material material, bool requireNormalMapping)
    {
        SetKeyword(material, "_NORMALMAP", requireNormalMapping);
    }

    private static void SetupCullMode(Material material, CullMode cullMode)
    {
        switch (cullMode)
        {
            case CullMode.Back:
                material.SetInt("_CullMode", (int) CullMode.Back);
                material.SetInt("_OutlineCullMode", (int) CullMode.Front);
                break;
            case CullMode.Front:
                material.SetInt("_CullMode", (int) CullMode.Front);
                material.SetInt("_OutlineCullMode", (int) CullMode.Back);
                break;
            case CullMode.Off:
                material.SetInt("_CullMode", (int) CullMode.Off);
                material.SetInt("_OutlineCullMode", (int) CullMode.Front);
                break;
        }
    }

    private static void SetKeyword(Material mat, string keyword, bool required)
    {
        if (required)
            mat.EnableKeyword(keyword);
        else
            mat.DisableKeyword(keyword);
    }
}