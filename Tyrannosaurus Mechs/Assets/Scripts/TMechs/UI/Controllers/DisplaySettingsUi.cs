using System.Globalization;
using TMechs.Data.Settings;
using TMechs.UI.Components;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class DisplaySettingsUi : MonoBehaviour
    {
        public UiSlider brightnessSlider;
        public UiSlider gammaSlider;
        public UiCheckbox vsyncSlider;

        private void Awake()
        {
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();
            
            if (brightnessSlider)
            {
                brightnessSlider.SetInstant(settings.brightness);
                brightnessSlider.onValueChange.AddListener(ob => settings.brightness = brightnessSlider.Value);
            }
            if (gammaSlider)
            {
                gammaSlider.SetInstant(settings.gamma);
                gammaSlider.onValueChange.AddListener(ob => settings.gamma = gammaSlider.Value);
            }

            if (vsyncSlider)
            {
                vsyncSlider.SetInstant(settings.vsync);
                vsyncSlider.onValueChange.AddListener(ob => settings.vsync = vsyncSlider.IsChecked);
            }
        }
    }
}