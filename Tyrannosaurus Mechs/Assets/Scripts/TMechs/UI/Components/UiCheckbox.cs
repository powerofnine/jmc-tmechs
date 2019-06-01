using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public class UiCheckbox : UiComponent
    {
        [FormerlySerializedAs("check")]
        public Graphic checkmarkGraphic;

        public bool IsChecked
        {
            get => isChecked;
            set
            {
                isChecked = value;
                UpdateState_Pre();
                NotifyValueChange();
            }
        }

        [SerializeField]
        private bool isChecked;

        public override void OnSubmit()
        {
            base.OnSubmit();
            IsChecked = !IsChecked;
        }

        public void SetInstant(bool value)
        {
            IsChecked = value;
            UpdateState_Pre(true);
        }

        protected override void UpdateState(bool instant)
        {
            base.UpdateState(instant);

            Transition(checkmarkGraphic, IsChecked, instant);
        }
    }
}