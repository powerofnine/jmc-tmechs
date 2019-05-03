using System;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TMechs.Data.Settings;
using UnityEngine;

namespace TMechs.UI.GamePad
{
    [AddComponentMenu("")]
    public class ButtonDisplayController : MonoBehaviour
    {
        public static ButtonDisplayController Instance { get; private set; }
        
        private Dictionary<Guid, ControllerDef> controllers;

        public ControllerDef Controller { get; private set; }
        public ControllerDef AbsoluteController { get; private set; }

        public ControllerDef.ButtonLayout ButtonLayout
        {
            get
            {
                ControllerSettings settings = SettingsData.Get<ControllerSettings>();
            
                ControllerDef.ButtonLayout layout = settings.buttonLayout;
                if (settings.autoDetectControllerType)
                    layout = Controller.layout;

                return layout;
            }
        }

        private ControllerDef unsupported;
        
        private void Awake()
        {
            if (Instance)
            {
                Destroy(this);
                return;
            }
            
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            unsupported = ScriptableObject.CreateInstance<ControllerDef>();
        }

        private void Start()
        {
            controllers = Resources.LoadAll<ControllerDef>("").ToDictionary(x => Guid.Parse(x.guid));

            if(ReInput.controllers.joystickCount > 0) {
                Joystick joy = ReInput.controllers.Joysticks[0];
                if (controllers.ContainsKey(joy.hardwareTypeGuid))
                    Controller = controllers[joy.hardwareTypeGuid];
            }
        }

        private void Update()
        {
            Controller controller = ReInput.controllers.GetLastActiveController();

            ControllerDef abs = null;
            
            if (controller is Joystick)
            {
                Joystick joystick = (Joystick) controller;
                if (controllers.ContainsKey(joystick.hardwareTypeGuid))
                {
                    Controller = controllers[joystick.hardwareTypeGuid];
                    abs = Controller;
                }
            }

            if (!Controller || !abs)
            {
                unsupported.guid = controller is Joystick ? ((Joystick)controller).hardwareTypeGuid.ToString() : Guid.Empty.ToString();
                unsupported.layout = ControllerDef.ButtonLayout.Unsupported;
                unsupported.padName = controller.type.ToString();

                if(!Controller)
                    Controller = unsupported;
                if (!abs)
                    abs = unsupported;
            }

            AbsoluteController = abs;
        }

        private void OnDestroy() => DestroyImmediate(unsupported);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            GameObject go = new GameObject("Button Display Controller");
            go.AddComponent<ButtonDisplayController>();
        }
    }
}
