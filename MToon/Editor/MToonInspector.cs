using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class MToonInspector : ShaderGUI
{
    public enum DebugMode
    {
        None,
        Normal
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
        Transparent
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
    private MaterialProperty _isFirstSetup;
    private MaterialProperty _lightColorAttenuation;
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
        _shadeShift = FindProperty("_ShadeShift", properties);
        _shadeToony = FindProperty("_ShadeToony", properties);
        _lightColorAttenuation = FindProperty("_LightColorAttenuation", properties);
        _sphereAdd = FindProperty("_SphereAdd", properties);
        _emissionColor = FindProperty("_EmissionColor", properties);
        _emissionMap = FindProperty("_EmissionMap", properties);
        _outlineWidthTexture = FindProperty("_OutlineWidthTexture", properties);
        _outlineWidth = FindProperty("_OutlineWidth", properties);
        _outlineScaledMaxDistance = FindProperty("_OutlineScaledMaxDistance", properties);
        _outlineColor = FindProperty("_OutlineColor", properties);
        _outlineLightingMix = FindProperty("_OutlineLightingMix", properties);
        _isFirstSetup = FindProperty("_IsFirstSetup", properties);

        var uvMappedTextureProperties = new[]
        {
            _mainTex,
            _shadeTexture,
            _bumpMap,
            _receiveShadowTexture,
            _emissionMap,
            _outlineWidthTexture
        };

        foreach (var obj in materialEditor.targets)
        {
            var mat = (Material) obj;
            var isFirstSetup = mat.GetFloat(_isFirstSetup.name);
            if (isFirstSetup < 0.5f) continue;
            
            mat.SetFloat(_isFirstSetup.name, 0.0f);
            var mainTex = mat.GetTexture(_mainTex.name);
            var shadeTex = mat.GetTexture(_shadeTexture.name);
            if (mainTex != null && shadeTex == null)
            {
                mat.SetTexture(_shadeTexture.name, mainTex);
            }
        }

        foreach (var obj in materialEditor.targets)
        {
            var mat = (Material) obj;
            SetupBlendMode(mat, (RenderMode) mat.GetFloat(_blendMode.name), setRenderQueueAsDefault: false);
            SetupNormalMode(mat, mat.GetTexture(_bumpMap.name));
            SetupOutlineMode(mat,
                (OutlineWidthMode) mat.GetFloat(_outlineWidthMode.name),
                (OutlineColorMode) mat.GetFloat(_outlineColorMode.name));
            SetupDebugMode(mat, (DebugMode) mat.GetFloat(_debugMode.name));
            SetupCullMode(mat, (CullMode) mat.GetFloat(_cullMode.name));
        }


        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.LabelField("Basic", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Mode", EditorStyles.boldLabel);
                var bm = (RenderMode) _blendMode.floatValue;
                if (PopupEnum<RenderMode>("Rendering Type", _blendMode, materialEditor))
                {
                    bm = (RenderMode) _blendMode.floatValue;
                    foreach (var obj in materialEditor.targets)
                    {
                        SetupBlendMode((Material) obj, bm, setRenderQueueAsDefault: true);
                    }
                }

                EditorGUI.showMixedValue = false;

                if (PopupEnum<CullMode>("Cull Mode", _cullMode, materialEditor))
                {
                    var cm = (CullMode) _cullMode.floatValue;
                    foreach (var obj in materialEditor.targets) SetupCullMode((Material) obj, cm);
                }

                EditorGUI.showMixedValue = false;
                EditorGUILayout.Space();

                if (bm != RenderMode.Opaque)
                {
                    EditorGUILayout.LabelField("Alpha", EditorStyles.boldLabel);
                    {
                        if (bm == RenderMode.Transparent)
                            EditorGUILayout.TextField("Ensure your lit color and texture have alpha channels.");

                        if (bm == RenderMode.Cutout)
                        {
                            EditorGUILayout.TextField("Ensure your lit color and texture have alpha channels.");
                            materialEditor.ShaderProperty(_cutoff, "Cutoff");
                        }
                    }
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                {
                    // Color
                    materialEditor.TexturePropertySingleLine(new GUIContent("Lit & Alpha", "Lit (RGB), Alpha (A)"),
                        _mainTex, _color);
                    materialEditor.TexturePropertySingleLine(new GUIContent("Shade", "Shade (RGB)"), _shadeTexture,
                        _shadeColor);
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Lighting", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Shade", EditorStyles.boldLabel);
                {
                    // Shade
                    materialEditor.ShaderProperty(_shadeShift, "Shift");
                    materialEditor.ShaderProperty(_shadeToony, "Toony");
                    materialEditor.ShaderProperty(_lightColorAttenuation, "LightColor Attenuation");
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Shadow", EditorStyles.boldLabel);
                {
                    // Shadow
                    if (((Material) materialEditor.target).GetFloat("_ShadeShift") < 0f)
                        EditorGUILayout.LabelField(
                            "Receive rate should be lower value when Shade Shift is lower than 0.",
                            EditorStyles.wordWrappedLabel);

                    materialEditor.TexturePropertySingleLine(
                        new GUIContent("Receive Rate", "Receive Shadow Rate Map (A)"),
                        _receiveShadowTexture, _receiveShadowRate);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("MatCap", EditorStyles.boldLabel);
                {
                    // MatCap Light
                    materialEditor.TexturePropertySingleLine(new GUIContent("Additive", "Additive MatCap Texture (RGB)"),
                        _sphereAdd);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Emission", EditorStyles.boldLabel);
                {
                    materialEditor.TexturePropertySingleLine(new GUIContent("Emission", "Emission (RGB)"), _emissionMap,
                        _emissionColor);
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Normal", EditorStyles.boldLabel);
                {
                    // Normal
                    EditorGUI.BeginChangeCheck();
                    materialEditor.TexturePropertySingleLine(new GUIContent("Normal Map", "Normal Map (RGB)"), _bumpMap,
                        _bumpScale);
                    if (EditorGUI.EndChangeCheck())
                    {
                        materialEditor.RegisterPropertyChangeUndo("BumpEnabledDisabled");

                        foreach (var obj in materialEditor.targets)
                        {
                            var mat = (Material) obj;
                            SetupNormalMode(mat, mat.GetTexture(_bumpMap.name));
                        }
                    }
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Outline", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                EditorGUILayout.LabelField("Width", EditorStyles.boldLabel);
                {
                    // Outline
                    EditorGUI.BeginChangeCheck();

                    PopupEnum<OutlineWidthMode>("Mode", _outlineWidthMode, materialEditor);
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

                    if (EditorGUI.EndChangeCheck())
                    {
                        var colorMode = (OutlineColorMode) _outlineColorMode.floatValue;
                        foreach (var obj in materialEditor.targets)
                            SetupOutlineMode((Material) obj, widthMode, colorMode);
                    }
                }
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Color", EditorStyles.boldLabel);
                {
                    var widthMode = (OutlineWidthMode) _outlineWidthMode.floatValue;
                    if (widthMode != OutlineWidthMode.None)
                    {
                        EditorGUI.BeginChangeCheck();

                        PopupEnum<OutlineColorMode>("Mode", _outlineColorMode, materialEditor);
                        var colorMode = (OutlineColorMode) _outlineColorMode.floatValue;

                        materialEditor.ShaderProperty(_outlineColor, "Color");
                        if (colorMode == OutlineColorMode.MixedLighting)
                            materialEditor.DefaultShaderProperty(_outlineLightingMix, "Lighting Mix");

                        if (EditorGUI.EndChangeCheck())
                            foreach (var obj in materialEditor.targets)
                                SetupOutlineMode((Material) obj, widthMode, colorMode);
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
                        var mode = (DebugMode) _debugMode.floatValue;
                        foreach (var obj in materialEditor.targets) SetupDebugMode((Material) obj, mode);
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

    private void DrawWidthProperties()
    {
    }

    private bool PopupEnum<T>(string name, MaterialProperty property, MaterialEditor editor) where T : struct
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

    private void SetupBlendMode(Material material, RenderMode renderMode, bool setRenderQueueAsDefault)
    {
        setRenderQueueAsDefault |= material.renderQueue == (int) RenderQueue.Geometry;
        
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
                if (setRenderQueueAsDefault)
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
                if (setRenderQueueAsDefault)
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
                if (setRenderQueueAsDefault)
                {
                    material.renderQueue = (int) RenderQueue.Transparent;
                }
                break;
        }
    }

    private void SetupOutlineMode(Material material, OutlineWidthMode outlineWidthMode,
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

    private void SetupNormalMode(Material material, bool requireNormalMapping)
    {
        SetKeyword(material, "_NORMALMAP", requireNormalMapping);
    }

    private void SetupCullMode(Material material, CullMode cullMode)
    {
        switch (cullMode)
        {
            case CullMode.Back:
                material.SetInt(_cullMode.name, (int) CullMode.Back);
                material.SetInt(_outlineCullMode.name, (int) CullMode.Front);
                break;
            case CullMode.Front:
                material.SetInt(_cullMode.name, (int) CullMode.Front);
                material.SetInt(_outlineCullMode.name, (int) CullMode.Back);
                break;
            case CullMode.Off:
                material.SetInt(_cullMode.name, (int) CullMode.Off);
                material.SetInt(_outlineCullMode.name, (int) CullMode.Front);
                break;
        }
    }

    private void SetKeyword(Material mat, string keyword, bool required)
    {
        if (required)
            mat.EnableKeyword(keyword);
        else
            mat.DisableKeyword(keyword);
    }
}