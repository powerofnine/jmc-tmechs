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

        private Dictionary<int, ControllerDef.ButtonLayout> map;

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
                map = iconSet.SetEnum<ControllerDef.ButtonLayout>();

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
            ControllerSettings settings = SettingsData.Get<ControllerSettings>();

            ControllerDef def = ButtonDisplayController.Instance.AbsoluteController;
            if (controllerName)
                controllerName.text = string.Format(nameFormat, def.padName);
            if (controllerLayout)
                controllerLayout.text = string.Format(layoutFormat, def.layout);
            if (controllerGuid)
                controllerGuid.text = string.Format(guidFormat, def.guid);

            if (iconSet)
                iconSet.gameObject.SetActive(!settings.autoDetectControllerType);
        }
    }
}