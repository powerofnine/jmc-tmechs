using UnityEngine;

namespace TMechs.Enemy
{
    public class TankyloRock : MonoBehaviour
    {
        public int damage = 15;

        public GameObject effect;
        
        private void OnCollisionEnter(Collision other)
        {
            if(other.collider.CompareTag("Player"))
                Player.Player.Instance.Damage(damage);

            if (effect)
                Instantiate(effect, transform.position, transform.rotation);
            
            Destroy(gameObject);
        }
    }
}
