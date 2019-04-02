using System;
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
    }
}
