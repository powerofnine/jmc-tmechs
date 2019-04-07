﻿using UnityEngine;
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

        public Image[] bars;

        private void UpdateRender()
        {
            if (bars == null || bars.Length == 0)
                return;

            float fillValue = bars.Length * fillAmount;

            foreach (Image bar in bars)
            {
                if (fillValue >= 1F)
                {
                    if (bar)
                        bar.fillAmount = 1F;
                    fillValue -= 1F;
                }
                else
                {
                    if (bar)
                        bar.fillAmount = fillValue;
                    fillValue = 0F;
                }
            }
        }

#if UNITY_EDITOR
        private void OnValidate() => UpdateRender();
#endif
    }
}