using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public abstract class UiSelectable : UiComponent
    {
        [ColorUsage(false)]
        public Color lockedMultiply = Color.red;

        private bool locked;

        protected override void UpdateState(bool instant)
        {
            highlightMultiply = locked ? lockedMultiply : Color.white;
        
            base.UpdateState(instant);
        }

        public override void OnSubmit()
        {
            base.OnSelect();

            locked = !locked;
            UpdateState_Pre();
        }

        public override bool OnCancel()
        {
            if (locked)
            {
                locked = false;
                UpdateState_Pre();
                return true;
            }

            return base.OnCancel();
        }

        protected override void OnDeselect()
        {
            base.OnDeselect();

            locked = false;
        }

        public override bool NavigateLeft() => locked ? DirectionPressed(Direction.Left) : locked;
        public override bool NavigateRight() => locked ? DirectionPressed(Direction.Right) : locked;
        public override bool NavigateUp() => locked ? DirectionPressed(Direction.Up) : locked;
        public override bool NavigateDown() => locked ? DirectionPressed(Direction.Down) : locked;

        public abstract bool DirectionPressed(Direction dir);

        public enum Direction
        {
            Up,
            Down,
            Left,
            Right
        }
    }
}