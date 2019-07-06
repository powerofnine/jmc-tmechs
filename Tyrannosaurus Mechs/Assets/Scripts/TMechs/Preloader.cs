using UnityEngine;

namespace TMechs
{
    [AddComponentMenu("")]
    public class Preloader : MonoBehaviour
    {
        private static GameObject preload;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Initialize()
        {
            GameObject input = Resources.Load<GameObject>("Input");

            if (input)
                Instantiate(input);
            
            GameObject preloadObject = Resources.Load<GameObject>("PRELOAD");

            if (preloadObject)
            {
                preload = Instantiate(preloadObject);
                preload.AddComponent<Preloader>();
            }
        }

        private void Start()
        {
            Destroy(gameObject, 1F);
        }
    }
}