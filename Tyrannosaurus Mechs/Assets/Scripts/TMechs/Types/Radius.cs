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

        public bool renderAsLine;

        public Radius(float radius, bool renderAsLine = false) : this()
        {
            this.radius = radius;
            this.renderAsLine = renderAsLine;
        }

        public static implicit operator float(Radius r)
            => r.radius;
    }
}