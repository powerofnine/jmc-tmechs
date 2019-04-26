using System.Linq;
using UnityEngine;

namespace TMechs.Enemy.Behaviour
{
    public class EnemyAttack : StateMachineBehaviour
    {
        public string hitboxId;
        public int damage;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            
            foreach (EnemyHitBox hitbox in animator.GetComponentsInChildren<EnemyHitBox>().Where(x => x.id.Equals(hitboxId)))
            {
                hitbox.damage = damage;
                hitbox.gameObject.SetActive(true);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            
            foreach (EnemyHitBox hitbox in animator.GetComponentsInChildren<EnemyHitBox>().Where(x => x.id.Equals(hitboxId)))
            {
                hitbox.damage = 0;
                hitbox.gameObject.SetActive(false);
            }
        }
    }
}