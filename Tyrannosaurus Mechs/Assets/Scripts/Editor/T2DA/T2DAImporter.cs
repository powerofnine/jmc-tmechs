using System.Linq;
using UnityEditor.Experimental.AssetImporters;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Editor.T2DA
{
    // ReSharper disable once InconsistentNaming
    [ScriptedImporter(1, "t2da")]
    public class T2DAImporter : ScriptedImporter
    {
        public Texture2D[] textures;

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string validate = Validate();
            if (validate != null)
                return;

            int w = textures[0].width;
            int h = textures[0].height;
            GraphicsFormat f = textures[0].graphicsFormat;

            Texture2DArray t2da = new Texture2DArray(w, h, textures.Length, f, TextureCreationFlags.Crunch)
            {
                    wrapMode = textures[0].wrapMode, 
                    wrapModeU = textures[0].wrapModeU, 
                    wrapModeV = textures[0].wrapModeV, 
                    wrapModeW = textures[0].wrapModeW,
                    anisoLevel = textures[0].anisoLevel,
                    filterMode = textures[0].filterMode
            };

            for (int i = 0; i < textures.Length; i++)
                Graphics.CopyTexture(textures[i], 0, 0, t2da, i, 0);
            
            ctx.AddObjectToAsset("T2DA", t2da);
            ctx.SetMainObject(t2da);
        }

        public string Validate()
        {
            if (textures == null || textures.Length <= 0)
                return "Texture list is empty";

            if (textures.Any(tex => !tex))
                return "Some textures are unassigned";

            int w = textures[0].width, h = textures[0].height;
            GraphicsFormat f = textures[0].graphicsFormat;

            if (textures.Any(tex => w != tex.width || h != tex.height || f != tex.graphicsFormat))
                return "Some textures are of different size or graphics format";

            return null;
        }
    }
}