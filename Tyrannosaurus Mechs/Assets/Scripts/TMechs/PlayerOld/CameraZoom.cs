using UnityEngine;

namespace TMechs.PlayerOld
{
    public class CameraZoom : MonoBehaviour
    {
        public Transform cameraRig;
        
        public float minDistance = 0F;
        public float maxDistance = 15F;

        public float distanceOffset = -.75F;

        [Header("Dash Zoom")]
        public float zoomDistance = 5F;
        public float zoomDamp = .1F;
        private float dashZoom;
        private float dashZoomVelocity;
        
        
        private void Update()
        {
            float dash = Player.Instance.Animator.GetFloat(Anim.MOVE_DELTA) <= .6F ? 0F : zoomDistance;
            dashZoom = Mathf.SmoothDamp(dashZoom, dash, ref dashZoomVelocity, zoomDamp);
            float maxDistance = this.maxDistance - dashZoom;
            
            float distance = maxDistance;

            if (Physics.Raycast(cameraRig.position, -transform.forward, out RaycastHit hit, maxDistance))
                distance = Mathf.Min(distance, hit.distance + distanceOffset);
            
            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Vector3 pos = transform.localPosition;
            pos.z = -distance;
            transform.localPosition = pos;
        }
    }
}