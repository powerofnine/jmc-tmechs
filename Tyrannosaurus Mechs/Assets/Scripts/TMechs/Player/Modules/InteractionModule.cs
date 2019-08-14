using System;
using System.Collections.Generic;
using System.Linq;
using TMechs.Environment.Interactables;
using TMechs.Player.Behavior;
using TMechs.UI.GamePad;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class InteractionModule : PlayerModule
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
            
            GamepadLabels.EnableLabel(GamepadLabels.ButtonLabel.Action, interactables[0].displayText);
            interactables[0].OnInteractAvailable();

            if (Player.Input.GetButtonDown(Controls.Action.INTERACT))
            {
                Interactable interactable = interactables.OrderByDescending(x => x.GetSortPriority()).First();
                interactable.OnInteract();

                PlayerBehavior beh = interactable.GetPushBehavior();
                if(beh != null)
                    player.PushBehavior(beh);
            }
            
            interactables.Clear();
        }
    }
}
