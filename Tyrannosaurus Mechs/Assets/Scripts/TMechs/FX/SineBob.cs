using UnityEngine;

namespace TMechs.FX
{
    public class SineBob : MonoBehaviour
    {
        public float frequency = 1F;

        public Vector3 min;
        public Vector3 max;

        public bool randomizeStartingAngle = true;

        private float angle;

        private Vector3 neutralPosition;

        private void Awake()
        {
            if (randomizeStartingAngle)
                angle = Random.Range(0F, 2 * Mathf.PI);

            neutralPosition = transform.localPosition;
        }

        private void Update()
        {
            angle += Time.deltaTime * frequency;
            float alpha = Utility.MathRemap(Mathf.Sin(angle), -1F, 1F, 0F, 1F);

            transform.localPosition = neutralPosition + Vector3.Lerp(min, max, alpha);

            if (angle > 2 * Mathf.PI)
                angle -= 2 * Mathf.PI;
        }
    }
}