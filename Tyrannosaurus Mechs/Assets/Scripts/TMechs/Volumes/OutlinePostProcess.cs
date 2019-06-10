using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace TMechs.Volumes
{
    [Serializable, VolumeComponentMenu("Post-processing/TMechs/Outline")]
    public class OutlinePostProcess : VolumeComponent
    {
        public ColorParameter outlineColor = new ColorParameter(Color.black);
        public ClampedFloatParameter normalOutlineMultiplier = new ClampedFloatParameter(1F, 0F, 4F);
        public ClampedFloatParameter normalOutlineBias = new ClampedFloatParameter(1F, 1F, 4F);
        public ClampedFloatParameter depthOutlineMultiplier = new ClampedFloatParameter(1F, 0F, 4F);
        public ClampedFloatParameter depthOutlineBias = new ClampedFloatParameter(1F, 1F, 4F);
        
        
    }
}
