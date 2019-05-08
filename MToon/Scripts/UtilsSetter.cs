using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
{
    public static partial class Utils
    {
        private static void SetMToonParametersToMaterial(Material material, MToonDefinition parameters)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Validate properties and Set hidden properties, keywords.
        /// if isBlendModeChangedByUser is true, renderQueue will set specified render mode's default value.
        /// </summary>
        /// <param name="material"></param>
        /// <param name="isBlendModeChangedByUser"></param>
        public static void ValidateProperties(Material material, bool isBlendModeChangedByUser = false)
        {
            ValidateBlendMode(material, (RenderMode) material.GetFloat(PropBlendMode), isBlendModeChangedByUser);
            ValidateNormalMode(material, material.GetTexture(PropBumpMap));
            ValidateOutlineMode(material,
                (OutlineWidthMode) material.GetFloat(PropOutlineWidthMode),
                (OutlineColorMode) material.GetFloat(PropOutlineColorMode));
            ValidateDebugMode(material, (DebugMode) material.GetFloat(PropDebugMode));
            ValidateCullMode(material, (CullMode) material.GetFloat(PropCullMode));

            var mainTex = material.GetTexture(PropMainTex);
            var shadeTex = material.GetTexture(PropShadeTexture);
            if (mainTex != null && shadeTex == null)
            {
                material.SetTexture(PropShadeTexture, mainTex);
            }
        }

        private static void ValidateDebugMode(Material material, DebugMode debugMode)
        {
            switch (debugMode)
            {
                case DebugMode.None:
                    SetKeyword(material, KeyDebugNormal, false);
                    SetKeyword(material, KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.Normal:
                    SetKeyword(material, KeyDebugNormal, true);
                    SetKeyword(material, KeyDebugLitShadeRate, false);
                    break;
                case DebugMode.LitShadeRate:
                    SetKeyword(material, KeyDebugNormal, false);
                    SetKeyword(material, KeyDebugLitShadeRate, true);
                    break;
            }
        }

        public static void ValidateBlendMode(Material material, RenderMode renderMode, bool isChangedByUser)
        {
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueOpaque);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Cutout:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparentCutout);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, true);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Transparent:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, DisabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, true);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.TransparentWithZWrite:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, EnabledIntValue);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, true);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
            }

            var requirement = GetRenderQueueRequirement(renderMode);
            if (isChangedByUser)
            {
                material.renderQueue = requirement.DefaultValue;
            }
            else
            {
                material.renderQueue = Mathf.Clamp(material.renderQueue, requirement.MinValue, requirement.MaxValue);
            }
        }

        private static void ValidateOutlineMode(Material material, OutlineWidthMode outlineWidthMode,
            OutlineColorMode outlineColorMode)
        {
            var isFixed = outlineColorMode == OutlineColorMode.FixedColor;
            var isMixed = outlineColorMode == OutlineColorMode.MixedLighting;
            
            switch (outlineWidthMode)
            {
                case OutlineWidthMode.None:
                    SetKeyword(material, KeyOutlineWidthWorld, false);
                    SetKeyword(material, KeyOutlineWidthScreen, false);
                    SetKeyword(material, KeyOutlineColorFixed, false);
                    SetKeyword(material, KeyOutlineColorMixed, false);
                    break;
                case OutlineWidthMode.WorldCoordinates:
                    SetKeyword(material, KeyOutlineWidthWorld, true);
                    SetKeyword(material, KeyOutlineWidthScreen, false);
                    SetKeyword(material, KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, KeyOutlineColorMixed, isMixed);
                    break;
                case OutlineWidthMode.ScreenCoordinates:
                    SetKeyword(material, KeyOutlineWidthWorld, false);
                    SetKeyword(material, KeyOutlineWidthScreen, true);
                    SetKeyword(material, KeyOutlineColorFixed, isFixed);
                    SetKeyword(material, KeyOutlineColorMixed, isMixed);
                    break;
            }
        }

        private static void ValidateNormalMode(Material material, bool requireNormalMapping)
        {
            SetKeyword(material, KeyNormalMap, requireNormalMapping);
        }

        private static void ValidateCullMode(Material material, CullMode cullMode)
        {
            switch (cullMode)
            {
                case CullMode.Back:
                    material.SetInt(PropCullMode, (int) CullMode.Back);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Front);
                    break;
                case CullMode.Front:
                    material.SetInt(PropCullMode, (int) CullMode.Front);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Back);
                    break;
                case CullMode.Off:
                    material.SetInt(PropCullMode, (int) CullMode.Off);
                    material.SetInt(PropOutlineCullMode, (int) CullMode.Front);
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