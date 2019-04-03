using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMechs.Attributes;
using TMechs.Data.Settings;
using TMechs.UI.Components;
using TMechs.UI.GamePad;
using TMPro;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class ControllerSettingsUi : MonoBehaviour
    {
        public UiCheckbox autoIcons;
        public UiSelection iconSet;

        [Header("Controller Info")]
        public TextMeshProUGUI controllerName;
        public TextMeshProUGUI controllerLayout;
        public TextMeshProUGUI controllerGuid;

        private string nameFormat;
        private string layoutFormat;
        private string guidFormat;

        private readonly Dictionary<int, ControllerDef.ButtonLayout> map = new Dictionary<int, ControllerDef.ButtonLayout>();
        
        private void Awake()
        {
            ControllerSettings settings = SettingsData.Get<ControllerSettings>();

            if (autoIcons)
            {
                autoIcons.SetInstant(settings.autoDetectControllerType);
                autoIcons.onValueChange.AddListener(ob => settings.autoDetectControllerType = autoIcons.IsChecked);
            }

            if (iconSet)
            {
                ControllerDef.ButtonLayout[] layouts = Enum.GetValues(typeof(ControllerDef.ButtonLayout)).Cast<ControllerDef.ButtonLayout>().ToArray();
                List<string> values = new List<string>();
                
                foreach (ControllerDef.ButtonLayout layout in layouts)
                {
                    MemberInfo info = layout.GetType().GetMember(layout.ToString()).SingleOrDefault();
                    if (info == null)
                        continue;
                    
                    FriendlyName fn = (FriendlyName) info.GetCustomAttributes(typeof(FriendlyName)).SingleOrDefault();
                    if(fn == null)
                        continue;
                    
                    map.Add(values.Count, layout);
                    values.Add(fn.name);
                }

                iconSet.values = values.ToArray();
                
                iconSet.Value = map.SingleOrDefault(x => x.Value.Equals(settings.buttonLayout)).Key;
                iconSet.onValueChange.AddListener(ob => settings.buttonLayout = map[iconSet.Value]);
            }

            if (controllerName)
                nameFormat = controllerName.text;
            if (controllerLayout)
                layoutFormat = controllerLayout.text;
            if (controllerGuid)
                guidFormat = controllerGuid.text;
        }

        private void LateUpdate()
        {
            ControllerDef def = ButtonDisplayController.Instance.Controller;

            if (controllerName)
                controllerName.text = string.Format(nameFormat, def.padName);
            if (controllerLayout)
                controllerLayout.text = string.Format(layoutFormat, def.layout);
            if (controllerGuid)
                controllerGuid.text = string.Format(guidFormat, def.guid);
            
        }
    }
}
