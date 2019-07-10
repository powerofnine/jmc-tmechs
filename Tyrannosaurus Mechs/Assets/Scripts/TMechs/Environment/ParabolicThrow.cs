using System;
using UnityEngine;

namespace TMechs.Environment
{
    public class ParabolicThrow : MonoBehaviour
    {
        public Vector3 target;
        public float inAngle = 45F;
        public float outAngle = 45F;
        [Range(0F, .5F)]
        public float trajectory = .25F;
        public float speed = 20F;

        public Action onEnd;

        private float progress;
        private float length;
        private Vector3[] cvs;
        
        #if UNITY_EDITOR
        public Mesh previewMesh;
        private float previewProgress;
        #endif

        private void Awake()
        {
            onEnd = () => Destroy(gameObject);
        }

        private void Start()
        {
            SetupCvs();
        }

        private void Update()
        {
            progress += speed * Time.deltaTime;

            transform.position = ComputeCurve(Mathf.Clamp01(progress / length));

            if (progress >= length)
            {
                Destroy(this);
                onEnd?.Invoke();
            }
        }

        public void SetupCvs()
        {
            Vector3 start = transform.position;
            
            Vector3 heading = target - start;
            float distance = heading.magnitude;
            Vector3 direction = heading / distance;

            cvs = new[]
            {
                    start,
                    start + distance * trajectory * direction + Vector3.up * inAngle / 2F,
                    target - distance * trajectory * direction + Vector3.up * outAngle / 2F,
                    target
            };

            length = ComputeLength();
        }
        
        public Vector3 ComputeCurve(float t)
        {
            return Mathf.Pow(1 - t, 3) * cvs[0] + 3 * t * Mathf.Pow(1 - t, 2) * cvs[1] + 3 * Mathf.Pow(t, 2) * (1 - t) * cvs[2] + Mathf.Pow(t, 3) * cvs[3];
        }

        public float ComputeLength()
        {
            float chord = (cvs[3] - cvs[0]).magnitude;
            float contNet = (cvs[0] - cvs[1]).magnitude + (cvs[2] - cvs[1]).magnitude + (cvs[3] - cvs[2]).magnitude;

            return (contNet + chord) / 2F;
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            previewProgress += speed * Time.deltaTime;
            
            SetupCvs();
            float length = ComputeLength();

            Gizmos.color = Color.red;
            foreach (Vector3 cv in cvs)
                Gizmos.DrawSphere(cv, .5F);
            
            Gizmos.color = Color.green;

            if (previewMesh)
                Gizmos.DrawWireMesh(previewMesh, 0, ComputeCurve(Mathf.Clamp01(previewProgress / length)));

            for (int i = 0; i < 20; i++)
                Gizmos.DrawSphere(ComputeCurve(i / 20F), .1F);

            if (previewProgress > length)
                previewProgress = 0F;
        }
        #endif
    }
}
