using UnityEngine;

namespace TMechs.Player
{
    public class CameraZoom : MonoBehaviour
    {
        public Transform cameraRig;
        
        public float minDistance = 0F;
        public float maxDistance = 15F;

        public float distanceOffset = -.75F;
        
        private void LateUpdate()
        {
            float distance = maxDistance;

            if (Physics.Raycast(cameraRig.position, -transform.forward, out RaycastHit hit, maxDistance))
                distance = hit.distance + distanceOffset;

            distance = Mathf.Clamp(distance, minDistance, maxDistance);

            Vector3 pos = transform.localPosition;
            pos.z = distance;
            transform.localPosition = -pos;
        }
    }
}