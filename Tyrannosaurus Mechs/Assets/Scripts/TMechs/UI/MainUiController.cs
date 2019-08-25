using System.Collections;
using TMechs.Environment.Targets;
using TMechs.MechsDebug;
using TMechs.Player;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class MainUiController : MonoBehaviour
    {
        public UiPath health;
//        public HealthBarCollection health;

        [Space]
        public CanvasGroup rocketFistReadyFade;
        public Image rocketFistCharge;
        public float rocketFistMinAlpha = .25F;
        public float rocketFistMaxAlpha = 1F;

        public float fadeTime = .5F;

        private bool isEnabled = true;
        
        private CanvasGroup group;

        private float rocketFistAlphaVelocity;
        
        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            Player.Player player = Player.Player.Instance;
            health.Value = player.Health.Health;

            rocketFistCharge.fillAmount = player.rocketFist.rocketFistCharge / player.rocketFist.maxChargeTime;
            
            EnemyTarget enemy = TargetController.Instance.GetTarget<EnemyTarget>();
            bool rocketFistAvailable = player.rocketFist.IsCharging || enemy && player.rocketFist.rocketFistCharge <= Mathf.Epsilon;

            rocketFistReadyFade.alpha = Mathf.SmoothDamp(rocketFistReadyFade.alpha, rocketFistAvailable ? rocketFistMaxAlpha : rocketFistMinAlpha, ref rocketFistAlphaVelocity, .1F);
            
            bool shouldBeEnabled = Time.timeScale > float.Epsilon && !Freecam.HideUi;

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