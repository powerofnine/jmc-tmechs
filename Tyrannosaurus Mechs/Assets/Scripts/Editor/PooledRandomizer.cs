using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Editor
{
    public class PooledRandomizer : ScriptableWizard
    {
        public GameObject[] objectPool;

        [MenuItem("Tools/Pooled Randomizer")]
        private static void CreateWizard()
        {
            DisplayWizard<PooledRandomizer>("Pooled Randomizer", "Randomize and Close", "Randomize");
        }

        private void OnWizardUpdate()
        {
            errorString = Validate();
        }

        private void OnWizardCreate()
        {
            OnWizardOtherButton();
        }

        private void OnWizardOtherButton()
        {
            if (!string.IsNullOrWhiteSpace(errorString))
            {
                Debug.LogError(errorString);
                return;
            }

            GameObject[] selection = GetSelection();
            foreach (GameObject go in selection)
            {
                GameObject choice = objectPool[Random.Range(0, objectPool.Length)];
                GameObject newObject;

                if (IsPrefab(choice))
                    newObject = PrefabUtility.InstantiatePrefab(choice) as GameObject;
                else
                    newObject = Instantiate(choice);

                if (!newObject)
                    continue;
                
                newObject.transform.SetParent(go.transform.parent, false);
                newObject.transform.SetSiblingIndex(go.transform.GetSiblingIndex());
                newObject.transform.position = go.transform.position;
                newObject.transform.rotation = go.transform.rotation;
                newObject.transform.localScale = go.transform.localScale;
                newObject.name = go.name;
                
                Undo.RegisterCreatedObjectUndo(newObject, "Create replacement object");
                Undo.DestroyObjectImmediate(go);
            }
            
            Undo.CollapseUndoOperations(selection.Length * 2 + 1);
            Undo.SetCurrentGroupName("Randomize objects");
        }

        private string Validate()
        {
            if (objectPool == null || objectPool.Length == 0)
                return "Object pool is empty";
            if (objectPool.Any(x => !x))
                return "Object pool contains empty objects";
            
            return null;
        }

        private bool IsPrefab(GameObject go)
        {
            return go && PrefabUtility.IsPartOfAnyPrefab(go);
        }

        private GameObject[] GetSelection()
        {
            return Selection.GetFiltered<GameObject>(SelectionMode.Editable | SelectionMode.TopLevel | SelectionMode.ExcludePrefab);
        }
    }
}
