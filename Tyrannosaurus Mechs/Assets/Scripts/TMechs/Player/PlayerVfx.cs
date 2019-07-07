using System;
using JetBrains.Annotations;
using TMechs.Attributes;
using TMechs.Types;
using UnityEngine;
using UnityEngine.Experimental.VFX;

namespace TMechs.Player
{
    public class PlayerVfx : MonoBehaviour
    {
        public StringVfxDictionary vfxRegistry = new StringVfxDictionary();
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

        [UsedImplicitly]
        private void PlayVfx(string effect)
        {
            
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
        
        [Serializable]
        public struct Vfx
        {
            public Transform anchor;
            public bool isDynamic;
            public VisualEffectAsset effect;
            public TerrainTypeVfxAssetDictionary dynamicEffect;
        }
    }
}