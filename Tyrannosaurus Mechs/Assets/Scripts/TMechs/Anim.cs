using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace TMechs
{
    public static class Anim
    {
        public static readonly int PLAYER_SPEED = Hash("Player Speed");
        public static readonly int HAS_ENEMY = Hash("Has Enemy");
        public static readonly int HAS_GRAPPLE = Hash("Has Grapple");
        public static readonly int DASH = Hash("Dash");
        public static readonly int ATTACK = Hash("Attack");
        public static readonly int ATTACK_HELD = Hash("Attack Held");
        public static readonly int LEFT_ARM_HELD = Hash("Left Arm Held");
        public static readonly int RIGHT_ARM = Hash("Right Arm");
        public static readonly int RIGHT_ARM_HELD = Hash("Right Arm Held");
        public static readonly int PICKUP_TARGET_TYPE = Hash("Pickup Target Type");
        public static readonly int MOVE_DELTA = Hash("MoveDelta");

        public static readonly int GROUNDED = Hash("Grounded");

        public static readonly int IS_CARRYING = Hash("Is Carrying");
        public static readonly int IS_CARRYING_ATTACKABLE = Hash("Is Carrying Attackable");

        public static readonly int JUMP = Hash("Jump");
        public static readonly int AIR_JUMP = Hash("AirJump");
        public static readonly int DIE = Hash("Die");
        public static readonly int HIT = Hash("Hit");

        public static readonly int ROCKET_RETURN = Hash("Rocket Fist Return");
        public static readonly int ROCKET_OVERCHARGE = Hash("Rocket Fist Overcharge");
        public static readonly int ROCKET_READY = Hash("Rocket Fist Ready");
        
        public static readonly int GRAPPLE_END = Hash("Grapple End");

        public static readonly Dictionary<int, string> RAINBOW = new Dictionary<int, string>
        {
                {Hash("Rocket Fist"), "Rocket Fist"},
                {Hash("Rocket Fist Charge"), "Rocket Fist Charge"},
                {Hash("Rocket Fist Intro"), "Rocket Fist Intro"}
        };

        [Pure]
        public static int Hash(string s)
            => Animator.StringToHash(s);
    }
}