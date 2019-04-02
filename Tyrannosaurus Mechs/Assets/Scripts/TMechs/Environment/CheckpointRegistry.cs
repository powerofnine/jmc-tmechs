using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMechs.Environment
{
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