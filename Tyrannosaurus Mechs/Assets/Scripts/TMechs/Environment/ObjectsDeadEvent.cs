using System.Collections.Generic;
using UIEventDelegate;
using UnityEngine;

namespace TMechs.Environment
{
    public class ObjectsDeadEvent : MonoBehaviour
    {
        public List<GameObject> trackedObjects = new List<GameObject>();
        public ReorderableEventList onAllDead;

        private void Update()
        {
            trackedObjects.RemoveAll(x => !x);

            if (trackedObjects.Count <= 0)
            {
                if(onAllDead != null)
                    EventDelegate.Execute(onAllDead.List);
            
                Destroy(this);
            }
        }
    }
}
