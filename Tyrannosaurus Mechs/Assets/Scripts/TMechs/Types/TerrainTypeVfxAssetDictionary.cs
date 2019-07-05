using System;
using TMechs.PlayerOld;
using UnityEngine.Experimental.VFX;

namespace TMechs.Types
{
    [Serializable]
    public class TerrainTypeVfxAssetDictionary : SerializedDictionary<PlayerVfx.TerrainType, VisualEffectAsset>
    {
        public override PlayerVfx.TerrainType GetNextKey()
        {
            foreach (PlayerVfx.TerrainType t in Enum.GetValues(typeof(PlayerVfx.TerrainType)))
            {
                if (!ContainsKey(t))
                    return t;
            }

            return default;
        }
    }
}