using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public abstract class UiComponent : UIBehaviour, IPointerClickHandler
    {
        public Graphic highlight;
        [HideInInspector]
        public ValueChangedEvent onValueChange;

        [NonSerialized]
        public Color highlightMultiply = Color.white;

        public bool IsSelected { get; private set; }

        public virtual void OnSubmit()
        {
        }

        public virtual bool OnCancel() => false;
        public virtual bool NavigateUp() => false;
        public virtual bool NavigateDown() => false;
        public virtual bool NavigateLeft() => false;
        public virtual bool NavigateRight() => false;

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            OnSubmit();
        }

        public void OnSelect(bool instantEffect)
        {
            IsSelected = true;
            OnSelect();
            UpdateState_Pre(instantEffect);
        }

        public void OnDeselect(bool instantEffect)
        {
            IsSelected = false;
            OnDeselect();
            UpdateState_Pre(instantEffect);
        }

        protected virtual void OnSelect()
        {
        }

        protected virtual void OnDeselect()
        {
        }

        protected void NotifyValueChange()
        {
            if (onValueChange != null)
                onValueChange.Invoke(this);
        }

        protected void UpdateState_Pre(bool instant = false)
        {
            StopAllCoroutines();
            UpdateState(instant);
        }

        protected virtual void UpdateState(bool instant)
        {
            Transition(highlight, IsSelected, instant);
        }

        protected void Transition(Graphic g, bool value, bool instant)
        {
            StartCoroutine(DoTransition(g, value, instant));
        }

        private IEnumerator DoTransition(Graphic g, bool visible, bool instant)
        {
            float transition = 0F;
            Color startColor = g ? g.canvasRenderer.GetColor() : default;

            Color targetColor = highlightMultiply;
            targetColor.a = visible ? 1F : 0F;

            while (g && transition < 1F)
            {
                transition += Time.unscaledDeltaTime * 4F;
                if (instant)
                    transition = 1F;

                g.canvasRenderer.SetColor(Color.Lerp(startColor, targetColor, transition));

                yield return null;
            }
        }

        protected override void OnEnable()
        {
            UpdateState_Pre(true);
        }

        [Serializable]
        public class ValueChangedEvent : UnityEvent<object>
        {
        }
    }
}