using TMechs.UI.GamePad;

namespace TMechs.Data.Settings
{
    [SettingsProvider]
    public class ControllerSettings
    {
        public bool autoDetectControllerType = true;
        public ControllerDef.ButtonLayout buttonLayout = ControllerDef.ButtonLayout.Ps3;
    }
}