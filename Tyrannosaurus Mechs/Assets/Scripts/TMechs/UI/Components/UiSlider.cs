using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public class UiSlider : UiComponent
    {
        public Image bar;
        public TextMeshProUGUI percentage;
        public int valueIncrement = 5;

        [ColorUsage(false)]
        public Color lockedMultiply = Color.red;

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
        private bool locked;

        public void SetInstant(int value)
        {
            Value = value;
            UpdateState_Pre(true);
        }
        
        protected override void UpdateState(bool instant)
        {
            if (bar)
                StartCoroutine(TransitionBar(instant));

            if (percentage)
                percentage.text = value + "%";

            highlightMultiply = locked ? lockedMultiply : Color.white;
        
            base.UpdateState(instant);
        }

        private IEnumerator TransitionBar(bool instant)
        {
            float progress = 0F;
            float fillValue = bar.fillAmount;

            while (progress < 1F)
            {
                if (instant)
                    progress = 1F;
                
                progress += Time.deltaTime * 6F;
                bar.fillAmount = Mathf.Lerp(fillValue, value / 100F, progress);

                yield return null;
            }
        }

        public override void OnSubmit()
        {
            base.OnSelect();

            locked = true;
            UpdateState_Pre();
        }

        public override bool OnCancel()
        {
            if (locked)
            {
                locked = false;
                UpdateState_Pre();
                return false;
            }

            return base.OnCancel();
        }

        public override bool NavigateLeft()
        {
            if(locked)
                Value -= valueIncrement;
            
            return locked;
        }

        public override bool NavigateRight()
        {
            if(locked)
                Value += valueIncrement;
            
            return locked;
        }
    }
}