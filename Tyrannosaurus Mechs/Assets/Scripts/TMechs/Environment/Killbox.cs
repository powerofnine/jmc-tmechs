using System;
using TMechs.Entity;
using UnityEngine;

namespace TMechs.Environment
{
    public class Killbox : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            EntityHealth health = other.GetComponent<EntityHealth>();
            
            if(health)
                health.Damage(5000000F, default);
        }
    }
}
