using TMechs.Attributes;

namespace TMechs.Data.Settings
{
    [SettingsProvider]
    public class DisplaySettings
    {
        public float brightness;
        public float gamma;
        public bool vsync = true;
        public FpsDisplay fpsDisplay;

        public enum FpsDisplay
        {
            [FriendlyName("Don't Show")]
            None,
            [FriendlyName("Top Left")]
            TopLeft,
            [FriendlyName("Top Right")]
            TopRight,
            [FriendlyName("Bottom Left")]
            BottomLeft,
            [FriendlyName("Bottom Right")]
            BottomRight
        }
    }
}