using TMechs.UI.GamePad;
using UnityEngine;

namespace TMechs.Player.Behavior
{
    public class BehaviorSprinting : PlayerBehavior
    {
        public override void OnUpdate()
        {
            base.OnUpdate();

            GamepadLabels.AddLabel(IconMap.IconGeneric.L3, "Stop Sprinting", -100);
            if (Input.GetButtonDown(Controls.Action.SPRINT) || player.movement.inputMagnitude <= Mathf.Epsilon)
                player.PopBehavior();
        }

        public override float GetSpeed()
            => player.movement.runSpeed;
    }
}