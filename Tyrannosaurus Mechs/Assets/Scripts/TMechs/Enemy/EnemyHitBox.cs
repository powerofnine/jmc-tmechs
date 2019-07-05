using System;
using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyHitBox : MonoBehaviour
    {
        public string id = "";

        [NonSerialized]
        public int damage;

        private void Awake()
        {
            gameObject.SetActive(false);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                PlayerOld.Player.Instance.Damage(damage);
        }
    }
}