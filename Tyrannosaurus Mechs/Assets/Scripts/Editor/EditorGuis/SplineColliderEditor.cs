using TMechs.Environment;
using UnityEditor;
using UnityEngine;

namespace Editor.EditorGuis
{
    [CustomEditor(typeof(SplineCollider))]
    public class SplineColliderEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            SplineCollider col = target as SplineCollider;

            if (!col)
                return;

            if (col.locks.lockEditor)
                return;
            
            Handles.matrix = col.transform.localToWorldMatrix;

            for (int i = 0; i < col.points.Count; i++)
            {
                Handles.color = Color.magenta;

                if (i == 0 || i + 1 >= col.points.Count)
                    Handles.color = Color.black;
                
                Vector3 vec = Handles.FreeMoveHandle(col.points[i], Quaternion.identity, col.controlSize, Vector3.zero, Handles.DotHandleCap);
                
                if (col.locks.lockX)
                    vec.x = col.points[i].x;
                if (col.locks.lockY)
                    vec.y = col.points[i].y;
                if (col.locks.lockZ)
                    vec.z = col.points[i].z;

                if (Vector3.Distance(col.points[i], vec) >= float.Epsilon)
                {
                    Undo.RecordObject(col, "Moved Point");                
                    col.points[i] = vec;
                }

                if(i == 0)
                    continue;

                Vector3 mid = (col.points[i] + col.points[i - 1]) / 2;
                
                Handles.color = Color.cyan;
                if (Handles.Button(mid, Quaternion.identity, col.controlSize / 2F, col.controlSize / 1.5F, Handles.DotHandleCap))
                {
                    Undo.RecordObject(col, "Inserted Point");
                    col.points.Insert(i, mid);
                }

                if (i + 1 >= col.points.Count)
                    return;

                Handles.color = Color.red;
                if (Handles.Button(col.points[i] + col.controlSize * 1.5F * Camera.current.transform.up, Quaternion.identity, col.controlSize / 2F, col.controlSize / 2F, Handles.DotHandleCap))
                {
                    Undo.RecordObject(col, "Removed Point");
                    col.points.RemoveAt(i);
                }
            }
        }
    }
}