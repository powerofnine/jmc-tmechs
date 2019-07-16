using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class HandyTools
    {
        [MenuItem("Tools/Handy Tools/Distance to Editor Camera")]
        private static void GetDistanceToCamera()
        {
            Camera cam = SceneView.lastActiveSceneView.camera;
        
            if(cam && Selection.activeGameObject)
                Debug.Log(Vector3.Distance(cam.transform.position, Selection.activeGameObject.transform.position));
        }
    }
}
