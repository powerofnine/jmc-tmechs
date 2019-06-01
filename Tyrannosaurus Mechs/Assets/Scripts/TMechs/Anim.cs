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
        public static readonly int ANGERY = Hash("ANGERY");
        public static readonly int DASH = Hash("Dash");
        public static readonly int ATTACK = Hash("Attack");
        public static readonly int ATTACK_HELD = Hash("Attack Held");
        public static readonly int GRAPPLE = Hash("Grapple");
        public static readonly int GRAPPLE_DOWN = Hash("Grapple Down");
        public static readonly int PICKUP_TARGET_TYPE = Hash("Pickup Target Type");
        public static readonly int MOVE_DELTA = Hash("MoveDelta");

        public static readonly int GROUNDED = Hash("Grounded");

        public static readonly int JUMP = Hash("Jump");
        public static readonly int AIR_JUMP = Hash("AirJump");
        public static readonly int DIE = Hash("Die");
        public static readonly int HIT = Hash("Hit");

        public static readonly int ROCKET_RETURN = Hash("Rocket Fist Return");
        public static readonly int ROCKET_OVERCHARGE = Hash("Rocket Fist Overcharge");
        public static readonly int ROCKET_READY = Hash("Rocket Fist Ready");

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