using System;
using System.Collections.Generic;
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

        public int maxHealth;
        public float damageCooldown = 0.25F;

        private float damageTimer = 0F;
        
        [Header("Objects")]
        public GameObject rocketFistGeo;
                
        [Header("Anchors")]
        public Transform rocketFistAnchor;
        public Transform pickupAnchor;

        [NonSerialized]
        public EnemyTarget pickedUp;

        public float Health
        {
            get => health;
            set
            {
                health = value;
                UpdateHealth();
            }
        }

        private float health = 1F;
        
        private static readonly int Z_WRITE = Shader.PropertyToID("_ZWrite");

        private void Awake()
        {
            Instance = this;

            Input = Rewired.ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            
            Animator = GetComponentInChildren<Animator>();
            Rigidbody = GetComponent<Rigidbody>();
            Controller = GetComponent<CharacterController>();
            Combat = GetComponent<PlayerCombat>();
            Movement = GetComponent<PlayerMovement>();
            
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
            Cursor.lockState = CursorLockMode.Locked;
            
            if (Input.GetButtonDown(Controls.Action.MENU) && !MenuController.Instance)
            {
                Instantiate(Resources.Load<GameObject>("UI/Menu"));
                MenuActions.SetPause(true);
            }

            if (damageTimer >= 0F)
                damageTimer -= Time.deltaTime;

            UpdateIcons();
        }

        public void Damage(int damage)
        {
            if (damage > 0 && damageTimer > 0F)
                return;
            Health -= (float) damage / maxHealth;
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
                //TODO proper death
                SceneTransition.LoadScene(0);
            }
        }

        private void UpdateIcons()
        {
            GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionBottomRow1, "Jump");
            GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionBottomRow2, "");
            GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionTopRow1, "");
            GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionTopRow2, "Interact");

            if (pickedUp)
            {
                // State: picked up

                GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionBottomRow2, "Throw");

                return;
            }

            if (Input.GetButton(Controls.Action.ANGERY))
            {
                //State: angry

                if (Animator.GetBool(Anim.HAS_ENEMY))
                    GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionTopRow1, "Rocket Fist");
            }
            else
            {
                //State: normal

                GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionTopRow1, "Attack");
            }

            // Grab/Grapple label
            if (Animator.GetInteger(Anim.PICKUP_TARGET_TYPE) != 0)
                GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionBottomRow2, "Grab");
            else if (Animator.GetBool(Anim.HAS_GRAPPLE))
            {
                GrappleTarget target = TargetController.Instance.GetTarget<GrappleTarget>();

                if (target)
                    GamepadLabels.SetLabel(GamepadLabels.ButtonLabel.ActionBottomRow2, target.isSwing ? "Swing" : "Grapple");
            }
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
    }
}