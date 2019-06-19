﻿using System;
using System.Collections;
using System.Collections.Generic;
using fuj1n.MinimalDebugConsole;
using TMechs.Data;
using TMechs.Environment.Targets;
using TMechs.UI;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        public static Rewired.Player Input { get; private set; }
        public Animator Animator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public CharacterController Controller { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerMovement Movement { get; private set; }
        
        public PlayerCamera CameraController { get; private set; }
        public Camera Camera { get; private set; }

        public int maxHealth;
        public float damageCooldown = 0.25F;

        private float damageTimer = 0F;

        [Header("Objects")]
        public GameObject rocketFistGeo;

        [Header("Anchors")]
        public Transform rocketFistAnchor;
        public Transform pickupAnchor;

        [NonSerialized]
        public ThrowableContainer pickedUp;

        public static bool isGod = false;

        public float Health
        {
            get => health;
            set
            {
                if (isGod && value <= health)
                    return;
                health = value;
                UpdateHealth();
            }
        }

        private float health = 1F;

        private static readonly int Z_WRITE = Shader.PropertyToID("_ZWrite");

        private bool displayCursor;

        private void Awake()
        {
            Instance = this;

            Input = Rewired.ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            Animator = GetComponentInChildren<Animator>();
            Rigidbody = GetComponent<Rigidbody>();
            Controller = GetComponent<CharacterController>();
            Combat = GetComponent<PlayerCombat>();
            Movement = GetComponent<PlayerMovement>();
            CameraController = GameObject.FindObjectOfType<PlayerCamera>();
            Camera = CameraController.GetComponentInChildren<Camera>();
            
            // Configure shaders
            foreach (Renderer render in GetComponentsInChildren<Renderer>())
            {
                foreach (Material mat in render.materials)
                {
                    if ("Shader Graphs/Player".Equals(mat.shader.name))
                    {
                        mat.SetInt(Z_WRITE, 1);
                    }
                }
            }
        }

        private void Update()
        {
            #if !UNITY_EDITOR
            Cursor.lockState = displayCursor ? CursorLockMode.None : CursorLockMode.Locked;
            #endif            
            if (Input.GetButtonDown(Controls.Action.MENU) && !MenuController.Instance)
            {
                Instantiate(Resources.Load<GameObject>("UI/Menu"));
                MenuActions.SetPause(true);
            }

            if (damageTimer >= 0F)
                damageTimer -= Time.deltaTime;

            if (pickedUp)
                GamepadLabels.AddLabel(IconMap.Icon.R2, "Throw");
            else
                GamepadLabels.AddLabel(IconMap.Icon.ActionTopRow1, "Attack");

            if (Animator)
                Animator.SetBool(Anim.IS_CARRYING, pickedUp);
        }

        public void Damage(float damage)
        {
            if (damage > 0 && damageTimer > 0F)
                return;
            Health -= damage / maxHealth;
        }

        public void SavePlayerData(ref SaveSystem.SaveData data)
        {
            data.health = Health;
        }

        public void LoadPlayerData(SaveSystem.SaveData data)
        {
            Health = data.health;
        }

        private void UpdateHealth()
        {
            if (health <= 0F)
            {
                IEnumerator Death()
                {
                    yield return new WaitForSeconds(2F);
                    SceneTransition.LoadScene(0);
                }

                Animator.SetTrigger(Anim.DIE);
                StartCoroutine(Death());
            }
        }

        private void OnEnable()
        {
            fuj1n.MinimalDebugConsole.DebugConsole.Instance.OnConsoleToggle += OnConsoleToggle;
        }

        private void OnDisable()
        {
            fuj1n.MinimalDebugConsole.DebugConsole.Instance.OnConsoleToggle -= OnConsoleToggle;
        }

        private void OnConsoleToggle(bool state)
        {
            MenuActions.SetPause(state);
            displayCursor = state;
        }

        public IEnumerable<string> GetDebugInfo()
        {
            List<string> ret = new List<string>
            {
                    $"World Position: {transform.position}",
                    $"World Rotation: {transform.eulerAngles}",
                    $"Health: {Health * maxHealth} / {maxHealth}",
                    $"Velocity: {Controller.velocity}"
            };

            return ret;
        }

        [DebugConsoleCommand("god")]
        private static void ToggleGodMode()
        {
            isGod = !isGod;
            fuj1n.MinimalDebugConsole.DebugConsole.Instance.AddMessage($"God mode {(isGod ? "enabled" : "disabled")}", Color.cyan);
        }

        [DebugConsoleCommand("tp")]
        private static void Teleport(Vector3 pos)
        {
            Instance.Controller.enabled = false;
            Instance.transform.position = pos;
            Instance.Controller.enabled = true;
        }

        [DebugConsoleCommand("playerVar")]
        private static void SetVariable(PlayerVar variable, float value)
        {
            float oldVal = 0F;
            
            switch (variable)
            {
                case PlayerVar.MovementSpeed:
                    oldVal = Instance.Movement.movementSpeed;
                    Instance.Movement.movementSpeed = value;
                    break;
                case PlayerVar.RunSpeed:
                    oldVal = Instance.Movement.runSpeed;
                    Instance.Movement.runSpeed = value;
                    break;
                case PlayerVar.JumpForce:
                    oldVal = Instance.Movement.jumpForce;
                    Instance.Movement.jumpForce = value;
                    break;
                case PlayerVar.JumpCount:
                    oldVal = Instance.Movement.maxJumps;
                    Instance.Movement.maxJumps = (int) value;
                    break;
            }
            
            fuj1n.MinimalDebugConsole.DebugConsole.Instance.AddMessage($"{variable}: old = {oldVal} new = {value}", Color.cyan);
        }

        private enum PlayerVar
        {
            MovementSpeed,
            RunSpeed,
            JumpForce,
            JumpCount
        }
    }
}