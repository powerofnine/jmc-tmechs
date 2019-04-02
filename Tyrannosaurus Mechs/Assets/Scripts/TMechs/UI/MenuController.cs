using System;
using System.Collections.Generic;
using UnityEngine;

namespace TMechs.UI
{
    public class MenuController : MonoBehaviour
    {
        public GameObject menuMain;
        public GameObject menuPause;
        public GameObject menuSettings;
        public GameObject menuLoad;

        public Menu startingMenu;
        public bool canClose = true;
        public event Action OnMenuClose;

        private readonly Stack<Menu> stateStack = new Stack<Menu>();
        private Menu cachedMenu = Menu.None;

        private void Awake()
        {
            foreach (Menu state in Enum.GetValues(typeof(Menu)))
            {
                GameObject menu = GetStateMenu(state);

                if (menu)
                    menu.SetActive(false);
            }

            Open(startingMenu);
        }

        private void Update()
        {
            Menu current = Menu.None;
            if (stateStack.Count > 0)
                current = stateStack.Peek();

            if (current != cachedMenu)
            {
                foreach (Menu state in Enum.GetValues(typeof(Menu)))
                {
                    GameObject menu = GetStateMenu(state);

                    if (menu)
                    {
                        menu.SetActive(state == current);
                        foreach (IMenuCallback callback in menu.GetComponents<IMenuCallback>())
                            callback.OnMenuChanged(state == current);
                    }
                }

                cachedMenu = current;
            }
        }

        public void Open(Menu menu)
        {
            if (menu == Menu.None)
                return;
            stateStack.Push(menu);
        }

        public void Close()
        {
            if (stateStack.Count > 0 && (stateStack.Count > 1 || canClose))
                stateStack.Pop();

            if (stateStack.Count == 0)
            {
                Destroy(gameObject);
                if (OnMenuClose != null)
                    OnMenuClose();
            }
        }

        public void CloseAll()
        {
            while (stateStack.Count > 0 && (stateStack.Count > 1 || canClose))
                Close();
        }

        private GameObject GetStateMenu(Menu menu)
        {
            switch (menu)
            {
                case Menu.None:
                    return null;
                case Menu.MainMenu:
                    return menuMain;
                case Menu.PauseMenu:
                    return menuPause;
                case Menu.Settings:
                    return menuSettings;
                case Menu.LoadGame:
                    return menuLoad;
                default:
                    throw new ArgumentOutOfRangeException(nameof(menu), menu, null);
            }
        }

        [Serializable]
        public enum Menu
        {
            None,
            MainMenu,
            PauseMenu,
            Settings,
            LoadGame
        }

        public interface IMenuCallback
        {
            void OnMenuChanged(bool activated);
        }
    }
}