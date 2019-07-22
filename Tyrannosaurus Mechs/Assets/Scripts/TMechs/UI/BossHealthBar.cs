using fuj1n.MinimalDebugConsole;
using TMechs.Entity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class BossHealthBar : MonoBehaviour
    {
        public static EntityHealth activeHealthBar;
        public static string text;

        [SerializeField]
        private Image bar;
        [SerializeField]
        private TextMeshProUGUI label;

        private CanvasGroup group;
        private float alphaVelocity;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            group.alpha = 0F;
        }

        private void Update()
        {
            if (activeHealthBar)
            {
                if (label)
                    label.text = text;

                bar.fillAmount = activeHealthBar.Health;
            }

            group.alpha = Mathf.SmoothDamp(group.alpha, activeHealthBar ? 1F : 0F, ref alphaVelocity, .5F);
        }

        [DebugConsoleCommand("bossbar")]
        private static void BossBarDebug()
        {
            activeHealthBar = Player.Player.Instance.Health;
            text = "Tyrannosaurus Mechs";
        }
    }
}