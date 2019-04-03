using System.Linq;
using TMechs.Data;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class LoadGameUi : MonoBehaviour, MenuController.IMenuCallback
    {
        public GameObject saveSlotTemplate;
        public RectTransform saveSlotRoot;

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
                ui.GetComponent<SaveSlotProperties>()?.Set(entry);
            }
            
            saveSlotRoot.GetComponentInParent<UiNavigation>().RefreshComponents();
        }

        public void OnMenuChanged(bool activated)
        {
            if(activated)
                RefreshUi();
        }
    }
}