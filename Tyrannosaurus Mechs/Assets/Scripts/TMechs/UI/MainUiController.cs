using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class MainUiController : MonoBehaviour
    {
        public Image health;
        public RectTransform charge;

        private void Update()
        {
            Player.Player player = Player.Player.Instance;
            health.fillAmount = player.Health;
            charge.localEulerAngles = -charge.localEulerAngles.Set(Mathf.Lerp(0F, 180F, player.Combat.rocketFistCharge / player.Combat.rocketFistChargeMax), Utility.Axis.Z);
        }
    }
}
