using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class LodHelperSettings : ScriptableObject
{
    private const string PATH = "Assets/Plugins/fuj1n/LodHelper/Editor/Settings.asset";
    
    public static LodHelperSettings Instance
    {
        get
        {
            if (instance)
                return instance;

            instance = AssetDatabase.LoadAssetAtPath<LodHelperSettings>(PATH);
            if (instance)
                return instance;

            instance = CreateInstance<LodHelperSettings>();

            string[] path = Path.GetDirectoryName(PATH)?.Split('/');

            if (path == null)
                return null;
            
            for (int i = 1; i < path.Length; i++)
            {
                AssetDatabase.CreateFolder(path[i - 1], path[i]);
            }
            
            AssetDatabase.CreateAsset(instance, PATH);
            return instance;
        }
    }

    private static LodHelperSettings instance;

    public bool activelyPreventLodSelection;
}
