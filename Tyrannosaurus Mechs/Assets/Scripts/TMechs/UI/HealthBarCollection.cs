using System;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class HealthBarCollection : MonoBehaviour
    {
        public float FillAmount
        {
            get => fillAmount;
            set
            {
                fillAmount = Mathf.Clamp01(value);
                UpdateRender();
            }
        }

        [SerializeField]
        [Range(0F, 1F)]
        private float fillAmount;

        public HealthBar[] bars;

        private void UpdateRender()
        {
            if (bars == null || bars.Length == 0)
                return;

            float fillValue = bars.Length * fillAmount;

            foreach (HealthBar bar in bars)
            {
                if (fillValue >= 1F)
                {
                    if (bar.image)
                        bar.image.fillAmount = 1F;
                    if (bar.path)
                        bar.path.Value = 1F;
                    fillValue -= 1F;
                }
                else
                {
                    if (bar.image)
                        bar.image.fillAmount = fillValue;
                    if (bar.path)
                        bar.path.Value = fillValue;
                    fillValue = 0F;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => UpdateRender();
#endif

        [Serializable]
        public struct HealthBar
        {
            public Image image;
            public UiPath path;
        }
    }
}