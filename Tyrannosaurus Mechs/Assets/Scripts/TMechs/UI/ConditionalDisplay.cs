using System;
using TMechs.Data;
using UnityEngine;

namespace TMechs.UI
{
    public class ConditionalDisplay : MonoBehaviour, MenuController.IMenuCallback
    {
        public Condition condition;

        private void Awake()
        {
            gameObject.SetActive(TestCondition());
        }

        private bool TestCondition()
        {
            switch (condition)
            {
                case Condition.HasSaveFile:
                    return SaveSystem.GetLexicon().Length > 0;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [Serializable]
        public enum Condition
        {
            HasSaveFile
        }

        public void OnMenuChanged(bool activated)
        {
            gameObject.SetActive(TestCondition());
        }
    }
}