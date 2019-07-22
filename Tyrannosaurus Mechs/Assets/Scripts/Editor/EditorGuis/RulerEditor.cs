using TMechs.Environment;
using UnityEditor;
using UnityEngine;

namespace Editor.EditorGuis
{
    [CustomEditor(typeof(Ruler))]
    public class RulerEditor : UnityEditor.Editor
    {
        private void OnSceneGUI()
        {
            Ruler ruler = target as Ruler;

            if (!ruler)
                return;

            if (ruler.locks.lockEditor)
                return;
            
            Handles.matrix = ruler.transform.localToWorldMatrix;

            for (int i = 0; i < ruler.points.Count; i++)
            {
                Handles.color = Color.magenta;

                if (i == 0 || i + 1 >= ruler.points.Count)
                    Handles.color = Color.black;
                
                Vector3 vec = Handles.FreeMoveHandle(ruler.points[i], Quaternion.identity, ruler.controlSize, Vector3.zero, Handles.DotHandleCap);
                
                if (ruler.locks.lockX)
                    vec.x = ruler.points[i].x;
                if (ruler.locks.lockY)
                    vec.y = ruler.points[i].y;
                if (ruler.locks.lockZ)
                    vec.z = ruler.points[i].z;

                if (Vector3.Distance(ruler.points[i], vec) >= float.Epsilon)
                {
                    Undo.RecordObject(ruler, "Moved Point");                
                    ruler.points[i] = vec;
                }

                if(i == 0)
                    continue;

                Vector3 mid = (ruler.points[i] + ruler.points[i - 1]) / 2;
                
                Handles.color = Color.cyan;
                if (Handles.Button(mid, Quaternion.identity, ruler.controlSize / 2F, ruler.controlSize / 1.5F, Handles.DotHandleCap))
                {
                    Undo.RecordObject(ruler, "Inserted Point");
                    ruler.points.Insert(i, mid);
                }

                if (i + 1 >= ruler.points.Count)
                    return;

                Handles.color = Color.red;
                if (Handles.Button(ruler.points[i] + ruler.controlSize * 1.5F * Camera.current.transform.up, Quaternion.identity, ruler.controlSize / 2F, ruler.controlSize / 2F, Handles.DotHandleCap))
                {
                    Undo.RecordObject(ruler, "Removed Point");
                    ruler.points.RemoveAt(i);
                }
            }
        }
    }
}