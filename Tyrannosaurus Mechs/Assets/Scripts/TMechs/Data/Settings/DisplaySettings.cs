using TMechs.Attributes;
using UnityEngine;

namespace TMechs.Data.Settings
{
    [SettingsProvider]
    public class DisplaySettings
    {
        public Resolution resolution = Screen.currentResolution;
        public FullscreenMode fullscreenMode = ModeFromUnity(Screen.fullScreenMode);
        public int qualityLevel = QualitySettings.GetQualityLevel();
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

        public enum FullscreenMode
        {
            [FriendlyName("Windowed")]
            Windowed,
            [FriendlyName("Fullscreen (Window)")]
            FullscreenWindow,
            [FriendlyName("Fullscreen (Exclusive)")]
            FullscreenExclusive
        }

        public static FullscreenMode ModeFromUnity(FullScreenMode mode)
        {
            switch (mode)
            {
                case FullScreenMode.Windowed:
                case FullScreenMode.MaximizedWindow:
                    return FullscreenMode.Windowed;
                case FullScreenMode.ExclusiveFullScreen:
                    return FullscreenMode.FullscreenExclusive;
                case FullScreenMode.FullScreenWindow:
                    return FullscreenMode.FullscreenWindow;
                
            }

            return FullscreenMode.FullscreenWindow;
        }

        public static FullScreenMode ModeToUnity(FullscreenMode mode)
        {
            switch (mode)
            {
                case FullscreenMode.Windowed:
                    return FullScreenMode.Windowed;
                case FullscreenMode.FullscreenExclusive:
                    return FullScreenMode.ExclusiveFullScreen;
                case FullscreenMode.FullscreenWindow:
                    return FullScreenMode.FullScreenWindow;
            }

            return FullScreenMode.FullScreenWindow;
        }
    }
}