using TMechs.Data;
using TMechs.UI.Controllers;
using TMPro;
using UnityEngine;

namespace TMechs.UI.Components
{
    public class UiSaveSlot : UiButton
    {
        [HideInInspector]
        public MenuActions menuActions;
        [HideInInspector]
        public LoadGameUi parent;

        [SerializeField]
        private TextMeshProUGUI meta;
        [SerializeField]
        private TextMeshProUGUI creationDate;
        [SerializeField]
        private TextMeshProUGUI id;

        private string creationDateFormat;
        private string idFormat;

        private SaveSystem.LexiconEntry entry;

        protected override void Awake()
        {
            base.Awake();

            if (creationDate)
                creationDateFormat = creationDate.text;

            if (id)
                idFormat = id.text;
        }

        public void Set(SaveSystem.LexiconEntry entry)
        {
            this.entry = entry;

            if (meta)
                meta.text = entry.meta;
            if (creationDate)
            {
                string date = entry.creationTime.ToShortDateString();
                string time = entry.creationTime.ToLongTimeString();

                creationDate.text = string.Format(creationDateFormat, date, time);
            }

            if (id)
                id.text = string.Format(idFormat, entry.id);
        }

        public override void OnSubmit()
        {
            base.OnSubmit();

            if (menuActions)
                menuActions.LoadGame(entry);
        }

        protected override void OnSelect()
        {
            base.OnSelect();

            if (parent)
                parent.OnSlotSelected((RectTransform) transform, entry);
        }
    }
}