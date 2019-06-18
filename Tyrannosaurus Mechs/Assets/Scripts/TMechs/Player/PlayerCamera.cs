using System;
using TMechs.Data.Settings;
using TMechs.Environment.Targets;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        private static Rewired.Player Input => Player.Input;

        public Transform player;
        public float cameraSpeed = 10F;

        [Header("Limits")]
        public float minX;
        public float maxX;

        [Header("Camera Rig")]
        public Transform aaRig;
        public Transform verticalRig;

        [NonSerialized]
        public new Camera camera;
        
        [Space]
        public Vector2 dampening;

        private CameraState state;

        private void Awake()
        {
            state.parent = this;

            if (!aaRig)
            {
                Debug.LogWarning("Missing axis aligned rig reference, expect camera errors");
                aaRig = transform;
            }

            if (!verticalRig)
            {
                Debug.LogWarning("Missing vertical rig reference, vertical camera rotation will not work");
                verticalRig = new GameObject("Vertical Stand-in").transform;
            }

            state.rotationX = verticalRig.localEulerAngles.x;
            camera = GetComponentInChildren<Camera>();
        }

        private void LateUpdate()
        {
            if (Time.timeScale <= float.Epsilon)
                return;
            
            if (TargetController.Instance.GetLock())
                LockedCamera();
            else
                FreeCamera();

            state.ClampState();
            transform.localEulerAngles = transform.localEulerAngles.Set(state.DampedY, Utility.Axis.Y);
            verticalRig.localEulerAngles = verticalRig.localEulerAngles.Set(state.DampedX, Utility.Axis.X);
        }

        public void LockedCamera()
        {
            transform.position = player.position;
            state.rotationX = 0F;
            state.rotationY = player.localEulerAngles.y;
        }

        public void FreeCamera()
        {
            CameraSettings settings = SettingsData.Get<CameraSettings>();

            // Get the difference between our position and the player's
            Vector3 playerDelta = transform.InverseTransformPoint(player.position);

            switch (settings.mode)
            {
                case CameraSettings.CameraMode.Pan:
                    transform.Translate(playerDelta.x, playerDelta.y, playerDelta.z, Space.Self);
                    break;
                case CameraSettings.CameraMode.Rotate:
                {
                    // Move towards the player in the local z axis
                    transform.Translate(0F, playerDelta.y, playerDelta.z, Space.Self);

                    Vector3 playerCamPos = player.position.Remove(Utility.Axis.Y);
                    Vector3 camCamPos = aaRig.position.Remove(Utility.Axis.Y);

                    // Calculate the angle between the aa rig and the player
                    float offsetAngle = Vector3.SignedAngle(playerCamPos - camCamPos, aaRig.forward, Vector3.up);

                    // Rotate around the aa rig to face the player
                    transform.RotateAround(camCamPos, Vector3.up, -offsetAngle);
                    if (Mathf.Abs(offsetAngle) > 1F)
                        state.rotationY = transform.localEulerAngles.y;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            Vector2 input = Input.GetAxis2DRaw(CAMERA_HORIZONTAL, CAMERA_VERTICAL);

            state.rotationY += input.x * cameraSpeed * Time.deltaTime * (settings.invertHorizontal ? -1 : 1);
            state.rotationX += -input.y * cameraSpeed * Time.deltaTime * (settings.invertVertical ? -1 : 1);
            state.rotationX = Mathf.Clamp(state.rotationX, minX, maxX);

            if (Input.GetButtonDown(CAMERA_CENTER))
                RecenterCamera();
        }

        public void RecenterCamera()
        {
            state.rotationX = 0F;
            state.rotationY = player.localEulerAngles.y;
        }
        
        private struct CameraState
        {
            public PlayerCamera parent;

            public float rotationX;
            public float rotationY;

            private float xVelocity;
            private float yVelocity;

            public float DampedX =>
                    Mathf.SmoothDampAngle(parent.verticalRig.localEulerAngles.x, rotationX, ref xVelocity, parent.dampening.x);

            public float DampedY => Mathf.SmoothDampAngle(parent.transform.localEulerAngles.y, rotationY, ref yVelocity, parent.dampening.y);

            public void ClampState()
            {
                // Loops x and y rotations between -360 and 360
                while (rotationX >= 360F)
                    rotationX -= 360F;
                while (rotationX <= -360F)
                    rotationX += 360F;

                while (rotationY >= 360F)
                    rotationY -= 360F;
                while (rotationY <= -360F)
                    rotationY += 360F;
            }
        }
    }
}