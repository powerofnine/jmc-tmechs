﻿using System;
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

        public int maxJumps = 1;

        // State
        private float intendedY;
        private float yDampVelocity;

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
                float inRot = Mathf.SmoothDampAngle(transform.eulerAngles.y, intendedY, ref yDampVelocity, .1F);

                transform.eulerAngles = transform.eulerAngles.Set(inRot, Utility.Axis.Y);
            }

            rb.velocity = rb.velocity.Isolate(Utility.Axis.Y) + movement * movementSpeed;

            RaycastHit? ground = GetGround();
            
            if (jumps > 0 && ground != null)
                jumps = 0;

            if (Input.GetButtonDown(JUMP) && jumps < maxJumps)
            {
                if (ground == null)
                    jumps++;
                rb.velocity = rb.velocity.Set(jumpForce, Utility.Axis.Y);
            }
        }

        private RaycastHit? GetGround()
        {
            if (Physics.Raycast(collider.bounds.center, -transform.up, out RaycastHit hit, collider.bounds.extents.y + .1F))
                return hit;
            return null;
        }
    }
}