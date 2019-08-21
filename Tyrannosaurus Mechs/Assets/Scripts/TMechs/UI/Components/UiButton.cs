using UIEventDelegate;
using UltEvents;

namespace TMechs.UI.Components
{
    public class UiButton : UiComponent
    {
        public UltEvent onClickNew;
        public ReorderableEventList onClick;

        public override void OnSubmit()
        {
            base.OnSubmit();

            if (onClick != null)
                EventDelegate.Execute(onClick.List);
            onClickNew.InvokeX();
        }
    }
}