using UnityEngine;

namespace TMechs.Enemy
{
    public class AnimatorEventListener : MonoBehaviour
    {
        public void OnAnimationEvent(string id)
        {
            foreach(IAnimatorEvent ev in GetComponentsInParent<IAnimatorEvent>())
                ev.OnAnimationEvent(id);
        }

        public interface IAnimatorEvent
        {
            void OnAnimationEvent(string id);
        }
    }
}
