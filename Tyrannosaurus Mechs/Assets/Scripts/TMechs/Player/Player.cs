using System;
using System.Collections.Generic;
using Animancer;
using fuj1n.MinimalDebugConsole;
using JetBrains.Annotations;
using Rewired;
using TMechs.Animation;
using TMechs.Data;
using TMechs.Entity;
using TMechs.Player.Behavior;
using TMechs.Player.Modules;
using TMechs.UI;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour, EntityHealth.IDeath, EntityHealth.IDamage, EntityHealth.IHeal
    {
        public const int LAYER_ARMS = 0;
        public const int LAYER_FALL = 1;
        public const int LAYER_GENERIC_1 = 2;
        public const int LAYER_GENERIC_2 = 3;
        public const int LAYER_LEGS = 4;
        public const int LAYER_TOP = 5;

        public static Player Instance { get; private set; }
        public static Rewired.Player Input { get; private set; }

        public Transform centerOfMass;

        public EntityHealth Health { get; private set; }
        public EventfulAnimancerComponent Animancer { get; private set; }
        public PlayerCamera CameraController { get; private set; }
        public Camera Camera { get; private set; }
        
        private readonly List<PlayerModule> newModules = new List<PlayerModule>();
        private readonly List<PlayerModule> modules = new List<PlayerModule>();

        [AnimationCollection.ValidateAttribute(typeof(PlayerAnim))]
        public AnimationCollection animations;

        [NonSerialized]
        public Vector3 contactPoint;
        
        [Header("Modules")]
        public ForcesModule forces = new ForcesModule();
        public MovementModule movement = new MovementModule();
        public CombatModule combat = new CombatModule();
        public VfxModule vfx = new VfxModule();
        public InteractionModule interaction = new InteractionModule();

        [Header("Behavior")]
        public BehaviorStandard standard = new BehaviorStandard();
        public BehaviorDash dash = new BehaviorDash();
        public BehaviorJump jump = new BehaviorJump();
        public BehaviorGrapple grapple = new BehaviorGrapple();
        public BehaviorAttack attack = new BehaviorAttack();
        public BehaviorRocketFist rocketFist = new BehaviorRocketFist();
        public BehaviorCarry carry = new BehaviorCarry();
        
        private readonly Stack<PlayerBehavior> behaviorStack = new Stack<PlayerBehavior>();
        private readonly HashSet<PlayerBehavior> initializedBehaviors = new HashSet<PlayerBehavior>();

        [NotNull]
        public PlayerBehavior Behavior => behaviorStack.Count > 0 ? behaviorStack.Peek() : standard;
        public bool CanMove => Behavior.CanMove();
        public float Speed => Behavior.GetSpeed();

        [Header("Visual")]
        public InverseKinematics rightArmIk;

        [Space]
        public GameObject deathScreenTemplate;

        [Header("Debug")]
        public GameObject tankyloIntro;
        
        private bool displayCursor;

        private AnimancerState takeDamage;
        
        #region Events
        private void Awake()
        {
            Instance = this;
            
            Input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            Health = GetComponent<EntityHealth>();
            CameraController = FindObjectOfType<PlayerCamera>();
            Camera = CameraController.GetComponentInChildren<Camera>();
            
            Animancer = GetComponentInChildren<EventfulAnimancerComponent>();
            Animancer.onEvent = new AnimationEventReceiver(null, OnAnimationEvent);
            AnimancerPlayable.maxLayerCount = 6;
            takeDamage = Animancer.GetOrCreateState(GetClip(PlayerAnim.TakeDamage), LAYER_TOP);

            Animancer.GetLayer(LAYER_ARMS).SetName("Arms Layer");
            Animancer.GetLayer(LAYER_FALL).SetName("Fall Layer");
            Animancer.GetLayer(LAYER_GENERIC_1).SetName("Generic Layer 1");
            Animancer.GetLayer(LAYER_GENERIC_2).SetName("Generic Layer 2");
            Animancer.GetLayer(LAYER_LEGS).SetName("Legs Layer");
            Animancer.GetLayer(LAYER_TOP).SetName("Topmost Layer");
            
            RegisterModule(forces);
            RegisterModule(movement);
            RegisterModule(combat);
            RegisterModule(vfx);
            RegisterModule(interaction);
            
            PushBehavior(standard);
        }

        private void Update()
        {
#if !UNITY_EDITOR
            Cursor.lockState = displayCursor ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = displayCursor;
#endif
            // Pauses audio when the game is paused,
            // all menu related audio should ignore the pause state
            bool shouldPauseAudio = Time.timeScale <= Mathf.Epsilon && !(Behavior is BehaviorDead);
            if (shouldPauseAudio != AudioListener.pause)
                AudioListener.pause = shouldPauseAudio;
            
            if (Time.timeScale <= Mathf.Epsilon)
                return;
            if (Behavior is BehaviorDead)
                return;
            
            if (Input.GetButtonDown(Controls.Action.MENU) && !MenuController.Instance)
            {
                Instantiate(Resources.Load<GameObject>("UI/Menu"));
                MenuActions.SetPause(true);
                return;
            }
            
            for (int i = 0; i < newModules.Count; i++)
            {
                PlayerModule module = newModules[i];
                
                if (!module.enabled)
                    continue;
                
                module.OnStart();
                newModules.RemoveAt(i);
                modules.Add(module);

                i--;
            }
            
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnUpdate();
            
            Behavior.OnUpdate();
        }

        private void LateUpdate()
        {
            if (Time.timeScale <= Mathf.Epsilon)
                return;
            
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnLateUpdate();
            
            Behavior.OnLateUpdate();
        }

        private void FixedUpdate()
        {
            if (Time.timeScale <= Mathf.Epsilon)
                return;
            
            foreach (PlayerModule module in modules)
                if(module.enabled)
                    module.OnFixedUpdate();
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
            => contactPoint = hit.point;

        private void OnAnimationEvent(AnimationEvent e)
            => Behavior.OnAnimationEvent(e);
        
        private void OnEnable()
        {
            DebugConsole.Instance.OnConsoleToggle += OnConsoleToggle;
        }

        private void OnDisable()
        {
            DebugConsole.Instance.OnConsoleToggle -= OnConsoleToggle;
        }
        
        private void OnConsoleToggle(bool state)
        {
            MenuActions.SetPause(state);
            displayCursor = state;
        }

        #endregion
        
        public void SavePlayerData(ref SaveSystem.SaveData data)
        {
            data.health = Health.Health;
        }

        public void LoadPlayerData(SaveSystem.SaveData data)
        {
            Health.Health = data.health;
        }
        
        public void RegisterModule(PlayerModule module)
        {
            if (module == null)
                throw new ArgumentException("Module is null");

            module.player = this;
            newModules.Add(module);
            module.OnRegistered();
        }

        public AnimationClip GetClip(PlayerAnim animation)
            => animations ? animations.GetClip(animation) : null;
        
        public void PushBehavior([NotNull] PlayerBehavior behavior)
        { 
            behavior.player = this;
            if (!initializedBehaviors.Contains(behavior))
            {
                behavior.OnInit();
                initializedBehaviors.Add(behavior);
            }
            
            Behavior.OnShadowed();

            behaviorStack.Push(behavior);
            behavior.OnPush();
        }

        [NotNull]
        public PlayerBehavior PopBehavior()
        {
            Behavior.OnPop();

            if(behaviorStack.Count > 1)
                return behaviorStack.Pop();
            
            Behavior.OnSurfaced();

            throw new InvalidOperationException("Cannot pop the last player behavior");
        }

        // ReSharper disable once RedundantAssignment
        public void OnDying(EntityHealth.DamageSource source, ref bool customDestroy)
        {
            if (source.Source)
            {
                transform.LookAt(source.Source.transform.position.Set(transform.position.y, Utility.Axis.Y));
                movement.ResetIntendedY();
            }
            
            customDestroy = true;
            PushBehavior(new BehaviorDead());
        }
        
        public void OnHealed(EntityHealth health, ref bool cancel)
        {
            if(!vfx.heal)
                return;
            
            vfx.heal.gameObject.SetActive(true);
            vfx.heal.Play();
        }
        
        public void OnDamaged(EntityHealth health, ref bool cancel)
        {
            if (isGod)
                cancel = true;
            
            Rumble.SetRumble(Rumble.CHANNEL_DAMAGED, .5F, .25F, .05F);

            if (Behavior.CanAnimateTakeDamage() && !takeDamage.IsPlaying)
            {
                Animancer.Play(takeDamage).OnEnd = () =>
                {
                    takeDamage.OnEnd = null;
                    takeDamage.StartFade(0F, .025F);
                };
            }
        }
        
        [AnimationCollection.Enum("Player Animations")]
        public enum PlayerAnim
        {
            [Header("Basic")]
            Idle,
            IdleBreather,
            Death,
            TakeDamage,
        
            [Header("Movement")]
            Walk,
            Run,
            Dash,
            Jump,
            AirJump,
            Fall,
            Land,
        
            [Header("Attack String")]
            Attack1,
            Attack2,
            Attack3,
            
            [Header("Grapple")]
            GrabObject,
            LongDistanceGrab,
            LongDistanceGrabReturn,
            ThrowObject,
            Grapple,
            Pummel,
        
            [Header("Rocket Fist")]
            RocketChargeIntro,
            RocketCharge,
            RocketHold,
            RocketBuckle,
            RocketRecover,
            RocketReturn,
            
            [Header("Interaction")]
            PullLever
        }
        
        #region Debug
        
        public IEnumerable<string> GetDebugInfo()
        {
            List<string> ret = new List<string>
            {
                    $"World Position: {transform.position}",
                    $"World Rotation: {transform.eulerAngles}",
                    $"Health: {Health.Health * Health.maxHealth} / {Health.maxHealth}",
                    $"Velocity: {forces.ControllerVelocity}"
            };

            return ret;
        }

        private static bool isGod;
        [DebugConsoleCommand("god")]
        private static void ToggleGodMode()
        {
            isGod = !isGod;
            DebugConsole.Instance.AddMessage($"God mode {(isGod ? "enabled" : "disabled")}", Color.cyan);
        }

        [DebugConsoleCommand("tp")]
        private static void Teleport(Vector3 pos)
        {
            Instance.forces.Teleport(pos);
        }

        [DebugConsoleCommand("intro")]
        private static void SpawnIntro()
        {
            Instantiate(Instance.tankyloIntro);
        }
        
        [DebugConsoleCommand("playerVar")]
        private static void SetVariable(PlayerVar variable, float value)
        {
            float oldVal = 0F;
            
            switch (variable)
            {
                case PlayerVar.MovementSpeed:
                    oldVal = Instance.movement.movementSpeed;
                    Instance.movement.movementSpeed = value;
                    break;
                case PlayerVar.RunSpeed:
                    oldVal = Instance.movement.runSpeed;
                    Instance.movement.runSpeed = value;
                    break;
                case PlayerVar.JumpForce:
                    oldVal = Instance.jump.jumpForce;
                    Instance.jump.jumpForce = value;
                    break;
                case PlayerVar.JumpCount:
                    oldVal = Instance.jump.maxAirJumps;
                    Instance.jump.maxAirJumps = (int) value;
                    break;
            }
            
            DebugConsole.Instance.AddMessage($"{variable}: old = {oldVal} new = {value}", Color.cyan);
        }

        [DebugConsoleCommand("damage")]
        private static void DebugDamage(string entity, float amount)
        {
            EntityHealth[] healths = FindObjectsOfType<EntityHealth>();

            foreach (EntityHealth health in healths)
            {
                if(health.gameObject.name.Contains(entity))
                    health.Damage(amount, default(EntityHealth.DamageSource).GetWithSource(Instance.transform));
            }
        }

        private enum PlayerVar
        {
            MovementSpeed,
            RunSpeed,
            JumpForce,
            JumpCount
        }
        
        #endregion
    }
}
