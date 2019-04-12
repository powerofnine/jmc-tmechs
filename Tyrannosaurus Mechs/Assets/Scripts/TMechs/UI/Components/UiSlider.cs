using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public class UiSlider : UiSelectable
    {
        public Image bar;
        public TextMeshProUGUI percentage;
        public float valueIncrement = .05F;
        
        public float Value
        {
            get => value;
            set
            {
                this.value = Mathf.Clamp(value, minValue, maxValue);
                UpdateState_Pre();
                NotifyValueChange();
            }
        }

        private float value;

        [Space]
        public float minValue = 0F;
        public float maxValue = 1F;

        public delegate string ToValue(float value, float min, float max);
        public ToValue toValue = (val, min, max) => Mathf.RoundToInt((val - min) / (max - min) * 100) + "%";
        
        public void SetInstant(float value)
        {
            Value = value;
            UpdateState_Pre(true);
        }
        
        protected override void UpdateState(bool instant)
        {
            base.UpdateState(instant);
            
            if (bar)
                StartCoroutine(TransitionBar(instant));

            if (percentage && toValue != null)
                percentage.text = toValue(Value, minValue, maxValue);
        }

        private IEnumerator TransitionBar(bool instant)
        {
            float progress = 0F;
            float fillValue = bar.fillAmount;

            while (progress < 1F)
            {
                if (instant)
                    progress = 1F;
                
                progress += Time.unscaledDeltaTime * 6F;
                bar.fillAmount = Mathf.Lerp(fillValue, (value - minValue) / (maxValue - minValue), progress);

                yield return null;
            }
        }

        public override bool DirectionPressed(Direction dir)
        {
            switch (dir)
            {
                case Direction.Left:
                    Value -= valueIncrement;
                    return true;
                case Direction.Right:
                    Value += valueIncrement;
                    return true;
                case Direction.Up:
                case Direction.Down:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(dir), dir, null);
            }

            return false;
        }
    }
}