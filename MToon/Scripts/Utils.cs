using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace MToon
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

    public struct RenderQueueRequirement
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
    }

    public static class Utils
    {
        public const string PropDebugMode = "_DebugMode";
        public const string PropOutlineWidthMode = "_OutlineWidthMode";
        public const string PropOutlineColorMode = "_OutlineColorMode";
        public const string PropBlendMode = "_BlendMode";
        public const string PropCullMode = "_CullMode";
        public const string PropOutlineCullMode = "_OutlineCullMode";
        public const string PropCutoff = "_Cutoff";
        public const string PropColor = "_Color";
        public const string PropShadeColor = "_ShadeColor";
        public const string PropMainTex = "_MainTex";
        public const string PropShadeTexture = "_ShadeTexture";
        public const string PropBumpScale = "_BumpScale";
        public const string PropBumpMap = "_BumpMap";
        public const string PropReceiveShadowRate = "_ReceiveShadowRate";
        public const string PropReceiveShadowTexture = "_ReceiveShadowTexture";
        public const string PropShadingGradeRate = "_ShadingGradeRate";
        public const string PropShadingGradeTexture = "_ShadingGradeTexture";
        public const string PropShadeShift = "_ShadeShift";
        public const string PropShadeToony = "_ShadeToony";
        public const string PropLightColorAttenuation = "_LightColorAttenuation";
        public const string PropIndirectLightIntensity = "_IndirectLightIntensity";
        public const string PropSphereAdd = "_SphereAdd";
        public const string PropEmissionColor = "_EmissionColor";
        public const string PropEmissionMap = "_EmissionMap";
        public const string PropOutlineWidthTexture = "_OutlineWidthTexture";
        public const string PropOutlineWidth = "_OutlineWidth";
        public const string PropOutlineScaledMaxDistance = "_OutlineScaledMaxDistance";
        public const string PropOutlineColor = "_OutlineColor";
        public const string PropOutlineLightingMix = "_OutlineLightingMix";
        public const string PropSrcBlend = "_SrcBlend";
        public const string PropDstBlend = "_DstBlend";
        public const string PropZWrite = "_ZWrite";

        public const string KeyNormalMap = "_NORMALMAP";
        public const string KeyAlphaTestOn = "_ALPHATEST_ON";
        public const string KeyAlphaBlendOn = "_ALPHABLEND_ON";
        public const string KeyAlphaPremultiplyOn = "_ALPHAPREMULTIPLY_ON";
        public const string KeyOutlineWidthWorld = "MTOON_OUTLINE_WIDTH_WORLD";
        public const string KeyOutlineWidthScreen = "MTOON_OUTLINE_WIDTH_SCREEN";
        public const string KeyOutlineColorFixed = "MTOON_OUTLINE_COLOR_FIXED";
        public const string KeyOutlineColorMixed = "MTOON_OUTLINE_COLOR_MIXED";
        public const string KeyDebugNormal = "MTOON_DEBUG_NORMAL";
        public const string KeyDebugLitShadeRate = "MTOON_DEBUG_LITSHADERATE";

        public const string TagRenderTypeKey = "RenderType";
        public const string TagRenderTypeValueOpaque = "Opaque";
        public const string TagRenderTypeValueTransparentCutout = "TransparentCutout";
        public const string TagRenderTypeValueTransparent = "Transparent";

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

        public static RenderQueueRequirement GetRenderQueueRequirement(RenderMode renderMode)
        {
            const int shaderDefaultQueue = -1;
            const int firstTransparentQueue = 2501;
            const int spanOfQueue = 50;
            
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = shaderDefaultQueue,
                        MinValue = shaderDefaultQueue,
                        MaxValue = shaderDefaultQueue,
                    };
                case RenderMode.Cutout:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = (int) RenderQueue.AlphaTest,
                        MinValue = (int) RenderQueue.AlphaTest,
                        MaxValue = (int) RenderQueue.AlphaTest,
                    };
                case RenderMode.Transparent:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = (int) RenderQueue.Transparent,
                        MinValue = (int) RenderQueue.Transparent - spanOfQueue + 1,
                        MaxValue = (int) RenderQueue.Transparent,
                    };
                case RenderMode.TransparentWithZWrite:
                    return new RenderQueueRequirement()
                    {
                        DefaultValue = firstTransparentQueue,
                        MinValue = firstTransparentQueue,
                        MaxValue = firstTransparentQueue + spanOfQueue - 1,
                    };
                default:
                    throw new ArgumentOutOfRangeException("renderMode", renderMode, null);
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

        private static void ValidateBlendMode(Material material, RenderMode renderMode, bool isChangedByUser)
        {
            switch (renderMode)
            {
                case RenderMode.Opaque:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueOpaque);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, 1);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Cutout:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparentCutout);
                    material.SetInt(PropSrcBlend, (int) BlendMode.One);
                    material.SetInt(PropDstBlend, (int) BlendMode.Zero);
                    material.SetInt(PropZWrite, 1);
                    SetKeyword(material, KeyAlphaTestOn, true);
                    SetKeyword(material, KeyAlphaBlendOn, false);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.Transparent:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, 0);
                    SetKeyword(material, KeyAlphaTestOn, false);
                    SetKeyword(material, KeyAlphaBlendOn, true);
                    SetKeyword(material, KeyAlphaPremultiplyOn, false);
                    break;
                case RenderMode.TransparentWithZWrite:
                    material.SetOverrideTag(TagRenderTypeKey, TagRenderTypeValueTransparent);
                    material.SetInt(PropSrcBlend, (int) BlendMode.SrcAlpha);
                    material.SetInt(PropDstBlend, (int) BlendMode.OneMinusSrcAlpha);
                    material.SetInt(PropZWrite, 1);
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