using UnityEngine;

namespace TMechs.FX
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 axis = new Vector3(45F, 35F, 25F);
        
        public bool randomizeStartRotation;
        public bool useUnscaledTime = false;

        private void Awake()
        {
            transform.Rotate(axis * Random.Range(0F, 360F));
        }

        private void Update()
        {
            transform.Rotate(axis * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime));
        }
    }
}