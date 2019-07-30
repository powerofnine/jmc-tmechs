using UnityEngine;

namespace TMechs.Player.FX
{
    public class DiegeticHealth : MonoBehaviour
    {
        [GradientUsage(true)]
        public Gradient gradient = new Gradient();

        public string property = "_EmissionColor";

        private Renderer render;

        private float cachedHealth = 1F;
        private float healthVelocity;
        
        private void Awake()
        {
            render = GetComponent<Renderer>();
            render.material.SetColor(property, gradient.Evaluate(cachedHealth));
        }

        private void Update()
        {
            if (Mathf.Abs(cachedHealth - Player.Instance.Health.Health) > Mathf.Epsilon)
            {
                cachedHealth = Mathf.SmoothDamp(cachedHealth, Player.Instance.Health.Health, ref healthVelocity, .2F);

                render.material.SetColor(property, gradient.Evaluate(cachedHealth));
            }
        }
    }
}
