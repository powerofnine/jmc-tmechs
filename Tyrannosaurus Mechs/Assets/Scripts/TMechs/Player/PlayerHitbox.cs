using UnityEngine;

namespace TMechs.Player
{
    public class PlayerHitbox : MonoBehaviour
    {
        public string id;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
            => Player.Instance.combat.OnHitbox(id, other);
    }
}