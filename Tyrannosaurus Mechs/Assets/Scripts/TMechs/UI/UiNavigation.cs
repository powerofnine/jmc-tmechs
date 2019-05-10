using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rewired;
using TMechs.UI.Components;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Action = TMechs.Controls.Action;

namespace TMechs.UI
{
    public class UiNavigation : UIBehaviour
    {
        public NavigationShouldCloseEvent closeAction;
        public UiModal modalTemplate;

        [Header("Tabs (optional)")]
        public GameObject[] tabs;
        public string[] tabNames;

        public RectTransform tabView;
        public GameObject tabTemplate;

        private Rewired.Player controller;

        private int currentTab;
        private Toggle[] toggles;

        private UiComponent[] components;
        private int[] currentComponent;

        private UiComponent CurrentComponent
        {
            get
            {
                if (components.Length == 0)
                    return null;

                List<UiComponent> active = ActiveComponents;

                if (currentComponent[currentTab] < 0 || currentComponent[currentTab] >= active.Count)
                    SetComponent(0);

                return active[currentComponent[currentTab]];
            }
        }

        private List<UiComponent> ActiveComponents => components.Where(x => x && x.IsActive()).ToList();

        private UiModal modal;
        public bool IsModal => modal;

        protected override void Start()
        {
            base.Start();

            controller = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);

            if (tabs == null || tabs.Length == 0)
                tabs = new[] {gameObject};

            toggles = new Toggle[tabs.Length];
            currentComponent = new int[tabs.Length > 0 ? tabs.Length : 1];

            if (tabView)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    string name = tabs[i].name;
                    if (tabNames != null && i < tabNames.Length)
                        name = tabNames[i];

                    RectTransform tab = Instantiate(tabTemplate, tabView).GetComponent<RectTransform>();

                    TextMeshProUGUI text = tab.GetComponentInChildren<TextMeshProUGUI>();
                    if (text)
                    {
                        text.text = name;
                        text.ForceMeshUpdate();

                        tab.sizeDelta = new Vector2(text.preferredWidth + 40F, tab.sizeDelta.y);
                    }

                    toggles[i] = tab.GetComponent<Toggle>();
                }
            }

            SetTab(0);
        }

        private void Update()
        {
            if (modal)
            {
                if (controller.GetButtonDown(Action.UIVERTICAL))
                    modal.NavigateDown();
                else if (controller.GetNegativeButtonDown(Action.UIVERTICAL))
                    modal.NavigateUp();

                if (controller.GetButtonDown(Action.UISUBMIT))
                    modal.Submit();
                else if (controller.GetButtonDown(Action.UICANCEL))
                    modal.Cancel();

                return;
            }

            int tab = currentTab;

            if (controller.GetButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateRight()))
                tab++;
            else if (controller.GetNegativeButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateLeft()))
                tab--;

            if (tab != currentTab)
            {
                SetTab(tab);
                return;
            }

            int component = currentComponent[currentTab];

            if (controller.GetButtonDown(Action.UIVERTICAL) && (!CurrentComponent || !CurrentComponent.NavigateDown()))
                component--;
            else if (controller.GetNegativeButtonDown(Action.UIVERTICAL) && (!CurrentComponent || !CurrentComponent.NavigateUp()))
                component++;

            if (component != currentComponent[currentTab])
            {
                SetComponent(component);
                return;
            }

            if (controller.GetButtonDown(Action.UISUBMIT))
            {
                if (CurrentComponent)
                    CurrentComponent.OnSubmit();
            }

            if (controller.GetButtonDown(Action.UICANCEL) && (!CurrentComponent || !CurrentComponent.OnCancel()))
            {
                if (closeAction != null)
                    closeAction.Invoke();
            }
        }

        public void RefreshComponents(bool instantEffect = false)
        {
            if (tabs == null || tabs.Length == 0)
                return;

            components = tabs[currentTab].GetComponentsInChildren<UiComponent>(true);
            List<UiComponent> active = ActiveComponents;

            foreach (UiComponent component in active)
                component.OnDeselect(instantEffect);
            if (active.Count > 0)
                active[currentComponent[currentTab]].OnSelect(instantEffect);
        }

        public void OpenModal(string text, IEnumerable<string> buttons, Action<string> callback = null)
            => StartCoroutine(ModalWindow(text, buttons, callback));

        public IEnumerator ModalWindow(string text, IEnumerable<string> buttons, Action<string> callback)
        {
            if (!modalTemplate)
            {
                Debug.LogError("No modal window template assigned");
                yield break;
            }

            modal = Instantiate(modalTemplate, transform);
            yield return modal.Show(text, buttons);
            modal = null;

            if (callback != null)
                callback(UiModal.Result);
        }

        private void SetTab(int id)
        {
            if (id < 0)
                id = tabs.Length - 1;
            else if (id >= tabs.Length)
                id = 0;

            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetActive(id == i);
                if (toggles[i])
                {
                    toggles[i].isOn = id == i;
                }
            }

            currentTab = id;

            RefreshComponents(true);
        }

        private void SetComponent(int id, bool noTransition = false)
        {
            List<UiComponent> active = ActiveComponents;

            if (active.Count <= 0)
                return;

            if (id < 0)
                id = active.Count - 1;
            if (id >= active.Count)
                id = 0;

            if (active[currentComponent[currentTab]])
                active[currentComponent[currentTab]].OnDeselect(noTransition);
            currentComponent[currentTab] = id;
            if (active[currentComponent[currentTab]])
                active[currentComponent[currentTab]].OnSelect(noTransition);
        }

        [Serializable]
        public class NavigationShouldCloseEvent : UnityEvent
        {
        }

        public void OnMenuChanged(bool activated)
        {
            if(currentComponent != null)
                SetComponent(currentComponent[currentTab], true);
        }
    }
}