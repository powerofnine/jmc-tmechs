using System.Collections.Generic;
using InspectorGadgets.Attributes;
using UnityEngine;

namespace TMechs.Environment
{
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class Ruler : MonoBehaviour
    {
        [Readonly]
        public float distance;
        
        [Header("Render")]
        public float controlSize = 2F;
        
        public bool drawLine = true;
        public SplineCollider.Locks locks = new SplineCollider.Locks(){lockY = true};

        [Space]
        public List<Vector3> points = new List<Vector3>() {Vector3.right * 2.5F, Vector3.right * 10F};

        private void Awake()
        {
            #if !UNITY_EDITOR
            Debug.LogWarning("Warning: removing ruler object")
            Destroy(this);
            #endif
        }

        private void Update()
        {
            distance = 0F;

            for (int i = 0; i < points.Count - 1; i++)
                distance += Vector3.Distance(points[i], points[i + 1]);
        }

        private void OnDrawGizmos()
        {
            if (!drawLine)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.yellow;
            
            for(int i = 1; i < points.Count; i++)
                Gizmos.DrawLine(points[i - 1], points[i]);
        }
    }
}
