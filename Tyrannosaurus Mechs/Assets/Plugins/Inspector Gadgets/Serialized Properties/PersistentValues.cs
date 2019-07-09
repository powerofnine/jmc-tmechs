// Inspector Gadgets // Copyright 2019 Kybernetik //

#if UNITY_EDITOR && UNITY_2017_3_OR_NEWER

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InspectorGadgets.Editor
{
    /// <summary>[Pro-Only] [Editor-Only]
    /// Manages values that need to persist after leaving Play Mode.
    /// </summary>
    public static class PersistentValues
    {
        /************************************************************************************************************************/

        public enum Operation
        {
            Toggle,
            Add,
            Remove,
        }

        /************************************************************************************************************************/

        private static readonly Dictionary<Object, SerializedObjectReference>
            Objects = new Dictionary<Object, SerializedObjectReference>();
        private static readonly List<SerializedPropertyReference>
            Properties = new List<SerializedPropertyReference>();

        private static bool _IsInitialised;

        /************************************************************************************************************************/

        private static List<Object> _ObjectsToPersist;

        [MenuItem(Strings.PersistAfterPlayModeComponent, priority = 510)]// Group just after Unity's Paste commands.
        private static void PersistComponent(MenuCommand command)
        {
            IGEditorUtils.GroupedInvoke(command, (context) =>
            {
                PersistObjects(Operation.Toggle, context.ToArray());
            });
        }

        [MenuItem(Strings.PersistAfterPlayModeComponent, validate = true)]
        private static bool ValidatePersistMethod()
        {
            return EditorApplication.isPlayingOrWillChangePlaymode;
        }

        /************************************************************************************************************************/

        public static bool WillPersist(Object obj)
        {
            if (!_IsInitialised)
                return false;

            return Objects.ContainsKey(obj);
        }

        /************************************************************************************************************************/

        public static void PersistObjects(Operation operation, params Object[] objects)
        {
            Initialise();

            bool isWatching = false;

            for (int i = 0; i < objects.Length; i++)
            {
                var obj = objects[i];

                if (operation == Operation.Add ||
                    (operation == Operation.Toggle && !Objects.ContainsKey(obj)))
                {
                    Objects.Add(obj, obj);

                    if (!isWatching)
                    {
                        isWatching = true;
                        WatcherWindow.Watch(objects);
                    }
                }
                else
                {
                    Objects.Remove(obj);
                }
            }
        }

        /************************************************************************************************************************/

        internal static void AddMenuItem(GenericMenu menu, SerializedProperty property)
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode ||
                EditorUtility.IsPersistent(property.serializedObject.targetObject))
                return;

            var index = IndexOfProperty(property);

            menu.AddItem(new GUIContent(Strings.PersistAfterPlayMode), index >= 0,
                () => PersistProperty(Operation.Toggle, property));
        }

        /************************************************************************************************************************/

        public static bool WillPersist(SerializedProperty property)
        {
            if (!_IsInitialised)
                return false;

            var index = IndexOfProperty(property);
            return index >= 0;
        }

        /************************************************************************************************************************/

        public static void PersistProperty(Operation operation, SerializedProperty property)
        {
            Initialise();

            var index = IndexOfProperty(property);

            if (operation == Operation.Add ||
                (operation == Operation.Toggle && index < 0))
            {
                Properties.Add(property);
                WatcherWindow.Watch(property);
            }
            else if (index >= 0)
            {
                Properties.RemoveAt(index);
            }
        }

        /************************************************************************************************************************/

        private static int IndexOfProperty(SerializedProperty property)
        {
            for (int i = 0; i < Properties.Count; i++)
            {
                if (IGEditorUtils.AreSameProperty(Properties[i].Property, property))
                    return i;
            }

            return -1;
        }

        /************************************************************************************************************************/

        private static void Initialise()
        {
            if (_IsInitialised)
                return;

            _IsInitialised = true;

            SerializedObjectReference[] objects = null;
            object[][] objectValues = null;
            SerializedPropertyReference[] properties = null;
            object[][] propertyValues = null;

            EditorApplication.playModeStateChanged += (change) =>
            {
                switch (change)
                {
                    case PlayModeStateChange.ExitingPlayMode:
                        // Objects.
                        objects = new SerializedObjectReference[Objects.Count];
                        Objects.Values.CopyTo(objects, 0);

                        objectValues = new object[objects.Length][];
                        for (int i = 0; i < objects.Length; i++)
                        {
                            objectValues[i] = GetValues(objects[i]);
                        }

                        // Properties.
                        properties = Properties.ToArray();
                        Properties.Clear();

                        propertyValues = new object[properties.Length][];
                        for (int i = 0; i < properties.Length; i++)
                        {
                            var property = properties[i];
                            var targetValues = new object[property.TargetObjects.Length];
                            var j = 0;
                            property.Property.serializedObject.Update();
                            IGEditorUtils.ForEachTarget(property, (prop) =>
                            {
                                targetValues[j++] = SerializedPropertyAccessor.GetValue(prop);
                            }, null);
                            propertyValues[i] = targetValues;
                        }

                        break;

                    case PlayModeStateChange.EnteredEditMode:
                        // Objects.
                        for (int i = 0; i < objects.Length; i++)
                        {
                            SetValues(objects[i], objectValues[i]);
                        }

                        // Properties.
                        Debug.Assert(properties != null, "EnteredEditMode without ExitingPlayMode. This should never happen.");

                        for (int i = 0; i < properties.Length; i++)
                        {
                            var property = properties[i].Property;
                            if (property == null)
                                continue;

                            property.serializedObject.Update();
                            var targetValues = propertyValues[i];
                            var j = 0;
                            IGEditorUtils.ForEachTarget(property, (prop) =>
                            {
                                var value = targetValues[j++];
                                SerializedPropertyAccessor.SetValue(prop, value);
                            });
                        }

                        objects = null;
                        objectValues = null;
                        properties = null;
                        propertyValues = null;
                        break;
                }
            };
        }

        /************************************************************************************************************************/

        private static object[] GetValues(Object obj)
        {
            var component = obj as Component;
            if (!ReferenceEquals(component, null))
                return GetValues(component);

            Debug.Log("Unable to persist " + obj, obj);
            return null;
        }

        private static void SetValues(Object obj, object[] values)
        {
            var component = obj as Component;
            if (!ReferenceEquals(component, null))
            {
                SetValues(component, values);
                return;
            }

            Debug.Log("Unable to persist " + obj, obj);
        }

        /************************************************************************************************************************/

        private static object[] GetValues(Component component)
        {
            var values = new List<object>();

            using (var serializedObject = new SerializedObject(component))
            {
                IGEditorUtils.ForEachProperty(serializedObject, false, (property) =>
                {
                    values.Add(SerializedPropertyAccessor.GetValue(property));
                });
            }

            return values.ToArray();
        }

        private static void SetValues(Component component, object[] values)
        {
            var i = 0;

            using (var serializedObject = new SerializedObject(component))
            {
                IGEditorUtils.ForEachProperty(serializedObject, false, (property) =>
                {
                    SerializedPropertyAccessor.SetValue(property, values[i++]);
                });
            }
        }

        /************************************************************************************************************************/
    }
}

#endif
