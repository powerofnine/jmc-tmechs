using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class MainUiController : MonoBehaviour
    {
        public HealthBarCollection health;
        public RectTransform charge;
        public Image chargeGlow;

        public float fadeTime = .5F;

        private bool isEnabled = true;
        
        private CanvasGroup group;
        
        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
            
            if(chargeGlow)
                chargeGlow.canvasRenderer.SetAlpha(0F);
        }

        private void Update()
        {
            Player.Player player = Player.Player.Instance;
            health.FillAmount = player.Health.Health;
            
            charge.localEulerAngles = charge.localEulerAngles.Set(Mathf.Lerp(0F, -180F, player.rocketFist.rocketFistCharge / player.rocketFist.maxChargeTime), Utility.Axis.Z);

            if (chargeGlow && player.rocketFist.damageColors != null && player.rocketFist.damageColors.Length > 0)
            {
                int stage = player.rocketFist.ChargeStage;
                
                Color c = player.rocketFist.damageColors[stage];
                
                if (player.Behavior != player.rocketFist)
                    c.a = 0F;
                
                chargeGlow.CrossFadeColor(c, .5F, false, true);
            }

            bool shouldBeEnabled = Time.timeScale > float.Epsilon;

            if (shouldBeEnabled != isEnabled)
            {
                isEnabled = shouldBeEnabled;
                StopAllCoroutines();
                StartCoroutine(Fade(shouldBeEnabled ? 1F : 0F));
            }
        }

        private IEnumerator Fade(float to)
        {
            if(!group)
                yield break;

            if (fadeTime <= Mathf.Epsilon)
            {
                group.alpha = to;
                yield break;
            }
            
            float from = group.alpha;
            float timer = 0F;

            while (timer < fadeTime)
            {
                timer += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, timer / fadeTime);
                
                yield return null;
            }
        }
    }
}