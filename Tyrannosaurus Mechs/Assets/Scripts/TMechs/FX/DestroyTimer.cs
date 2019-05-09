using UnityEngine;

namespace TMechs.FX
{
    public class DestroyTimer : MonoBehaviour
    {
        public float time = 5F;
        
        private void Start()
        {
            Destroy(gameObject, time);
        }
    }
}
