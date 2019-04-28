using System;
using UnityEngine;

namespace TMechs.Types
{
    [Serializable]
    public struct Radius
    {
        public float radius;
        public bool visualize;
        [ColorUsage(false)]
        public Color visualizeColor;

        public Radius(float radius) : this()
            => this.radius = radius;
        
        public static implicit operator float(Radius r)
            => r.radius;
    }
}