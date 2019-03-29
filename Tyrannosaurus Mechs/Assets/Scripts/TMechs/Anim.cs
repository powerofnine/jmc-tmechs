using System.Collections.Generic;
using UnityEngine;

namespace TMechs
{
    public static class Anim
    {
        public static readonly int PLAYER_SPEED = Animator.StringToHash("Player Speed");
        public static readonly int HAS_ENEMY = Animator.StringToHash("Has Enemy");
        public static readonly int HAS_GRAPPLE = Animator.StringToHash("Has Grapple");
        public static readonly int ANGERY = Animator.StringToHash("ANGERY");
        public static readonly int DASH = Animator.StringToHash("Dash");
        public static readonly int ATTACK = Animator.StringToHash("Attack");
        public static readonly int ATTACK_HELD = Animator.StringToHash("Attack Held");
        public static readonly int GRAPPLE = Animator.StringToHash("Grapple");
        public static readonly int GRAPPLE_DOWN = Animator.StringToHash("Grapple Down");
        public static readonly int PICKUP_TARGET_TYPE = Animator.StringToHash("Pickup Target Type");
        public static readonly int MOVE_DELTA = Animator.StringToHash("MoveDelta");
        
        public static readonly int ROCKET_RETURN = Animator.StringToHash("Rocket Fist Return");
        public static readonly int ROCKET_OVERCHARGE = Animator.StringToHash("Rocket Fist Overcharge");
        public static readonly int ROCKET_READY = Animator.StringToHash("Rocket Fist Ready");

        public static readonly Dictionary<int, string> RAINBOW = new Dictionary<int, string>()
        {
                {Animator.StringToHash("Rocket Fist"), "Rocket Fist"},
                {Animator.StringToHash("Rocket Fist Charge"), "Rocket Fist Charge"},
                {Animator.StringToHash("Rocket Fist Intro"), "Rocket Fist Intro"}
        };
    }
}