using TMechs.Animation;
using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        [AnimationCollection.Enum("Player Animations")]
        public enum PlayerAnims
        {
            [Header("Basic")]
            Idle,
            Death,
        
            [Header("Movement")]
            Walk,
            Run,
            Dash,
            Jump,
            AirJump,
        
            [Header("Attack String")]
            Attack1,
            Attack2,
            Attack3,
            
            [Header("Grapple")]
            GrabObject,
            ThrowObject,
            Grapple,
        
            [Header("Rocket Fist")]
            RocketChargeIntro,
            RocketCharge,
            RocketHold,
            RocketRecover,
            RocketReturn
        }
    }
}
