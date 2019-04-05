using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public class UiSlider : UiSelectable
    {
        public Image bar;
        public TextMeshProUGUI percentage;
        public int valueIncrement = 5;

        public int Value
        {
            get => value;
            set
            {
                this.value = Mathf.Clamp(value, 0, 100);
                UpdateState_Pre();
                NotifyValueChange();
            }
        }

        private int value;

        public void SetInstant(int value)
        {
            Value = value;
            UpdateState_Pre(true);
        }
        
        protected override void UpdateState(bool instant)
        {
            base.UpdateState(instant);
            
            if (bar)
                StartCoroutine(TransitionBar(instant));

            if (percentage)
                percentage.text = value + "%";
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
                bar.fillAmount = Mathf.Lerp(fillValue, value / 100F, progress);

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