using System.Linq;
using UnityEngine;
using UnityEngine.Playables;

namespace TMechs.Enemy.AI
{
    public class AiManager : MonoBehaviour
    {
        private AiComponent[] components;
        private PlayableGraph ai;
        
        private void Awake()
        {
            ai = PlayableGraph.Create("Enemy AI " + name);
            GraphVisualizerClient.Show(ai);
            
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
