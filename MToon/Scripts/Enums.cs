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

    public enum CullMode
    {
        Off,
        Front,
        Back,
    }

    public struct RenderQueueRequirement
    {
        public int DefaultValue;
        public int MinValue;
        public int MaxValue;
    }
}