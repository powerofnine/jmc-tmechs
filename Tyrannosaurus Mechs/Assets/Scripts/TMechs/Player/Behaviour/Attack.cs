using UnityEngine;

namespace TMechs.Player.Behaviour
{
    public class Attack : StateMachineBehaviour
    {
        public string hitboxId;
        public float damage;

        private PlayerCombat combat;
        
        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if(!combat)
                combat = animator.GetComponent<PlayerCombat>();

            if(string.IsNullOrWhiteSpace(hitboxId))
                Debug.LogWarningFormat("No HitBox ID specified for {0}, nothing will happen", stateInfo.shortNameHash);
            
            combat.SetHitbox(hitboxId, damage);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            combat.SetHitbox(null, 0F);
        }
    }
}
