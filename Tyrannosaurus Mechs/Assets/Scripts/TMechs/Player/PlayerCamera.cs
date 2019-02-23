using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        public Transform player;
        public float cameraSpeed = 10F;

        private void LateUpdate()
        {
            // TODO camera follows player forwards and backwards by transforming
            // TODO when the player moves side to side, the camera follows by rotating
            // TODO see 'Ratchet and Clank' and Vexx
            transform.position = player.position;

            Vector2 cam = PlayerMovement.Input.GetAxis2DRaw(CAMERA_HORIZONTAL, CAMERA_VERTICAL);
            transform.Rotate(0F, cam.x * cameraSpeed * Time.deltaTime, 0F);
        }
    }
}