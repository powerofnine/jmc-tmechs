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
        public static readonly int GRAPPLE = Animator.StringToHash("Grapple");
        public static readonly int GRAPPLE_DOWN = Animator.StringToHash("Grapple Down");
        public static readonly int PICKUP_TARGET_TYPE = Animator.StringToHash("Pickup Target Type");
        public static readonly int MOVE_DELTA = Animator.StringToHash("MoveDelta");
    }
}