using JetBrains.Annotations;
using UnityEngine;

namespace TMechs
{
    public class AnimatorEventListener : MonoBehaviour
    {
        [UsedImplicitly]
        public void OnAnimationEvent(string id)
        {
            foreach (IAnimatorEvent ev in GetComponentsInParent<IAnimatorEvent>())
                ev.OnAnimationEvent(id);
        }

        public interface IAnimatorEvent
        {
            void OnAnimationEvent(string id);
        }
    }
}