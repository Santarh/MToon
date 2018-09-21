using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
{
    public class MToonInspector : ShaderGUI
    {
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
            _debugMode = FindProperty(Utils.PropDebugMode, properties);
            _outlineWidthMode = FindProperty(Utils.PropOutlineWidthMode, properties);
            _outlineColorMode = FindProperty(Utils.PropOutlineColorMode, properties);
            _blendMode = FindProperty(Utils.PropBlendMode, properties);
            _cullMode = FindProperty(Utils.PropCullMode, properties);
            _outlineCullMode = FindProperty(Utils.PropOutlineCullMode, properties);
            _cutoff = FindProperty(Utils.PropCutoff, properties);
            _color = FindProperty(Utils.PropColor, properties);
            _shadeColor = FindProperty(Utils.PropShadeColor, properties);
            _mainTex = FindProperty(Utils.PropMainTex, properties);
            _shadeTexture = FindProperty(Utils.PropShadeTexture, properties);
            _bumpScale = FindProperty(Utils.PropBumpScale, properties);
            _bumpMap = FindProperty(Utils.PropBumpMap, properties);
            _receiveShadowRate = FindProperty(Utils.PropReceiveShadowRate, properties);
            _receiveShadowTexture = FindProperty(Utils.PropReceiveShadowTexture, properties);
            _shadingGradeRate = FindProperty(Utils.PropShadingGradeRate, properties);
            _shadingGradeTexture = FindProperty(Utils.PropShadingGradeTexture, properties);
            _shadeShift = FindProperty(Utils.PropShadeShift, properties);
            _shadeToony = FindProperty(Utils.PropShadeToony, properties);
            _lightColorAttenuation = FindProperty(Utils.PropLightColorAttenuation, properties);
            _indirectLightIntensity = FindProperty(Utils.PropIndirectLightIntensity, properties);
            _sphereAdd = FindProperty(Utils.PropSphereAdd, properties);
            _emissionColor = FindProperty(Utils.PropEmissionColor, properties);
            _emissionMap = FindProperty(Utils.PropEmissionMap, properties);
            _outlineWidthTexture = FindProperty(Utils.PropOutlineWidthTexture, properties);
            _outlineWidth = FindProperty(Utils.PropOutlineWidth, properties);
            _outlineScaledMaxDistance = FindProperty(Utils.PropOutlineScaledMaxDistance, properties);
            _outlineColor = FindProperty(Utils.PropOutlineColor, properties);
            _outlineLightingMix = FindProperty(Utils.PropOutlineLightingMix, properties);

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

        private void Draw(MaterialEditor materialEditor, Material[] materials,
            MaterialProperty[] uvMappedTextureProperties)
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
                    if (bm == RenderMode.Cutout)
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.LabelField("Alpha", EditorStyles.boldLabel);
                        {
                            materialEditor.ShaderProperty(_cutoff, "Cutoff");
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
            SetupBlendMode(material, (RenderMode) material.GetFloat(Utils.PropBlendMode), isBlendModeChangedByUser);
            SetupNormalMode(material, material.GetTexture(Utils.PropBumpMap));
            SetupOutlineMode(material,
                (OutlineWidthMode) material.GetFloat(Utils.PropOutlineWidthMode),
                (OutlineColorMode) material.GetFloat(Utils.PropOutlineColorMode));
            SetupDebugMode(material, (DebugMode) material.GetFloat(Utils.PropDebugMode));
            SetupCullMode(material, (CullMode) material.GetFloat(Utils.PropCullMode));

            var mainTex = material.GetTexture(Utils.PropMainTex);
            var shadeTex = material.GetTexture(Utils.PropShadeTexture);
            if (mainTex != null && shadeTex == null)
            {
                material.SetTexture(Utils.PropShadeTexture, mainTex);
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
                    SetKeyword(material, Utils.KeyDebugNormal, false);
                    SetKeyword(material, Utils.KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.Normal:
                    SetKeyword(material, Utils.KeyDebugNormal, true);
                    SetKeyword(material, Utils.KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.LitShadeRate:
                    SetKeyword(material, Utils.KeyDebugNormal, false);
                    SetKeyword(material, Utils.KeyDebugLitShadeRate, true);
                    break;
            }
        }

        private static void SetupBlendMode(Material material, RenderMode renderMode, bool isChangedByUser)
        {
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    material.SetOverrideTag(Utils.TagRenderTypeKey, Utils.TagRenderTypeValueOpaque);
                    material.SetInt(Utils.PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(Utils.PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Utils.PropZWrite, 1);
                    SetKeyword(material, Utils.KeyAlphaTestOn, false);
                    SetKeyword(material, Utils.KeyAlphaBlendOn, false);
                    SetKeyword(material, Utils.KeyAlphaPremultiplyOn, false);
                    if (isChangedByUser)
                    {
                        material.renderQueue = -1;
                    }

                    break;
                case RenderMode.Cutout:
                    material.SetOverrideTag(Utils.TagRenderTypeKey, Utils.TagRenderTypeValueTransparentCutout);
                    material.SetInt(Utils.PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(Utils.PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(Utils.PropZWrite, 1);
                    SetKeyword(material, Utils.KeyAlphaTestOn, true);
                    SetKeyword(material, Utils.KeyAlphaBlendOn, false);
                    SetKeyword(material, Utils.KeyAlphaPremultiplyOn, false);
                    if (isChangedByUser)
                    {
                        material.renderQueue = (int) RenderQueue.AlphaTest;
                    }

                    break;
                case RenderMode.Transparent:
                    material.SetOverrideTag(Utils.TagRenderTypeKey, Utils.TagRenderTypeValueTransparent);
                    material.SetInt(Utils.PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Utils.PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Utils.PropZWrite, 0);
                    SetKeyword(material, Utils.KeyAlphaTestOn, false);
                    SetKeyword(material, Utils.KeyAlphaBlendOn, true);
                    SetKeyword(material, Utils.KeyAlphaPremultiplyOn, false);
                    if (isChangedByUser)
                    {
                        material.renderQueue = (int) RenderQueue.Transparent;
                    }

                    break;
                case RenderMode.TransparentWithZWrite:
                    material.SetOverrideTag(Utils.TagRenderTypeKey, Utils.TagRenderTypeValueTransparent);
                    material.SetInt(Utils.PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(Utils.PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(Utils.PropZWrite, 1);
                    SetKeyword(material, Utils.KeyAlphaTestOn, false);
                    SetKeyword(material, Utils.KeyAlphaBlendOn, true);
                    SetKeyword(material, Utils.KeyAlphaPremultiplyOn, false);
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
            var isFixed = outlineColorMode == OutlineColorMode.FixedColor;
            var isMixed = outlineColorMode == OutlineColorMode.MixedLighting;
            
            switch (outlineWidthMode)
            {
                case OutlineWidthMode.None:
                    SetKeyword(material, Utils.KeyOutlineWidthWorld, false);
                    SetKeyword(material, Utils.KeyOutlineWidthScreen, false);
                    SetKeyword(material, Utils.KeyOutlineColorFixed, false);
                    SetKeyword(material, Utils.KeyOutlineColorMixed, false);
                    break;
                case OutlineWidthMode.WorldCoordinates:
                    SetKeyword(material, Utils.KeyOutlineWidthWorld, true);
                    SetKeyword(material, Utils.KeyOutlineWidthScreen, false);
                    SetKeyword(material, Utils.KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, Utils.KeyOutlineColorMixed, isMixed);
                    break;
                case OutlineWidthMode.ScreenCoordinates:
                    SetKeyword(material, Utils.KeyOutlineWidthWorld, false);
                    SetKeyword(material, Utils.KeyOutlineWidthScreen, true);
                    SetKeyword(material, Utils.KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, Utils.KeyOutlineColorMixed, isMixed);
                    break;
            }
        }

        private static void SetupNormalMode(Material material, bool requireNormalMapping)
        {
            SetKeyword(material, Utils.KeyNormalMap, requireNormalMapping);
        }

        private static void SetupCullMode(Material material, CullMode cullMode)
        {
            switch (cullMode)
            {
                case CullMode.Back:
                    material.SetInt(Utils.PropCullMode, (int) CullMode.Back);
                    material.SetInt(Utils.PropOutlineCullMode, (int) CullMode.Front);
                    break;
                case CullMode.Front:
                    material.SetInt(Utils.PropCullMode, (int) CullMode.Front);
                    material.SetInt(Utils.PropOutlineCullMode, (int) CullMode.Back);
                    break;
                case CullMode.Off:
                    material.SetInt(Utils.PropCullMode, (int) CullMode.Off);
                    material.SetInt(Utils.PropOutlineCullMode, (int) CullMode.Front);
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
}