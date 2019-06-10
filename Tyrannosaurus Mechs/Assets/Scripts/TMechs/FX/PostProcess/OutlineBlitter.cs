using UnityEngine;

namespace TMechs.FX.PostProcess
{
    public class OutlineBlitter : MonoBehaviour
    {
        public Material material;

        private void Start()
        {
            Camera cam = GetComponent<Camera>();
            if (!cam)
            {
                Destroy(this);
                return;
            }

            cam.depthTextureMode = cam.depthTextureMode | DepthTextureMode.DepthNormals;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, dest, material);
        }
    }
}
