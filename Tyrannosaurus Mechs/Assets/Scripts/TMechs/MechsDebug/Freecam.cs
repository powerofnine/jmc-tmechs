using fuj1n.MinimalDebugConsole;
using Rewired;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.MechsDebug
{
    public class Freecam : MonoBehaviour
    {
        public static bool IsFreecam => freecam != null;
        public static bool HideUi => IsFreecam || hideUi;
        
        private static Freecam freecam;
        
        private static float speed = 25F;
        private static float sensitivity = 15F;
        private static bool hideUi;
        
        private Rewired.Player input;

        private void Awake()
        {
            input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
        }

        private void Update()
        {
            Freecam.speed += Input.mouseScrollDelta.y * Time.deltaTime * 20F;
            
            Vector3 control = transform.rotation * input.GetAxis2D(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();
            Vector3 cam = input.GetAxis2D(CAMERA_VERTICAL, CAMERA_HORIZONTAL);

            float speed = Freecam.speed;
            if (input.GetButton(SPRINT))
                speed *= 2F;
            
            transform.position += Time.deltaTime * speed * control;
            transform.eulerAngles += Time.deltaTime * sensitivity * cam;
        }

        [DebugConsoleCommand("freecam")]
        private static void SetProperty(string property, float value)
        {
            switch (property.ToLower())
            {
                case "speed":
                    speed = value;
                    break;
                case "sensitivity":
                    sensitivity = value;
                    break;
                default:
                    DebugConsole.Instance.AddMessage($"{property} is not a valid property, valid properties are speed and sensitivity", Color.red);
                    break;
            }
        }
        
        [DebugConsoleCommand("freecam")]
        private static void Toggle()
        {
            if (IsFreecam)
            {
                Destroy(freecam.gameObject);
                DebugConsole.Instance.AddMessage($"Freecam <#FF0000>disabled</color>", Color.cyan);
            }
            else
            {
                GameObject go = new GameObject("Freecam");
                go.AddComponent<Camera>();
                freecam = go.AddComponent<Freecam>();

                Player.Player player = Player.Player.Instance;
                
                if (player)
                {
                    Transform trs = player.Camera.transform;
                    go.transform.position = trs.position;
                    go.transform.rotation = trs.rotation;
                }
            }
        }

        [DebugConsoleCommand("gui")]
        private static void ToggleUi()
        {
            hideUi = !hideUi;
        }
    }
}