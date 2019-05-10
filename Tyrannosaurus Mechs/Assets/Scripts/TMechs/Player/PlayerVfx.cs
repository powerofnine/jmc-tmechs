using System;
using TMechs.Attributes;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Player
{
    public class PlayerVfx : MonoBehaviour
    {
        [Header("References")]
        public VisualEffect sprint;
        public VisualEffect jump;
        public VisualEffect rocketFistCharge;
        public VisualEffect leftPunch;
        public VisualEffect rightPunch;

        [Header("Sprint")]
        [ArrayElementNameBind(nameof(RunEffectMap.terrain))]
        public RunEffectMap[] runEffects = {};

        [Header("Punch")]
        public VisualEffectAsset hitSpark;

        private void Awake()
        {
            sprint.Play();
        }

        private void OnValidate()
        {
            TerrainType[] terrains = (TerrainType[]) Enum.GetValues(typeof(TerrainType));
            
            if(runEffects == null)
                runEffects = new RunEffectMap[terrains.Length];
            
            if(runEffects.Length != terrains.Length)
                Array.Resize(ref runEffects, terrains.Length);

            for (int i = 0; i < terrains.Length; i++)
                runEffects[i].terrain = terrains[i];
        }

        public enum TerrainType
        {
            Neutral,
            Jungle
        }

        [Serializable]
        public struct RunEffectMap
        {
            public TerrainType terrain;
            public VisualEffectAsset effect;
        }
    }
}