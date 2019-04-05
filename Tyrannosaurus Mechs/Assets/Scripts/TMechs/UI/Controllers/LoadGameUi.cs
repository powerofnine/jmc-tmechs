using System.Linq;
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

        public void RefreshUi()
        {
            if (!saveSlotRoot || !saveSlotTemplate)
                return;

            foreach(Transform child in saveSlotRoot)
                Destroy(child.gameObject);
            
            SaveSystem.LexiconEntry[] entries = SaveSystem.GetLexicon();

            for(int i = 0; i < 50; i++)
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

        public void OnSlotSelected(RectTransform rect)
        {
            if (!scrollRect || !scrollTarget)
                return;

            float scrolledPos = rect.anchoredPosition.y + scrollTarget.anchoredPosition.y;
            
            if (scrolledPos > 0)
                scrollTarget.anchoredPosition = new Vector2(0F, -rect.anchoredPosition.y - rect.sizeDelta.y);
            else if(scrolledPos < -scrollRect.sizeDelta.y)
                scrollTarget.anchoredPosition = new Vector2(0F, -scrollRect.sizeDelta.y - rect.anchoredPosition.y);
        }
    }
}