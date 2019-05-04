using System;
using System.Collections.Generic;
using System.Linq;

namespace TMechs.Data.Settings
{
    [SettingsProvider]
    public class SoundSettings
    {
        public Dictionary<Channel, int> volume = Enum.GetValues(typeof(Channel)).Cast<Channel>().ToDictionary(x => x, x => 100);

        public enum Channel
        {
            Music,
            Sfx,
            Master = -9999
        }
    }
}