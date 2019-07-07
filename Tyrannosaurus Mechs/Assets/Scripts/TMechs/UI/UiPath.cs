using System;
using UnityEngine;

namespace TMechs.UI
{
    public class UiPath : MonoBehaviour
    {
        public float Value
        {
            get => value;
            set
            {
                this.value = Mathf.Clamp01(value);
                UpdateValue();
            }
        }

        [SerializeField]
        [Range(0F, 1F)]
        private float value;

        [Min(2)]
        public int steps = 2;
        public Vector2[] positions;
        public float[] rotations;

        private void UpdateValue()
        {
            float progress = (steps - 1) * value;

            int thisStep = Mathf.Clamp(Mathf.FloorToInt(progress), 0, steps - 1);
            int nextStep = Mathf.Clamp(thisStep + 1, 0, steps - 1);

            Vector2 pos = Vector2.Lerp(positions[thisStep], positions[nextStep], progress % 1F);
            float rot = Mathf.LerpAngle(rotations[thisStep], rotations[nextStep], progress % 1F);

            RectTransform rect = (RectTransform) transform;

            if (rect)
                rect.anchoredPosition = pos;

            transform.localEulerAngles = transform.localEulerAngles.Set(rot, Utility.Axis.Z);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (steps < 2)
                steps = 2;

            if (positions == null)
                positions = new Vector2[steps];
            if (rotations == null)
                rotations = new float[steps];

            if (positions.Length != steps)
                Array.Resize(ref positions, steps);
            if (rotations.Length != steps)
                Array.Resize(ref rotations, steps);

            UpdateValue();
        }
#endif
    }
}