using TMechs.Environment.Targets;
using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class RocketFist : StateMachineBehaviour
    {
        public GameObject template;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            EnemyTarget target = TargetController.Instance.GetTarget<EnemyTarget>();

            if (!target)
            {
                animator.SetTrigger(Anim.ROCKET_RETURN);
                return;
            }

            Entity.RocketFist rf = Instantiate(template).GetComponent<Entity.RocketFist>();
            rf.damage = Player.Instance.Combat.RocketFistDamage;
            rf.target = target.transform;
            
            animator.transform.Find("T_MechsProto_RocketFist").gameObject.SetActive(false);
        }
        
        private void RocketFistBegin()
        {
            
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            base.OnStateExit(animator, animatorStateInfo, layerIndex);
            
            animator.transform.Find("T_MechsProto_RocketFist").gameObject.SetActive(true);
        }
    }
}
