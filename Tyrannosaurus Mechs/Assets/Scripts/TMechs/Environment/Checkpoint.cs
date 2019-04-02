using System;
using System.Collections.Generic;
using TMechs.InspectorAttributes;
using UnityEngine;

namespace TMechs.Environment
{
    public class Checkpoint : MonoBehaviour
    {
        [Id("Unique Checkpoint ID")]
        public string id;

        private void Awake()
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarningFormat("Checkpoint {0} does not have an ID, the checkpoint will not be remembered", name);
                id = Guid.NewGuid().ToString();
            }
            
            if(CheckpointRegistry.Instance.IsRegistered(id))
                throw new CheckpointIdNotUniqueException("Checkpoint ID of " + this + " not unique");
                
            CheckpointRegistry.Instance.Register(this);
        }
        
        public class CheckpointIdNotUniqueException : Exception
        {
            public CheckpointIdNotUniqueException(string message) : base(message)
            {
            }
        }
        
        public class CheckpointRegistry : MonoBehaviour
        {
            public static CheckpointRegistry Instance
            {
                get
                {
                    if (!instance)
                    {
                        instance = new GameObject("Checkpoint Registry").AddComponent<CheckpointRegistry>();
                        instance.gameObject.transform.SetAsFirstSibling();
                    }

                    return instance;
                }
            }
            private static CheckpointRegistry instance;

            private readonly Dictionary<string, Checkpoint> checkpoints = new Dictionary<string, Checkpoint>();
        
            public void Register(Checkpoint cp)
            {
                if (IsRegistered(cp.id))
                    return;
                checkpoints.Add(cp.id, cp);
            }

            public bool IsRegistered(string id) => checkpoints.ContainsKey(id);
        }
    }
}
