using System;
using UnityEngine;

namespace TMechs.FX
{
    public class RotationLock : MonoBehaviour
    {
        private Quaternion rotation;
        
        private void Awake()
        {
            rotation = transform.rotation;
        }

        private void Update()
        {
            transform.rotation = rotation;
        }
    }
}
