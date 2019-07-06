using System.Collections;
using UnityEngine;

namespace TMechs.UI
{
    public class MainUiController : MonoBehaviour
    {
        public HealthBarCollection health;
        public RectTransform charge;

        public float fadeTime = .5F;

        private bool isEnabled = true;

        private CanvasGroup group;

        private void Awake()
        {
            group = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            Player.Player player = Player.Player.Instance;
            health.FillAmount = player.Health.Health;
            
            //TODO update when rocket fist readded
            //charge.localEulerAngles = -charge.localEulerAngles.Set(Mathf.Lerp(0F, 180F, player.Combat.rocketFistCharge / player.Combat.rocketFistChargeMax), Utility.Axis.Z);

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