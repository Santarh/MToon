using UnityEngine;

namespace MToon
{
    public class MToonDefinition
    {
        public MetaDefinition Meta;
        public RenderingDefinition Rendering;
        public ColorDefinition Color;
        public LightingDefinition Lighting;
        public OutlineDefinition Outline;
        public TextureUvCoordsDefinition TextureOption;
        public RenderQueueDefinition RenderQueue;
    }

    public class MetaDefinition
    {
        public int VersionNumber;
    }

    public class RenderingDefinition
    {
        public RenderMode RenderMode;
        public CullMode CullMode;
    }

    public class ColorDefinition
    {
        public Color LitColor;
        public Texture2D LitMultiplyTexture;
        public Color ShadeColor;
        public Texture2D ShadeMultiplyTexture;
        public float CutoutThresholdValue;
    }
    
    public class LightingDefinition
    {
        public LitAndShadeMixingDefinition LitAndShadeMixing;
        public LightingInfluenceDefinition LightingInfluence;
        public EmissionDefinition Emission;
        public MatCapDefinition MatCap;
        public NormalDefinition Normal;
    }
    
    public class LitAndShadeMixingDefinition
    {
        public float ShadingShiftValue;
        public float ShadingToonyValue;
        public float ShadowReceiveMultiplierValue;
        public Texture2D ShadowReceiveMultiplierMultiplyTexture;
        public float LitAndShadeMixingMultiplierValue;
        public Texture2D LitAndShadeMixingMultiplierMultiplyTexture;
    }

    public class LightingInfluenceDefinition
    {
        public float LightColorAttenuationValue;
        public float GiIntensityValue;
    }

    public class EmissionDefinition
    {
        public Color EmissionColor;
        public Texture2D EmissionMultiplyTexture;
    }

    public class MatCapDefinition
    {
        public Texture2D AdditiveTexture;
    }

    public class NormalDefinition
    {
        public Texture2D NormalTexture;
        public float NormalScaleValue;
    }

    public class OutlineDefinition
    {
        public OutlineWidthMode OutlineWidthMode;
        public float OutlineWidthValue;
        public Texture2D OutlineWidthMultiplyTexture;
        public float OutlineScaledMaxDistanceValue;
        public OutlineColorMode OutlineColorMode;
        public Color OutlineColor;
        public float OutlineLightingMixValue;
    }

    public class TextureUvCoordsDefinition
    {
        public Vector2 MainTextureLeftBottomOriginScale;
        public Vector2 MainTextureLeftBottomOriginOffset;
    }

    public class RenderQueueDefinition
    {
        public RenderMode RenderQueueOriginMode;
        public int RenderQueueOffsetNumber;
    }
}