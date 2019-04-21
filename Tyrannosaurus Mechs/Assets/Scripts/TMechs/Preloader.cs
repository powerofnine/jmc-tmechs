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
            GameObject preloadObject = Resources.Load<GameObject>("PRELOAD");

            if (preloadObject)
            {
                preload = Instantiate(preloadObject);
                preload.AddComponent<Preloader>();
            }
        }

        private void Start()
        {
            Invoke(nameof(Remove), 1F);
        }

        private void Remove() => Destroy(gameObject);
    }
}
