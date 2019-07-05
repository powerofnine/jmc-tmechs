using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TMechs.Animation
{
    [CreateAssetMenu(menuName = "Animation Collection", fileName = "Animations", order = 400)]
    public class AnimationCollection : ScriptableObject, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string serializedType;
        [SerializeField]
        private AnimationClip[] animations;

        private Type enumType;
        private Dictionary<Enum, AnimationClip> valueMap;

        public bool IsType(Type type)
        {
            return enumType == type;
        }

        public AnimationClip GetClip(Enum at)
        {
            if (!valueMap.ContainsKey(at))
                return null;

            return valueMap[at];
        }
        
        public void OnBeforeSerialize()
        {
            if (enumType == null)
                return;
            
            serializedType = enumType.AssemblyQualifiedName;
        }

        public void OnAfterDeserialize()
        {
            enumType = ParseType(serializedType);

            if (enumType == null)
                return;
            
            int len = Enum.GetValues(enumType).Length;
            
            if(animations == null)
                animations = new AnimationClip[len];
            else if(animations.Length != len)
                Array.Resize(ref animations, len);

            Enum[] vals = Enum.GetValues(enumType).Cast<Enum>().ToArray();
            valueMap = Enumerable.Range(0, vals.Length).ToDictionary(x => vals[x], x => animations[x]);
        }

        public static Type ParseType(string type)
        {
            if (string.IsNullOrWhiteSpace(type))
                return null;
            
            Type enumType = Type.GetType(type);
            if(enumType == null)
                throw new ArgumentException($"Specified type {type} cannot be found");
            if (!enumType.IsEnum)
                throw new ArgumentException($"Specified type {type} is not enum");

            return enumType;
        }

        [AttributeUsage(AttributeTargets.Enum)]
        public sealed class EnumAttribute : Attribute
        {}

        public sealed class ValidateAttribute : PropertyAttribute
        {
            public readonly Type type;

            public ValidateAttribute(Type type)
            {
                this.type = type;
            }
        }
    }
}
