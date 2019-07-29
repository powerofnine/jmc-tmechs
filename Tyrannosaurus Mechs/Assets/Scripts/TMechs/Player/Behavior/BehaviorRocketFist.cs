using System;
using Animancer;
using TMechs.Entity;
using TMechs.Environment.Targets;
using TMechs.UI.GamePad;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TMechs.Player.Behavior
{
    [Serializable]
    public class BehaviorRocketFist : PlayerBehavior
    {
        private const int ATTACK_LAYER = 2;

        public float[] damageStages =
        {
                10F,
                15F,
                30F,
                50F,
                75F
        };
        public Color[] damageColors =
        {
            Color.red,
            new Color(1F, .5F, 0), 
            Color.yellow,
            Color.cyan,
            Color.blue
        };
        
        public float maxChargeTime = 5F;
        public float rechargeSpeed = 2F;
        public float rocketFistReturnTime = 1.5F;
        
        [NonSerialized]
        public float rocketFistCharge;
        
        public int ChargeStage => Mathf.Clamp(Mathf.CeilToInt(rocketFistCharge / ((maxChargeTime + 1F) / damageStages.Length)) - 1, 0, damageStages.Length - 1);

        [Space]
        public GameObject rocketFistTemplate;
        public GameObject rocketFistGeo;
        public Transform rocketFistAnchor;
        
        private AnimancerState intro;
        private AnimancerState charge;
        private AnimancerState hold;
        private AnimancerState buckle;
        private AnimancerState fire;
        private AnimancerState comeBack;

        private AnimancerState current;
        
        private bool charging;
        private bool fired;
        private float returnTimer;
        [NonSerialized]
        public bool rocketReturned;

        private int prevStage;
        
        public override void OnInit()
        {
            base.OnInit();

            intro = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketChargeIntro), ATTACK_LAYER);
            charge = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketCharge), ATTACK_LAYER);
            hold = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketHold), ATTACK_LAYER);
            buckle = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketBuckle), ATTACK_LAYER);
            fire = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketRecover), ATTACK_LAYER);
            comeBack = Animancer.GetOrCreateState(player.GetClip(Player.PlayerAnim.RocketReturn), ATTACK_LAYER);
            
            if(damageColors.Length != damageStages.Length)
                Debug.LogWarning("Damage colors list is different length to damage stage list");
        }

        public override void OnPush()
        {
            base.OnPush();

            prevStage = 0;
            charging = false;
            fired = false;
            rocketReturned = false;
            returnTimer = 0F;

            Animancer.CrossFadeFromStart(intro).OnEnd = () =>
            {
                intro.OnEnd = null;

                charging = true;
                current = Animancer.CrossFadeFromStart(charge, .1F);

                current.OnEnd = () =>
                {
                    current = Animancer.CrossFadeFromStart(hold, .1F);
                    hold.OnEnd = null;
                };
            };

            if (player.vfx.rocketFistCharge)
            {
                player.vfx.rocketFistCharge.gameObject.SetActive(true);
                player.vfx.rocketFistCharge.Play();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            int stage = ChargeStage;
            if (charging)
            {
                rocketFistCharge += Time.deltaTime;
                stage = ChargeStage;
                
                if (stage > prevStage)
                {
                    if (player.vfx.rocketBurst)
                    {
                        player.vfx.rocketBurst.gameObject.SetActive(true);
                        player.vfx.rocketBurst.Play();
                    }
                    
                    buckle.Stop();
                    Animancer.CrossFadeFromStart(buckle, 0F).OnEnd = () =>
                    {
                        buckle.Stop();
                        current?.Play();
                    };
                    
                    if(stage >= damageStages.Length - 1 && player.vfx.rocketOvercharge)
                    {
                        player.vfx.rocketOvercharge.gameObject.SetActive(true);
                        player.vfx.rocketOvercharge.Play();
                    }
                }
            }

            rocketFistCharge = Mathf.Clamp(rocketFistCharge, 0F, maxChargeTime);

            prevStage = stage;
            
            if (rocketReturned)
            {
                if (returnTimer <= rocketFistReturnTime)
                {
                    returnTimer += Time.deltaTime;
                    return;
                }
                
                Animancer.CrossFadeFromStart(comeBack, .1F).OnEnd = () =>
                {
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                    comeBack.OnEnd = null;
                };
                
                rocketFistGeo.SetActive(true);
                player.PopBehavior();
                return;
            }
            
            if (!fired)
            {
                GamepadLabels.AddLabel(IconMap.Icon.L2, "Rocket Fist");
                EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();
                
                if(enemy)
                    transform.LookAt(enemy.transform.position.Set(transform.position.y, Utility.Axis.Y));
                player.movement.ResetIntendedY();
            }
            
            if (charging && !Input.GetButton(Controls.Action.LEFT_ARM))
            {
                intro.OnEnd = null;
                charge.OnEnd = null;

                charging = false;
                
                EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();
                if (!enemy)
                {
                    player.PopBehavior();
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                    return;
                }
                
                RocketFist rf = Object.Instantiate(rocketFistTemplate, rocketFistAnchor.position, rocketFistAnchor.rotation).GetComponent<RocketFist>();
                
                rf.damage = damageStages[stage];
                rf.target = enemy.transform;
                rocketFistGeo.SetActive(false);

                if (player.vfx.rocketShot)
                {
                    player.vfx.rocketShot.gameObject.SetActive(true);
                    player.vfx.rocketShot.Play();
                }
                
                if(player.vfx.rocketOvercharge)
                    player.vfx.rocketOvercharge.Stop();
                
                if(player.vfx.rocketFistCharge)
                    player.vfx.rocketFistCharge.Stop();
                Animancer.CrossFadeFromStart(fire).OnEnd = () =>
                {
                    fire.OnEnd = null;
                    fired = true;
                    Animancer.GetLayer(ATTACK_LAYER).StartFade(0F);
                };
            }
        }

        public override void OnPop()
        {
            base.OnPop();
            
            if(player.vfx.rocketFistCharge)
                player.vfx.rocketFistCharge.gameObject.SetActive(false);
            
            if(player.vfx.rocketOvercharge)
                player.vfx.rocketOvercharge.Stop();
        }

        public override float GetSpeed() => base.GetSpeed() * (fired ? .8F : .4F);
        public override bool CanRun() => false;
    }
}
