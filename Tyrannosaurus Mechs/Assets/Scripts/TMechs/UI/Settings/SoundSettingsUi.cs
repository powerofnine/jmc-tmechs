using TMechs.Data.Settings;
using TMechs.UI.Components;
using UnityEngine;

namespace TMechs.UI.Settings
{
    public class SoundSettingsUi : MonoBehaviour
    {
        public UiSlider masterSlider;
        public UiSlider musicSlider;
        public UiSlider sfxSlider;
        
        private void Awake()
        {
            SoundSettings settings = SettingsData.Get<SoundSettings>();

            if (masterSlider)
            {
                masterSlider.SetInstant(settings.volume[SoundSettings.Channel.Master]);
                masterSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Master] = masterSlider.Value);
            }
            if (musicSlider)
            {
                musicSlider.SetInstant(settings.volume[SoundSettings.Channel.Music]);
                musicSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Music] = musicSlider.Value);
            }
            if (sfxSlider)
            {
                sfxSlider.SetInstant(settings.volume[SoundSettings.Channel.Sfx]);
                sfxSlider.onValueChange.AddListener(ob => settings.volume[SoundSettings.Channel.Sfx] = sfxSlider.Value);
            }
        }
    }
}