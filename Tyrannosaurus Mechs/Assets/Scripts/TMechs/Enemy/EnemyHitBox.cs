using System;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyHitBox : MonoBehaviour
    {
        public string id = "";

        [NonSerialized]
        public float damage;

        public EntityHealth.DamageSource damageSource;
        public Action callback;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                Player.Player.Instance.Health.Damage(damage, damageSource.GetWithSource(transform));
        }
    }
}