using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    public class ButtonDisplayController : MonoBehaviour
    {
        public static ButtonDisplayController Instance { get; private set; }
        
        private Dictionary<Guid, ControllerDef> controllers;

        public ControllerDef Controller { get; private set; }

        private ControllerDef custom;
        
        private void Awake()
        {
            custom = ScriptableObject.CreateInstance<ControllerDef>();
        }

        private void Start()
        {
            Instance = this;
            controllers = Resources.LoadAll<ControllerDef>("").ToDictionary(x => Guid.Parse(x.guid));
        }

        private void Update()
        {
            Controller controller = ReInput.controllers.GetLastActiveController();

            if (controller is Joystick)
            {
                Joystick joystick = (Joystick) controller;
                Controller = controllers[joystick.hardwareTypeGuid];
            }

            if (!Controller)
            {
                custom.guid = Guid.Empty.ToString();
                custom.layout = ControllerDef.ButtonLayout.Unsupported;
                custom.padName = ReInput.controllers.GetLastActiveControllerType().ToString();

                Controller = custom;
            }
        }

        private void OnDestroy() => DestroyImmediate(custom);
    }
}
