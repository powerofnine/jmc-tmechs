using UnityEngine;

namespace TMechs.Enemy.AI
{
    public abstract class AiComponent : MonoBehaviour
    {
        public int aiIndex = 0;
        
        public abstract void OnAi();
        public abstract bool CanExecute();
    }
}
