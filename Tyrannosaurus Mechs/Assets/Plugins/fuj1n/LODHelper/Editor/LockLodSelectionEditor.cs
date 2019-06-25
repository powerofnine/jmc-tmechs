using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LockLodSelection))]
[CanEditMultipleObjects]
public class LockLodSelectionEditor : Editor
{
    private void OnSceneGUI()
    {
        LockLodSelection trg = target as LockLodSelection;

        if (!trg)
            return;

        Selection.objects = Selection.objects.Select(s =>
        {
            GameObject go = s as GameObject;

            if (!go)
                return s;

            if (!go.GetComponent<LockLodSelection>())
                return s;

            return go.GetComponentInParent<LODGroup>().gameObject;
        }).ToArray();
    }
}
