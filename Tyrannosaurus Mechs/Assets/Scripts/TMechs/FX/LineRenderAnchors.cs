using System.Linq;
using UnityEngine;

namespace TMechs.FX
{
    [ExecuteAlways]
    public class LineRenderAnchors : MonoBehaviour
    {
        public Transform[] anchors = {};

        private LineRenderer render;
        
        private void LateUpdate()
        {
            if (!render)
                render = GetComponent<LineRenderer>();

            render.positionCount = anchors.Length;
            render.SetPositions(anchors.Select(x => x.position).ToArray());
        }
    }
}
