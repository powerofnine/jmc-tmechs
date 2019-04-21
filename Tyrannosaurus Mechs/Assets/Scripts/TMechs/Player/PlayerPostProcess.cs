using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace TMechs.Player
{
    public class PlayerPostProcess : MonoBehaviour
    {
        public static PlayerPostProcess Instance { get; private set; }

        public PostProcessVolume ui;
        
        private void Awake()
        {
            Instance = this;
        }
    }
}
