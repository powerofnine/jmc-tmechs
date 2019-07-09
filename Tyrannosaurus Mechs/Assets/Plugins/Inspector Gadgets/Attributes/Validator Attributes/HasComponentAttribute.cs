// Inspector Gadgets // Copyright 2019 Kybernetik //

using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace InspectorGadgets.Attributes
{
    /// <summary>[Pro-Only]
    /// When applied to a <see cref="GameObject"/> field, any object assigned to that field must have a component of
    /// the specified type.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class HasComponentAttribute : ValidatorAttribute
    {
        /************************************************************************************************************************/

        /// <summary>The types of components that are required.</summary>
        public readonly Type[] ComponentTypes;

        /************************************************************************************************************************/

        /// <summary>
        /// Constructs a new <see cref="HasComponentAttribute"/> to require a component of the specified type.
        /// </summary>
        public HasComponentAttribute(params Type[] componentTypes)
        {
            ComponentTypes = componentTypes;
        }

        /************************************************************************************************************************/
#if UNITY_EDITOR
        /************************************************************************************************************************/

        private static Object _PreviousValue;

        /************************************************************************************************************************/

        /// <summary>
        /// Called before the property is drawn so you can cache its value if needed.
        /// </summary>
        public override void OnBeforeProperty(UnityEditor.SerializedProperty property)
        {
            if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
                _PreviousValue = property.objectReferenceValue;
        }

        /************************************************************************************************************************/

        /// <summary>
        /// Validate the value of the specified 'property'.
        /// </summary>
        public override void Validate(UnityEditor.SerializedProperty property)
        {
            if (property.propertyType == UnityEditor.SerializedPropertyType.ObjectReference)
            {
                var value = property.objectReferenceValue;
                var gameObject = value as GameObject;
                if (gameObject != null)
                {
                    for (int i = 0; i < ComponentTypes.Length; i++)
                    {
                        var componentType = ComponentTypes[i];
                        if (gameObject.GetComponent(componentType) == null)
                        {
                            Debug.LogWarning("Unable to assign " + gameObject + " to " + property.propertyPath +
                                " because it doesn't have a " + componentType.GetNameCS() + " component", gameObject);

                            property.objectReferenceValue = _PreviousValue;
                        }
                    }
                }
            }
        }

        /************************************************************************************************************************/
#endif
    }
}

