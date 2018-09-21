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
        public const string KeyOutlineWidthWorld = "_MTOON_OUTLINE_WIDTH_WORLD";
        public const string KeyOutlineWidthScreen = "_MTOON_OUTLINE_WIDTH_SCREEN";
        public const string KeyOutlineColorFixed = "_MTOON_OUTLINE_COLOR_FIXED";
        public const string KeyOutlineColorMixed = "_MTOON_OUTLINE_COLOR_MIXED";
        public const string KeyDebugNormal = "MTOON_DEBUG_NORMAL";
        public const string KeyDebugLitShadeRate = "MTOON_DEBUG_LITSHADERATE";

        public const string TagRenderTypeKey = "RenderType";
        public const string TagRenderTypeValueOpaque = "Opaque";
        public const string TagRenderTypeValueTransparentCutout = "TransparentCutout";
        public const string TagRenderTypeValueTransparent = "Transparent";
    }
}