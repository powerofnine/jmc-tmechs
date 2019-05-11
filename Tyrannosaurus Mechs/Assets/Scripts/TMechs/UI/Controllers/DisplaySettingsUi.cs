using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TMechs.Attributes;
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
        public UiSelection fpsDisplay;

        private UiNavigation navigation;

        private Dictionary<int, DisplaySettings.FpsDisplay> fpsValueMap;

        private Dictionary<string, Resolution> resolutions;
        private static readonly List<string> screenModes = FriendlyNameAttribute.GetNames<DisplaySettings.FullscreenMode>(false).ToList();
        
        private void Awake()
        {
            navigation = GetComponentInParent<UiNavigation>();
            
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

            if (fpsDisplay)
            {
                fpsValueMap = fpsDisplay.SetEnum<DisplaySettings.FpsDisplay>();

                fpsDisplay.Value = fpsValueMap.SingleOrDefault(x => x.Value.Equals(settings.fpsDisplay)).Key;
                fpsDisplay.onValueChange.AddListener(ob => settings.fpsDisplay = fpsValueMap[fpsDisplay.Value]);
            }
        }

        [UsedImplicitly]
        public void OpenResolutionDialog()
        {
            resolutions = Screen.resolutions.Reverse().ToDictionary(x => x.ToString());
            
            int index = 0;

            if (resolutions.ContainsValue(Screen.currentResolution))
                index = resolutions.Values.ToList().IndexOf(Screen.currentResolution);
            
            navigation.OpenModal("Display Resolution", resolutions.Keys, SetResolution, index);
        }

        private void SetResolution(string result)
        {
            Debug.Log(resolutions.ContainsKey(result));
            if (resolutions.ContainsKey(result))
            {
                DisplaySettings settings = SettingsData.Get<DisplaySettings>();
                settings.resolution = resolutions[result];
                SettingsApplier.ApplyResolution();
            }
        }
        
        [UsedImplicitly]
        public void OpenDisplayModeDialog()
        {
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();
            
            navigation.OpenModal("Fullscreen Mode", screenModes, SetDisplayMode, (int)settings.fullscreenMode);
        }

        private void SetDisplayMode(string result)
        {
            if (screenModes.Contains(result))
            {
                DisplaySettings settings = SettingsData.Get<DisplaySettings>();
                settings.fullscreenMode = (DisplaySettings.FullscreenMode) screenModes.IndexOf(result);
                SettingsApplier.ApplyResolution();
            }
        }

        [UsedImplicitly]
        public void OpenQualityDialog()
        {
            navigation.OpenModal("Quality", QualitySettings.names, SetQualityLevel, QualitySettings.GetQualityLevel());
        }

        private void SetQualityLevel(string quality)
        {
            DisplaySettings settings = SettingsData.Get<DisplaySettings>();
            settings.qualityLevel = QualitySettings.names.ToList().IndexOf(quality);
            SettingsApplier.ApplyQualitySettings();
        }
    }
}