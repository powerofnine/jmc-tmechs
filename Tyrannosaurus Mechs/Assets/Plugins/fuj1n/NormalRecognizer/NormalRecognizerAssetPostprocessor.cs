using System;
using UnityEditor;
using UnityEngine;

namespace fuj1n.NormalRecognizer
{
    public class NormalRecognizerAssetPostprocessor : AssetPostprocessor
    {
        private void OnPostprocessTexture(Texture2D texture)
        {
            // Packages are inherently readonly, so trying to change it and reimport will cause it to reimport without
            // changes, which will cause Unity to try to reimport the same asset over and over again.
            
            // So let's not do that
            if (assetPath.StartsWith("Packages"))
                return;
            
            TextureImporter importer = (TextureImporter) assetImporter;
            if (importer.textureType != TextureImporterType.Default)
                return;
            
            for (int x = -5; x < 5; x++)
            {
                for (int y = -5; y < 5; y++)
                {
                    if(x == 0 || y == 0)
                        continue;
                    
                    int xc = texture.width / 2 / x + texture.width / 2;
                    int yc = texture.height / 2 / y + texture.height / 2;

                    Vector3 v = SampleNormal(texture.GetPixel(xc, yc));

                    if (v.z < -.15F)
                        return;

                    if (v.magnitude > 1.25F)
                        return;
                }
            }
            
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            
            settings.ApplyTextureType(TextureImporterType.NormalMap);
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }

        private Vector3 SampleNormal(Color c)
        {
            float x = Mathf.Lerp(-1F, 1F, c.r);
            float y = Mathf.Lerp(-1F, 1F, c.g);
            float z = Mathf.Lerp(-1F, 1F, c.b);
            
            return new Vector3(x, y, z);
        }
    }
}
