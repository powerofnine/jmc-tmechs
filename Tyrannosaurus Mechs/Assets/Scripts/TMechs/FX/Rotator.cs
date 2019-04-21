using UnityEngine;

namespace TMechs.FX
{
    public class Rotator : MonoBehaviour
    {
        public Vector3 axis = new Vector3(45F, 35F, 25F);

        private void Update()
        {
            transform.Rotate(axis * Time.unscaledDeltaTime);
        }
    }
}
