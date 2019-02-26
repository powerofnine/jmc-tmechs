using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        public Transform player;
        public float cameraSpeed = 10F;

        [Header("Limits")]
        public float minX;
        public float maxX;
        
        [Header("Camera Rig")]
        public Transform aaRig;
        public Transform verticalRig;

        private float rotationX;

        private void Awake()
        {
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
            
            rotationX = verticalRig.localEulerAngles.x;
        }

        private void LateUpdate()
        {           
            // Get the difference between our position and the player's
            Vector3 playerDelta = transform.InverseTransformPoint(player.position);
            
            // Move towards the player in the local z axis
            transform.Translate(0F, playerDelta.y, playerDelta.z, Space.Self);

            Vector3 playerCamPos = player.position.Remove(Utility.Axis.Y);
            Vector3 camCamPos = aaRig.position.Remove(Utility.Axis.Y);
            
            // Calculate the angle between the aa rig and the player
            float offsetAngle = Vector3.SignedAngle(playerCamPos - camCamPos, aaRig.forward, Vector3.up);
            
            // Rotate around the aa rig to face the player
            transform.RotateAround(camCamPos, Vector3.up, -offsetAngle);

            Vector2 input = PlayerMovement.Input.GetAxis2DRaw(CAMERA_HORIZONTAL, CAMERA_VERTICAL);
            
            transform.Rotate(0F, -input.x * cameraSpeed * Time.deltaTime, 0F);

            rotationX += input.y * cameraSpeed * Time.deltaTime;
            rotationX = Mathf.Clamp(rotationX, minX, maxX);
            
            verticalRig.localEulerAngles = new Vector3(rotationX, 0F, 0F);
        }
    }
}