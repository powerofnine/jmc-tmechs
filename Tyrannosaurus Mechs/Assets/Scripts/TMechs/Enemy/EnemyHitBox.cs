using System;
using UnityEngine;

namespace TMechs.Enemy
{
    public class EnemyHitBox : MonoBehaviour
    {
        public string id = "";

        [NonSerialized]
        public int damage;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
                Player.Player.Instance.Damage(damage);
        }
    }
}