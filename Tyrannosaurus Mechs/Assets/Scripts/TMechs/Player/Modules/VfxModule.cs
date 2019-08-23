using System;
using TMechs.FX;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Player.Modules
{
    [Serializable]
    public class VfxModule : PlayerModule
    {
        [Header("References")]
        public VisualEffect sprint;
        public VisualEffect heal;
        public VisualEffect death;
        public VisualEffect dash;
        
        [Space]
        public VisualEffect rocketFistCharge;
        public VisualEffect rocketBurst;
        public VisualEffect rocketShot;
        public VisualEffect rocketOvercharge;
        
        [Space]
        public VisualEffect leftPunchTrail;
        public VisualEffect rightPunchTrail;
        
        [Header("Spawnables")]
        public VisualEffectAsset jump;
        public VisualEffectAsset groundSlam;
        public VisualEffectAsset waterSplash;

        [Header("Anchors")]
        public Transform groundSlamAnchor;
        public Transform leftFootAnchor;
        public Transform rightFootAnchor;

        private float sprintAlpha;
        private float sprintAlphaVel;

        public override void OnUpdate()
        {
            base.OnUpdate();
            
            bool shouldEnableSprint = player.movement.isSprinting && !player.IsInWater && player.forces.IsGrounded && player.forces.ControllerVelocity.magnitude > player.movement.runSpeed / 2F;

            sprintAlpha = Mathf.SmoothDamp(sprintAlpha, shouldEnableSprint ? 1F : 0F, ref sprintAlphaVel, .15F);
            sprint.SetFloat("Alpha", sprintAlpha);
        }

        public static VisualEffect SpawnEffect(VisualEffectAsset asset, Vector3 wsPosition, Quaternion wsOrientation, float time)
        {
            if (!asset)
                return null;
            
            GameObject go = new GameObject($"VFX:{asset.name}");
            go.transform.SetPositionAndRotation(wsPosition, wsOrientation);

            VisualEffect vfx = go.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = asset;

            go.AddComponent<DestroyTimer>().time = time;
            
            return vfx;
        }

        public void SpawnGroundSlam()
        {
            if (player.forces.IsGrounded)
            {
                VisualEffect vfx = SpawnEffect(groundSlam, groundSlamAnchor.position, transform.rotation, 3F);

                if (Physics.Raycast(groundSlamAnchor.position, Vector3.down, out RaycastHit hit, 1F))
                    vfx.transform.up = hit.normal;
            }
        }
    }
}