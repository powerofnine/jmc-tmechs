using UnityEngine;

namespace TMechs.FX
{
    public class SineBob : MonoBehaviour
    {
        public float amplitude = 1F;
        public float frequency = 1F;

        public Vector3 min;
        public Vector3 max;
        
        public bool randomizeStartingAngle = true;
        
        private float angle = 0F;

        private Vector3 neutralPosition;

        private void Awake()
        {
            if (randomizeStartingAngle)
                angle = Random.Range(0F, 2 * Mathf.PI);

            neutralPosition = transform.localPosition;
        }

        private void Update()
        {
            angle += Time.deltaTime;
            float alpha = Mathf.Clamp01(Mathf.Sin(angle * frequency) * amplitude);

            transform.localPosition = neutralPosition + Vector3.Lerp(min, max, alpha);

            if (angle > 2 * Mathf.PI)
                angle -= 2 * Mathf.PI;
        }
    }
}
