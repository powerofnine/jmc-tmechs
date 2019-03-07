using UnityEngine;
using static TMechs.Controls.Action;

namespace TMechs.Player
{
    public class CameraZoom : MonoBehaviour
    {
        public Transform cameraRig;
        
        public float minDistance = 0F;
        public float maxDistance = 15F;

        public float distanceOffset = -.75F;

        public float zoomDistance = 5F;
        
        private void LateUpdate()
        {
            float distance = maxDistance;

//            float zoom = PlayerMovement.Input.GetAxisRaw(ZOOM);
//
//            if (zoom > float.Epsilon)
//                distance = Mathf.Lerp(maxDistance, zoomDistance, zoom);

            if (Physics.Raycast(cameraRig.position, -transform.forward, out RaycastHit hit, maxDistance))
                distance = Mathf.Min(distance, hit.distance + distanceOffset);

            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Vector3 pos = transform.localPosition;
            pos.z = distance;
            transform.localPosition = -pos;
        }
    }
}