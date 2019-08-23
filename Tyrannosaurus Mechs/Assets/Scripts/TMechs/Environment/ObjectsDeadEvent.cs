using System.Collections.Generic;
using UIEventDelegate;
using UltEvents;
using UnityEngine;

namespace TMechs.Environment
{
    public class ObjectsDeadEvent : MonoBehaviour
    {
        public List<GameObject> trackedObjects = new List<GameObject>();
        public ReorderableEventList onAllDead;
        public UltEvent onAllDeadNew;

        private void Update()
        {
            trackedObjects.RemoveAll(x => !x);

            if (trackedObjects.Count <= 0)
            {
                if(onAllDead != null)
                    EventDelegate.Execute(onAllDead.List);
                onAllDeadNew.InvokeX();
            
                Destroy(this);
            }
        }
    }
}
