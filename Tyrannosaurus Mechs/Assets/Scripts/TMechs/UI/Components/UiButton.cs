using UIEventDelegate;

namespace TMechs.UI.Components
{
    public class UiButton : UiComponent
    {
        public ReorderableEventList onClick;

        public override void OnSubmit()
        {
            base.OnSubmit();

            if (onClick != null)
                EventDelegate.Execute(onClick.List);
        }
    }
}