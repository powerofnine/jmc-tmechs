using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;

namespace Editor.T2DA
{
    public class CreateT2DA : EndNameEditAction
    {
        [MenuItem("Assets/Create/TMechs/Texture 2D Array")]
        private static void Create()
        {
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(0, CreateInstance<CreateT2DA>(),
                    "New Texture2D Array.t2da", null, null);
        }
        
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            File.WriteAllText(pathName, "This file is supposed to be blank");
            AssetDatabase.Refresh();
        }
    }
}