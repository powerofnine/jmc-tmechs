using System;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs
{
    [PublicAPI]
    public static class Utility
    {
        /// <summary>
        /// Isolates the given <paramref name="axis"/>
        /// </summary>
        /// <param name="source">Source Vector</param>
        /// <param name="axis">The axis to keep</param>
        /// <returns><paramref name="source"/>, but where the unspecified axis are removed</returns>
        public static Vector3 Isolate(this Vector3 source, Axis axis)
        {
            if (!axis.HasFlag(Axis.X))
                source.x = 0;
            if (!axis.HasFlag(Axis.Y))
                source.y = 0;
            if (!axis.HasFlag(Axis.Z))
                source.z = 0;

            return source;
        }

        /// <summary>
        /// Calls <see cref="Isolate(Vector3, Axis)"/> with an inverted <paramref name="axis"/>
        /// </summary>
        /// <param name="source">Source Vector</param>
        /// <param name="axis">The axis to remove</param>
        /// <returns><paramref name="source"/>, but where the specified axis are removed</returns>
        public static Vector3 Remove(this Vector3 source, Axis axis)
        {
            return Isolate(source, ~axis);
        }

        /// <summary>
        /// Clamps each component of the given vector accordingly
        /// </summary>
        /// <param name="source">The vector to clamp</param>
        /// <param name="min">The minimum values for each component</param>
        /// <param name="max">The maximum values for each component</param>
        /// <returns><paramref name="source"/> with components clamped to given values</returns>
        public static Vector3 Clamp(this Vector3 source, Vector3 min, Vector3 max)
        {
            source.x = Mathf.Clamp(source.x, min.x, max.x);
            source.y = Mathf.Clamp(source.y, min.y, max.y);
            source.z = Mathf.Clamp(source.z, min.z, max.z);

            return source;
        }

        /// <summary>
        /// Clamps each component of the given vector accordingly
        /// </summary>
        /// <param name="source">The vector to clamp</param>
        /// <param name="min">The minimum value for all components</param>
        /// <param name="max">The maximum value for all components</param>
        /// <returns><paramref name="source"/> with components clamped to given values</returns>
        public static Vector3 Clamp(this Vector3 source, float min, float max)
        {
            return Clamp(source, Vector3.one * min, Vector3.one * max);
        }

        /// <summary>
        /// Clamps the X component of the given vector accordingly
        /// </summary>
        /// <param name="source">The vector to clamp</param>
        /// <param name="min">The minimum value of X</param>
        /// <param name="max">The maximum value of X</param>
        /// <returns><paramref name="source"/> with X clamped to given values</returns>
        public static Vector3 ClampX(this Vector3 source, float min, float max)
        {
            source.x = Mathf.Clamp(source.x, min, max);

            return source;
        }

        /// <summary>
        /// Clamps the Y component of the given vector accordingly
        /// </summary>
        /// <param name="source">The vector to clamp</param>
        /// <param name="min">The minimum value of Y</param>
        /// <param name="max">The maximum value of Y</param>
        /// <returns><paramref name="source"/> with Y clamped to given values</returns>
        public static Vector3 ClampY(this Vector3 source, float min, float max)
        {
            source.y = Mathf.Clamp(source.y, min, max);

            return source;
        }

        /// <summary>
        /// Clamps the Z component of the given vector accordingly
        /// </summary>
        /// <param name="source">The vector to clamp</param>
        /// <param name="min">The minimum value of Z</param>
        /// <param name="max">The maximum value of Z</param>
        /// <returns><paramref name="source"/> with Z clamped to given values</returns>
        public static Vector3 ClampZ(this Vector3 source, float min, float max)
        {
            source.z = Mathf.Clamp(source.z, min, max);

            return source;
        }

        /// <summary>
        /// Remaps the vector components based on the given pattern
        /// </summary>
        /// <param name="source">The vector to remap</param>
        /// <param name="x">The component to make the x component</param>
        /// <param name="y">The component to make the y component</param>
        /// <param name="z">The component to make the z component</param>
        /// <returns>Remapped vector</returns>
        public static Vector3 Remap(this Vector3 source, Axis x, Axis y, Axis z)
        {
            return new Vector3(source.GetComponent(x), source.GetComponent(y), source.GetComponent(z));
        }

        /// <summary>
        /// Calls <see cref="Remap"/> with X, 0, Y
        /// </summary>
        /// <param name="source">The vector to remap</param>
        /// <returns>Remapped vector</returns>
        // ReSharper disable once InconsistentNaming
        public static Vector3 RemapXZ(this Vector2 source)
        {
            return Remap(source, Axis.X, 0, Axis.Y);
        }

        /// <summary>
        /// Sets the vector values and returns it
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="value"></param>
        /// <param name="axis"></param>
        public static Vector3 Set(this Vector3 vector, float value, Axis axis)
        {
            if (axis.HasFlag(Axis.X))
                vector.x = value;
            if (axis.HasFlag(Axis.Y))
                vector.y = value;
            if (axis.HasFlag(Axis.Z))
                vector.z = value;

            return vector;
        }
        
        private static float GetComponent(this Vector3 source, Axis component)
        {
            switch (component)
            {
                case Axis.X:
                    return source.x;
                case Axis.Y:
                    return source.y;
                case Axis.Z:
                    return source.z;
                default:
                    return 0F;
            }
        }

        [Flags]
        public enum Axis
        {
            X = 1,
            Y = 2,
            Z = 4
        }
    }
}