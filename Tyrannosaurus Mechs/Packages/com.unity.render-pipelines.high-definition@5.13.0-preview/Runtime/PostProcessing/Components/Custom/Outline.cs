using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.HDPipeline;
using UnityEngine.Rendering;

// ReSharper disable once CheckNamespace
namespace TMechs.PostProcessing
{
    [Serializable]
    [VolumeComponentMenu("Post-processing/TMechs/Outline")]
    public sealed class Outline : VolumeComponent, IPostProcessComponent
    {
        public BoolParameter enabled = new BoolParameter(false);
        [Tooltip("Number of pixels between samples that are tested for an edge. When this value is 1, tested samples are adjacent.")]
        public IntParameter scale = new IntParameter(1);
        public ColorParameter color = new ColorParameter(Color.black);
        [Tooltip("Difference between depth values, scaled by the current depth, required to draw an edge.")]
        public FloatParameter depthThreshold = new FloatParameter(1.5f);
        [Range(0, 1), Tooltip("The value at which the dot product between the surface normal and the view direction will affect " +
                              "the depth threshold. This ensures that surfaces at right angles to the camera require a larger depth threshold to draw " +
                              "an edge, avoiding edges being drawn along slopes.")]
        public FloatParameter depthNormalThreshold = new FloatParameter(0.5f);
        [Tooltip("Scale the strength of how much the depthNormalThreshold affects the depth threshold.")]
        public FloatParameter depthNormalThresholdScale = new FloatParameter(7f);
        [Range(0, 1), Tooltip("Larger values will require the difference between normals to be greater to draw an edge.")]
        public FloatParameter normalThreshold = new FloatParameter(0.4f);
        
        public bool IsActive()
        {
            return enabled.value && enabled.overrideState;
        }
    }
}