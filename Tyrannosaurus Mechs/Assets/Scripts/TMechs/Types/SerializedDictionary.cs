using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMechs.Types
{
    [Serializable]
    public class SerializedDictionary<K, V> : Dictionary<K, V>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private List<K> keys = new List<K>();

        [SerializeField]
        private List<V> values = new List<V>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();

            foreach (KeyValuePair<K, V> kvp in this)
            {
                keys.Add(kvp.Key);
                values.Add(kvp.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();

            for (int i = 0; i < keys.Count; i++)
            {
                if(ContainsKey(keys[i]))
                    keys[i] = GetNextKey();
                Add(keys[i], values[i]);
            }

            keys.Clear();
            values.Clear();
        }

        public virtual K GetNextKey()
        {
            return default;
        }
    }
}