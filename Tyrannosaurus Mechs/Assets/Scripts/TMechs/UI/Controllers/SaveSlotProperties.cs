using System.Globalization;
using TMechs.Data;
using TMPro;
using UnityEngine;

namespace TMechs.UI.Controllers
{
    public class SaveSlotProperties : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI meta;
        [SerializeField]
        private TextMeshProUGUI creationDate;
        [SerializeField]
        private TextMeshProUGUI id;

        private string creationDateFormat;
        private string idFormat;
        
        private void Awake()
        {
            if (creationDate)
                creationDateFormat = creationDate.text;

            if (id)
                idFormat = id.text;
        }

        public void Set(SaveSystem.LexiconEntry entry)
        {
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
    }
}