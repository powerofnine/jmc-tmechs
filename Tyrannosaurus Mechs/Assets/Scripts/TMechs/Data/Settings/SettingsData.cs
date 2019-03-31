using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using UnityEngine;

namespace TMechs.Data.Settings
{
    public static class SettingsData
    {
        private static Type[] providers;
        private static readonly Dictionary<Type, object> values = new Dictionary<Type, object>();

        public static T Get<T>() where T : class => (T) values[typeof(T)];

        public static void Reset<T>() where T : class
        {
            if (values.ContainsKey(typeof(T)))
                values.Remove(typeof(T));

            object inst = CreateInstance(typeof(T));
            values.Add(typeof(T), inst);
        }

        public static void Flush()
        {
            foreach (Type provider in providers)
            {
                if (!values.ContainsKey(provider))
                    continue;
                
                PlayerPrefs.SetString("settings:" + provider.FullName, JsonConvert.SerializeObject(values[provider]));
            }
        }

        private static object CreateInstance(Type t)
        {
            ConstructorInfo constructor = t.GetConstructor(new Type[0]);
            
            if(constructor == null)
                throw new ArgumentException("Error: " + nameof(t) + " does not contain an empty constructor");

            return constructor.Invoke(new object[0]);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Init()
        {
            IEnumerable<Type> discoveredProviders = 
                    from assembly in AppDomain.CurrentDomain.GetAssemblies()
                    from t in assembly.GetTypes()
                    let attrib = t.GetCustomAttribute<SettingsProviderAttribute>(false)
                    where attrib != null
                    select t;
            providers = discoveredProviders.ToArray();

            foreach (Type provider in providers)
            {
                object obj = null;

                try
                {
                    if (PlayerPrefs.HasKey("settings:" + provider.FullName))
                        obj = JsonConvert.DeserializeObject("settings:" + provider.FullName, provider);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.StackTrace);
                }

                if (obj == null)
                    obj = CreateInstance(provider);

                if (obj != null)
                    values.Add(provider, obj);
            }

            Application.quitting += Flush;
        }
    }
}