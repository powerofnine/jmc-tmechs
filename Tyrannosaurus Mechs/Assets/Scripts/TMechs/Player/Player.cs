using System;
using System.Collections.Generic;
using TMechs.Environment.Targets;
using TMechs.UI;
using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        public Animator Animator { get; private set; }
        public Rigidbody Rigidbody { get; private set; }
        public CharacterController Controller { get; private set; }
        public PlayerCombat Combat { get; private set; }

        public int maxHealth;

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

        private void Awake()
        {
            Instance = this;

            Animator = GetComponent<Animator>();
            Rigidbody = GetComponent<Rigidbody>();
            Controller = GetComponent<CharacterController>();
            Combat = GetComponent<PlayerCombat>();
        }

        private void Update()
        {
            if (PlayerMovement.Input.GetButtonDown(Controls.Action.MENU) && !MenuController.Instance)
            {
                Instantiate(Resources.Load<GameObject>("UI/Menu"));
                MenuActions.SetPause(true);
            }

            UpdateIcons();
        }

        public void Damage(int damage)
            => Health -= (float) damage / maxHealth;

        private void UpdateHealth()
        {
            if (health <= 0F)
            {
                //TODO proper death
                Destroy(gameObject);
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

            if (PlayerMovement.Input.GetButton(Controls.Action.ANGERY))
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