namespace TMechs.Data.Settings
{
    [SettingsProvider]
    public class CameraSettings
    {
        public bool invertHorizontal;
        public bool invertVertical;
        public CameraMode mode;

        public enum CameraMode
        {
            Pan,
            Rotate
        }
    }
}
