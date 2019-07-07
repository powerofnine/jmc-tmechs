// Inspector Gadgets // Copyright 2019 Kybernetik //

using System;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;
using InspectorGadgets.Attributes;

#if UNITY_EDITOR
using InspectorGadgets.Editor;
using UnityEditor;
#endif

namespace InspectorGadgets.Attributes
{
    /// <summary>[Pro-Only]
    /// When the attributed member is drawn in the inspector, it will be highlighted in red when it has the default value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RequiredAttribute : PropertyAttribute
    {
        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private SerializedProperty _SerializedProperty;
        private SerializedPropertyAccessor _SerializedPropertyAccessor;
        private FieldInfo _Field;
        private PropertyInfo _Property;
        private Type _Type;
        private bool _IsReferenceOrNullable;
        private object _DefaultValue;

        /************************************************************************************************************************/

        internal bool IsInitialised
        {
            get
            {
                return _SerializedProperty != null || _Field != null || _Property != null;
            }
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Initialises this attribute to check the value of the specified 'property'.
        /// </summary>
        public void Initialise(SerializedProperty property)
        {
            _SerializedProperty = property;
            _SerializedPropertyAccessor = SerializedPropertyAccessor.GetAccessor(property);
            if (_SerializedPropertyAccessor != null)
                Initialise(_SerializedPropertyAccessor.Field.FieldType);
        }

        /// <summary>
        /// Initialises this attribute to check the value of the specified 'field'.
        /// </summary>
        public void Initialise(FieldInfo field)
        {
            _Field = field;
            Initialise(field.FieldType);
        }

        /// <summary>
        /// Initialises this attribute to check the value of the specified 'property'.
        /// </summary>
        public void Initialise(PropertyInfo property)
        {
            _Property = property;
            Initialise(property.PropertyType);
        }

        /************************************************************************************************************************/

        private void Initialise(Type type)
        {
            _Type = type;

            if (!type.IsValueType || Nullable.GetUnderlyingType(type) != null)
                _IsReferenceOrNullable = true;
            else
                _DefaultValue = Activator.CreateInstance(type);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns true if the attributed member on the specified 'obj' still has its default value.
        /// </summary>
        public bool IsDefaultValue(object obj)
        {
            return IsDefaultValue(_SerializedPropertyAccessor, obj);
        }

        /// <summary>
        /// Returns true if the attributed member on the specified 'obj' still has its default value.
        /// </summary>
        public bool IsDefaultValue(SerializedPropertyAccessor accessor, object obj)
        {
            object value;
            if (accessor != null)
                value = accessor.GetValue(obj);
            else if (_Field != null)
                value = _Field.GetValue(obj);
            else if (_Property != null)
                value = _Property.GetValue(obj, null);
            else
                return false;

            if (value == null)
                return true;

            if (_Type == typeof(string))
                return value.Equals("");

            // Non-null reference or nullable is not default.
            if (_IsReferenceOrNullable)
            {
                return
                    (obj as Object) != null &&
                    value is Object &&
                    (value as Object) == null;
            }

            // Otherwise it is a value type, so we need to compare it to the default value.
            return value.Equals(_DefaultValue);
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Returns the full name of the attributed field or property.
        /// </summary>
        public override string ToString()
        {
            if (_Field != null)
                return _Field.DeclaringType.FullName + "." + _Field.Name;
            else if (_Property != null)
                return _Property.DeclaringType.FullName + "." + _Property.Name;
            else
                return base.ToString();
        }

        /************************************************************************************************************************/
#else
        /************************************************************************************************************************/

        /// <summary>Does nothing in Inspector Gadgets Lite.</summary>
        public void Initialise(FieldInfo field) { }

        /// <summary>Does nothing in Inspector Gadgets Lite.</summary>
        public void Initialise(PropertyInfo property) { }

        /// <summary>Returns true in Inspector Gadgets Lite.</summary>
        public bool IsDefaultValue(object obj) { return true; }

        /************************************************************************************************************************/
#endif
    }
}

#if UNITY_EDITOR
namespace InspectorGadgets.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(RequiredAttribute))]
    internal sealed class RequiredDrawer : ObjectDrawer
    {
        /************************************************************************************************************************/

        private RequiredAttribute _Attribute;

        /************************************************************************************************************************/

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        /************************************************************************************************************************/

        private static readonly Color
            ErrorFieldColor = new Color(1, 0.65f, 0.65f);

        public override void OnGUI(Rect area, SerializedProperty property, GUIContent label)
        {
            var color = GUI.color;

            if (Event.current.type == EventType.Repaint)
            {
                Initialise(property);

                if (_Attribute != null && _Attribute.IsInitialised)
                {
                    var accessor = SerializedPropertyAccessor.GetAccessor(property);

                    try
                    {
                        var targets = property.serializedObject.targetObjects;
                        foreach (var target in targets)
                        {
                            if (_Attribute.IsDefaultValue(accessor, target))
                            {
                                GUI.color = ErrorFieldColor;
                                break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                        GUI.color = ErrorFieldColor;
                    }
                }
            }

            base.OnGUI(area, property, label);

            GUI.color = color;
        }

        /************************************************************************************************************************/

        private void Initialise(SerializedProperty property)
        {
            if (_Attribute != null)
                return;

            _Attribute = attribute as RequiredAttribute;
            _Attribute.Initialise(property);
        }

        /************************************************************************************************************************/
    }
}
#endif
