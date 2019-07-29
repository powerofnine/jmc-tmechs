using System;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyBullet : MonoBehaviour
    {
        public float speed;
        public float lifeTime;

        public EntityHealth.DamageSource damageSource;
        
        [NonSerialized]
        public float damage;
        [NonSerialized]
        public Vector3 direction;
        [NonSerialized]
        public Transform owner;

        private void Update()
        {
            transform.position += speed * Time.deltaTime * direction;
            lifeTime -= Time.deltaTime;

            if (lifeTime <= 0F)
                Destroy(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.isTrigger || other.transform == owner)
                return;
            
            if(other.CompareTag("Player"))
                Player.Player.Instance.Health.Damage(damage, damageSource.GetWithSource(transform));
            
            Destroy(gameObject);
        }
    }
}