using UnityEngine;

namespace TMechs.Environment
{
    public class LevelInfo : MonoBehaviour
    {
        public static LevelInfo Instance { get; private set; }

        public string levelName;

        private void Awake()
        {
            Instance = this;
        }
    }
}
