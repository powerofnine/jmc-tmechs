using System;
using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public static Rewired.Player Input { get; private set; }

        public new Transform camera;

        public float movementSpeed = 10F;
        
        // State
        private float intendedY;
        
        private void Awake()
        {
            Input = Rewired.ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            if (!camera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                camera = transform;
            }

            intendedY = transform.eulerAngles.y;
        }

        private void Update()
        {
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();
            movement = Quaternion.Euler(0F, camera.eulerAngles.y, 0F) * movement;
            
            float movementMag = movement.sqrMagnitude;
            
            if (movementMag > float.Epsilon)
            {
                if (movementMag > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }

            Vector3 rot = transform.eulerAngles;
            if(Math.Abs(rot.y - intendedY) > float.Epsilon)
            {
                Vector3 inRot = rot;
                inRot.y = intendedY;

                transform.rotation = Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(inRot), 15 * Time.deltaTime);
            }

            transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);
        }
    }
    //Vector2 cam = Input.GetAxis2D(Controls.Action.CAMERA_HORIZONTAL, Controls.Action.CAMERA_VERTICAL);
}
