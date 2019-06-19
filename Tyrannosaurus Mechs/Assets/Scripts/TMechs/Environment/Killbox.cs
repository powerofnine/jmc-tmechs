using System;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Environment
{
    public class Killbox : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Player.Player.Instance.Damage(5000000);
                return;
            }

            EntityHealth health = other.GetComponent<EntityHealth>();
            
            if(health)
                health.Damage(5000000F);
        }
    }
}
