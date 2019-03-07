using UnityEngine;

namespace TMechs.Player
{
    public class Player : MonoBehaviour
    {
        public static Player Instance { get; private set; }
        public Animator Animator { get; private set; }

        [Header("Anchors")]
        public Transform rocketFistAnchor;

        private void Awake()
        {
            Instance = this;

            Animator = GetComponent<Animator>();
        }
    }
}
