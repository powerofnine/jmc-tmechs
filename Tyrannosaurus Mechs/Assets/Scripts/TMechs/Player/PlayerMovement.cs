using System;
using TMechs.InspectorAttributes;
using UnityEngine;
using UnityEngine.Serialization;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class PlayerMovement : MonoBehaviour
    {
        public static Rewired.Player Input { get; private set; }

        [Name("AA Camera")] public Transform aaCamera;

        [Header("Forces")] public float movementSpeed = 10F;
        public float jumpForce = 2 * 9.8F;

        public int maxJumps = 2;

        // State
        private float intendedY;

        private new Collider collider;
        private Rigidbody rb;

        private int jumps;

        private Animator animator;

        private void Awake()
        {
            animator = GetComponent<Animator>();
            animator.SetFloat("Player Speed", movementSpeed);

            Input = Rewired.ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            if (!aaCamera)
            {
                Debug.LogWarning("Camera not given to player, expect unintended gameplay");
                aaCamera = transform;
            }

            intendedY = transform.eulerAngles.y;

            collider = GetComponentInChildren<Collider>();
            rb = GetComponent<Rigidbody>();
        }

        private void Update()
        {
            Vector3 movement = Input.GetAxis2DRaw(MOVE_HORIZONTAL, MOVE_VERTICAL).RemapXZ();

            // Multiply movement by camera quaternion so that it is relative to the camera
            movement = Quaternion.Euler(0F, aaCamera.eulerAngles.y, 0F) * movement;

            float movementMag = movement.sqrMagnitude;

            if (movementMag > float.Epsilon)
            {
                if (movementMag > 1F)
                    movement.Normalize();

                intendedY = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg;
            }

            Vector3 rot = transform.eulerAngles;
            if (Math.Abs(rot.y - intendedY) > float.Epsilon)
            {
                Vector3 inRot = rot;
                inRot.y = intendedY;

                transform.rotation =
                    Quaternion.Lerp(Quaternion.Euler(rot), Quaternion.Euler(inRot), 15 * Time.deltaTime);
            }

            transform.Translate(movement * movementSpeed * Time.deltaTime, Space.World);

            if (jumps > 0 && IsGrounded())
                jumps = 0;

            if (Input.GetButtonDown(JUMP) && jumps < maxJumps)
            {
                if (!IsGrounded())
                    jumps++;
                rb.velocity = Vector3.up * jumpForce;
            }
        }

        private bool IsGrounded() =>
            Physics.Raycast(collider.bounds.center, -transform.up, collider.bounds.extents.y + .025F);
    }
}