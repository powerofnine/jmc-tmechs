using System;
using System.Collections.Generic;
using TMechs.Environment.Interactables;
using TMechs.Player.Behavior;
using TMechs.UI.GamePad;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class InteractionModule : PlayerBehavior
    {
        private readonly List<Interactable> interactables = new List<Interactable>();
        
        public void AddInteraction(Interactable interactable)
        {
            interactables.Add(interactable);
        }

        public void ProcessInteractions()
        {
            if (interactables.Count <= 0)
                return;
            
            GamepadLabels.AddLabel(IconMap.Icon.ActionTopRow2, interactables[0].displayText);
        }
    }
}
