using System;
using TMechs.Player.Modules;
using TMechs.UI;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Entity
{
    public class EntityHealth : MonoBehaviour
    {
        public float Health
        {
            get => health;
            set
            {
                health = Mathf.Clamp01(value);
                UpdateHealth();
            }
        }

        public float maxHealth;

        [Header("Damaged VFX")]
        public VisualEffectAsset damagedEffect;
        public Transform damagedEffectAnchor;
        public float damagedEffectDuration;

        [Header("Boss Bar")]
        public bool assignBossBar;
        [ConditionalHide("assignBossBar", true)]
        public string bossBarTitle;

        private float health = 1F;
        private bool isDead;

        private void Start()
        {
            if (assignBossBar)
            {
                BossHealthBar.activeHealthBar = this;
                BossHealthBar.text = bossBarTitle;
            }
        }

        private void UpdateHealth()
        {
            if (health <= 0F && !isDead)
            {
                isDead = true;
                bool customDestroy = false;
                
                foreach (IDeath evt in GetComponentsInChildren<IDeath>(true))
                    evt.OnDying(ref customDestroy);
                
                if(!customDestroy)
                    Destroy(gameObject);
            }
        }

        public void Damage(float damage, DamageSource source, bool percent = false)
        {
            bool cancel = false;

            if (damage > 0F)
            {
                foreach (IDamage evt in GetComponentsInChildren<IDamage>(true))
                {
                    evt.OnDamaged(this, ref cancel);

                    if (cancel)
                        return;
                }

                if(!source.cancelDefaultVfx)
                    VfxModule.SpawnEffect(damagedEffect, damagedEffectAnchor ? damagedEffectAnchor.position : transform.position, Quaternion.identity, damagedEffectDuration);
            }
            else if (damage < 0F)
            {
                foreach (IHeal evt in GetComponentsInChildren<IHeal>(true))
                {
                    evt.OnHealed(this, ref cancel);

                    if (cancel)
                        return;
                }
            }

            Health -= damage / (percent ? 1F : maxHealth);

            if (source.effect)
            {
                Vector3 pos = transform.position;
                Vector3 forward = Vector3.forward;

                switch (source.location)
                {
                    case DamageSource.EffectLocation.CenterSource when source.Source:
                        pos = source.Source.position;
                        break;
                    case DamageSource.EffectLocation.CenterSource:
                    case DamageSource.EffectLocation.TargetEffectAnchor:
                        pos = damagedEffectAnchor ? damagedEffectAnchor.position : transform.position;
                        break;
                }

                if (source.Source)
                {
                    switch (source.orient)
                    {
                        case DamageSource.EffectOrient.FaceSource:
                            forward = source.Source.position - transform.position;
                            break;
                        case DamageSource.EffectOrient.FaceTarget:
                            forward = transform.position - source.Source.position;
                            break;
                    }
                    
                    forward.Normalize();
                }

                VfxModule.SpawnEffect(source.effect, pos, Quaternion.identity, source.effectDuration).transform.forward = forward;
            }
        }

        public void Heal(float health, DamageSource source, bool percent = false)
        {
            Damage(-health, source, percent);
        }

        public interface IDamage
        {
            void OnDamaged(EntityHealth health, ref bool cancel);
        }

        public interface IHeal
        {
            void OnHealed(EntityHealth health, ref bool cancel);
        }

        public interface IDeath
        {
            void OnDying(ref bool customDestroy);
        }
        
        [Serializable]
        public struct DamageSource
        {
            [Header("VFX")]
            public bool cancelDefaultVfx;
            public VisualEffectAsset effect;
            public float effectDuration;
            public EffectLocation location;
            public EffectOrient orient;
            
            public Transform Source { get; private set; }
            
            public DamageSource GetWithSource(Transform t)
            {
                DamageSource source = this;
                source.Source = t;
                return source;
            }

            public enum EffectLocation
            {
                TargetEffectAnchor,
                CenterSource
            }

            public enum EffectOrient
            {
                Identity,
                FaceTarget,
                FaceSource
            }
        }
    }
}