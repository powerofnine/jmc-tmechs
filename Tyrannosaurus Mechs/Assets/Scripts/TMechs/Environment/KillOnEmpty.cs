using UnityEngine;

namespace TMechs.Environment
{
    public class KillOnEmpty : MonoBehaviour
    {
        private void LateUpdate()
        {
            if (transform.childCount <= 0)
                Destroy(gameObject);
        }
    }
}