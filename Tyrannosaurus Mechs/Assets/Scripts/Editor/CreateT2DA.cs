using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

namespace Editor
{
    // ReSharper disable once InconsistentNaming
    public class CreateT2DA : ScriptableWizard
    {
        public string path;
        public string filename = "New T2DA";
        public Texture2D[] textures = {};
        
        [MenuItem("Assets/Create/Texture 2D Array")]
        private static void CreateWizard()
        {
            CreateT2DA ct2da = DisplayWizard<CreateT2DA>("Create Texture2D Array", "Create", "Cancel");
            
            string path = "Assets";
            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }

            ct2da.path = path;
        }

        private void OnWizardCreate()
        {
            if (!Validate())
            {
                Debug.LogError("Could not create T2DA as there are unresolved errors");
                return;
            }

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
            
            AssetDatabase.CreateAsset(t2da, path + "/" + filename + ".asset");
        }

        private void OnWizardUpdate()
        {
            Validate();
        }

        private bool Validate()
        {
            errorString = "";
            
            if (textures == null || textures.Length <= 0)
            {
                errorString = "Texture list is empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(filename))
                filename = "New T2DA";
            
            if (textures.Any(tex => !tex))
            {
                errorString = "Some textures are unassigned";
                return false;
            }

            int w = textures[0].width, h = textures[0].height;
            GraphicsFormat f = textures[0].graphicsFormat;

            if (textures.Any(tex => w != tex.width || h != tex.height || f != tex.graphicsFormat))
            {
                errorString = "Some textures are of different size or graphics format";
                return false;
            }

            return true;
        }

        private void OnWizardOtherButton()
            => Close();
    }
}
