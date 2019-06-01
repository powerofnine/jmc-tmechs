using TMechs.Data.Settings;
using TMechs.UI.Components;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class CameraSettingsUi : MonoBehaviour
    {
        public UiCheckbox invertHorizontal;
        public UiCheckbox invertVertical;
        public UiCheckbox cameraPan;

        private void Awake()
        {
            CameraSettings settings = SettingsData.Get<CameraSettings>();

            if (invertHorizontal)
            {
                invertHorizontal.SetInstant(settings.invertHorizontal);
                invertHorizontal.onValueChange.AddListener(ob => settings.invertHorizontal = invertHorizontal.IsChecked);
            }

            if (invertVertical)
            {
                invertVertical.SetInstant(settings.invertVertical);
                invertVertical.onValueChange.AddListener(ob => settings.invertVertical = invertVertical.IsChecked);
            }

            if (cameraPan)
            {
                cameraPan.SetInstant(settings.mode == CameraSettings.CameraMode.Pan);
                cameraPan.onValueChange.AddListener(ob => settings.mode = cameraPan.IsChecked ? CameraSettings.CameraMode.Pan : CameraSettings.CameraMode.Rotate);
            }
        }
    }
}