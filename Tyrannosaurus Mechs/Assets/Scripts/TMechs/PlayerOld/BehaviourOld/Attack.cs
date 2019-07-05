using UnityEngine;

namespace TMechs.PlayerOld.BehaviourOld
{
    public class Attack : StateMachineBehaviour
    {
        public string hitboxId;
        public float damage;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);

            if (string.IsNullOrWhiteSpace(hitboxId))
                Debug.LogWarningFormat("No HitBox ID specified for {0}, nothing will happen", stateInfo.shortNameHash);

            Player.Instance.Combat.SetHitbox(hitboxId, damage);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            Player.Instance.Combat.SetHitbox(null, 0F);
        }
    }
}