﻿using System;
using System.Collections.Generic;
using TMechs.Environment.Targets;
using TMechs.UI;
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

        public IEnumerable<string> GetDebugInfo()
        {
            List<string> ret = new List<string>();
            
            ret.Add($"World Position: {transform.position}");
            ret.Add($"World Rotation: {transform.eulerAngles}");
            ret.Add($"Health: {Health * maxHealth} / {maxHealth}");
            ret.Add($"Velocity: {Controller.velocity}");

            return ret;
        }
    }
}