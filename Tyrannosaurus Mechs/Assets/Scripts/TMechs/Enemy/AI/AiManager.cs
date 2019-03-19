using System.Linq;
using UnityEngine;

namespace TMechs.Enemy.AI
{
    public class AiManager : MonoBehaviour
    {
        private AiComponent[] components;
        
        private void Awake()
        {
            components = GetComponentsInChildren<AiComponent>().OrderBy(x => x.aiIndex).ToArray();
        }

        private void Update()
        {
            foreach (AiComponent component in components)
            {
                if (component.CanExecute())
                {
                    component.OnAi();
                    return;
                }
            }
        }
    }
}
