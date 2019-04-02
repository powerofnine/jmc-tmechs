using System;
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
                
                if(currentComponent[currentTab] < 0 || currentComponent[currentTab] > components.Length)
                    SetComponent(0);
                
                return components[currentComponent[currentTab]];
            }
        }

        protected override void Start()
        {
            base.Start();

            controller = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            
            if (tabs == null || tabs.Length == 0)
                throw new ArgumentException("No tabs given");
            if(!tabView)
                throw new ArgumentException("No tab view provided");
            if(!tabTemplate)
                throw new ArgumentException("No tab template provided");
            
            toggles = new Toggle[tabs.Length];
            currentComponent = new int[tabs.Length];
            
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
                    
                    tab.sizeDelta = new Vector2(text.preferredWidth + 20F, tab.sizeDelta.y);
                }

                toggles[i] = tab.GetComponent<Toggle>();
                SetTab(0);
            }
        }

        private void Update()
        {
            int tab = currentTab;
            
            if (controller.GetButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateRight()))
                tab++;
            else if (controller.GetNegativeButtonDown(Action.UIHORIZONTAL) && (!CurrentComponent || !CurrentComponent.NavigateLeft()))
                tab--;

            if (tab < 0)
                tab = tabs.Length - 1;
            else if (tab >= tabs.Length)
                tab = 0;

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

            if (component < 0)
                component = components.Length - 1;
            if (component >= components.Length)
                component = 0;

            if (component != currentComponent[currentTab])
            {
                SetComponent(component);
                return;
            }

            if (controller.GetButtonDown(Action.UISUBMIT))
            {
                if(CurrentComponent)
                    CurrentComponent.OnSubmit();
            }
            
            if (controller.GetButtonDown(Action.UICANCEL) && (!CurrentComponent || !CurrentComponent.OnCancel()))
            {
                // TODO: exit settings screen
            }
        }

        private void SetTab(int id)
        {
            for (int i = 0; i < tabs.Length; i++)
            {
                tabs[i].SetActive(id == i);
                if (toggles[i])
                {
                    toggles[i].isOn = id == i;
                }
            }
            
            currentTab = id;

            components = tabs[id].GetComponentsInChildren<UiComponent>();
            foreach (UiComponent component in components)
                component.OnDeselect(true);
            SetComponent(currentComponent[id], true);
        }

        private void SetComponent(int id, bool noTransition = false)
        {
            if(CurrentComponent)
                CurrentComponent.OnDeselect(noTransition);
            currentComponent[currentTab] = id;
            if(CurrentComponent)
                CurrentComponent.OnSelect(noTransition);
        }


    }
}
