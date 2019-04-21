using System.Linq;
using Rewired;
using TMechs.Controls;
using TMechs.Data;
using TMechs.UI.Components;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class LoadGameUi : MonoBehaviour, MenuController.IMenuCallback
    {
        public MenuActions menuActions;
        public GameObject saveSlotTemplate;
        public RectTransform saveSlotRoot;

        public RectTransform scrollRect;
        public RectTransform scrollTarget;

        private Rewired.Player input;
        private UiNavigation navigation;

        private SaveSystem.LexiconEntry selectedEntry;
        
        private void Awake()
        {
            input = ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER);
            navigation = GetComponentInParent<UiNavigation>();
        }

        private void Update()
        {
            if(selectedEntry != null && !navigation.IsModal && input.GetButtonDown(Action.UIALTERNATE))
                navigation.OpenModal("Are you sure you want to delete this save? This cannot be undone!", new [] {"No", "Yes"}, Delete);
                
        }

        private void Delete(string confirm)
        {
            if (!"Yes".Equals(confirm) || selectedEntry == null)
                return;
         
            SaveSystem.DeleteSave(selectedEntry);
            RefreshUi();
        }

        public void RefreshUi()
        {
            if (!saveSlotRoot || !saveSlotTemplate)
                return;

            foreach(Transform child in saveSlotRoot)
                Destroy(child.gameObject);
            
            SaveSystem.LexiconEntry[] entries = SaveSystem.GetLexicon();

            foreach (SaveSystem.LexiconEntry entry in entries.OrderByDescending(x => x.creationTime))
            {
                GameObject ui = Instantiate(saveSlotTemplate, saveSlotRoot);
                UiSaveSlot slot = ui.GetComponent<UiSaveSlot>();

                if (slot)
                {
                    slot.Set(entry);
                    slot.menuActions = menuActions;
                    slot.parent = this;
                }
            }
            
            saveSlotRoot.GetComponentInParent<UiNavigation>().RefreshComponents();
        }

        public void OnMenuChanged(bool activated)
        {
            if(activated)
                RefreshUi();
        }

        public void OnSlotSelected(RectTransform rect, SaveSystem.LexiconEntry entry)
        {
            if (!scrollRect || !scrollTarget)
                return;

            float scrolledPos = rect.anchoredPosition.y + scrollTarget.anchoredPosition.y;
            
            if (scrolledPos > 0)
                scrollTarget.anchoredPosition = new Vector2(0F, -rect.anchoredPosition.y - rect.sizeDelta.y);
            else if(scrolledPos < -scrollRect.sizeDelta.y)
                scrollTarget.anchoredPosition = new Vector2(0F, -scrollRect.sizeDelta.y - rect.anchoredPosition.y);

            selectedEntry = entry;
        }
    }
}