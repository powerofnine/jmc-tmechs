using UnityEngine;

namespace TMechs.FX
{
    public class BillboardCamera : MonoBehaviour
    {
        public bool resetScale;
        
        private void LateUpdate()
        {
            transform.forward = -(Player.Player.Instance.Camera.transform.position - transform.position).normalized;
        }
    }
}
