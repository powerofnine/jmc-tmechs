using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMechs.Environment
{
    [DisallowMultipleComponent]
    public class SplineCollider : MonoBehaviour
    {
        public float girth = 20F;
        public float height = 500000F;
        
        public float controlSize = 100F;
        
        public bool drawLine = true;
        public Locks locks = new Locks(){lockY = true};

        [Header("Collider")]
        public bool isTrigger;
        public PhysicMaterial material;
        public bool addKillbox = true;
        
        [Space]
        public List<Vector3> points = new List<Vector3>() {Vector3.right * 2.5F, Vector3.right * 10F};
        
        private void Awake()
        {
            for (int i = 1; i < points.Count; i++)
            {
                Vector3 mid = (points[i] + points[i - 1]) / 2F;
                
                GameObject col = new GameObject($"Box{i}");

                if (addKillbox)
                    col.AddComponent<Killbox>();
                    
                col.transform.SetParent(transform);
                col.transform.localPosition = mid;

                BoxCollider box = col.AddComponent<BoxCollider>();
                box.isTrigger = isTrigger;
                box.sharedMaterial = material;
                
                Vector3 size;
                size.x = Vector3.Distance(points[i], points[i - 1]);
                size.y = height;
                size.z = girth;
                box.size = size;

                Vector3 worldA = transform.TransformPoint(points[i - 1].Remove(Utility.Axis.Y));
                Vector3 worldB = transform.TransformPoint(points[i].Remove(Utility.Axis.Y));

                col.transform.eulerAngles = col.transform.eulerAngles.Set(Mathf.Atan2(worldA.x - worldB.x, worldA.z - worldB.z) * Mathf.Rad2Deg + 90F, Utility.Axis.Y);
            }
        }

        private void OnDrawGizmos()
        {
            if (!drawLine)
                return;
            
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.green;
            
            for(int i = 1; i < points.Count; i++)
                Gizmos.DrawLine(points[i - 1], points[i]);
        }
        
        [Serializable]
        public struct Locks
        {
            public bool lockEditor;
            public bool lockX;
            public bool lockY;
            public bool lockZ;
        }
    }
}
