using JetBrains.Annotations;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TMechs.Player
{
    public class CameraShake : MonoBehaviour
    {
        public new Transform camera;
        public float strength = 2F;
        public float falloff = .25F;
        
        private float shake;

        private void Update()
        {
            if(!camera || Time.timeScale < Mathf.Epsilon)
                return;

            Vector3 cam = camera.localPosition;

            cam.x = Random.Range(0F, shake);
            cam.y = Random.Range(0F, shake);
            
            camera.localPosition = cam;
            
            if (shake >= 0F)
                shake -= falloff * Time.deltaTime;

            shake = Mathf.Max(0F, shake);
            Rumble.SetRumble(Rumble.CHANNEL_CAMERA, Mathf.Clamp01(Utility.MathRemap(shake, 0F, 4F, 0F, 1F)), .1F, 0F);
        }

        [UsedImplicitly] // Animator event
        private void Shake(float multiplier = 1F)
        {
            if (!Player.Instance.forces.IsGrounded)
                return;
                
            shake = strength * multiplier;
        }
    }
}