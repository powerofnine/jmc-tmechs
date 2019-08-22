using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UIEventDelegate;
using UnityEngine;
using UnityEngine.UI;

namespace TMechs.UI.Components
{
    public class UiModal : MonoBehaviour
    {
        public RectTransform windowAnchor;

        public TextMeshProUGUI textDisplay;
        public RectTransform scrollRect;
        public RectTransform buttonsAnchor;
        public GameObject buttonTemplate;

        public static string Result { get; private set; }
        private bool isDone;

        private readonly List<UiButton> buttons = new List<UiButton>();
        private int selectedButton;

        public IEnumerator Show(string text, IEnumerable<string> buttons, int startIndex)
        {
            float height = 25F;
            float textBottom = 10F;

            if (textDisplay)
            {
                textDisplay.text = text;
                textDisplay.ForceMeshUpdate();

                height += textDisplay.preferredHeight;
                textBottom += textDisplay.preferredHeight;
            }

            if (buttonTemplate && buttonsAnchor)
            {
                foreach (string button in buttons)
                {
                    GameObject ob = Instantiate(buttonTemplate, buttonsAnchor);
                    RectTransform trs = (RectTransform) ob.transform;

                    height += trs.sizeDelta.y + 5F;

                    TextMeshProUGUI textCmp = ob.GetComponentInChildren<TextMeshProUGUI>();
                    if (textCmp)
                        textCmp.text = button;

                    UiButton btn = ob.GetComponent<UiButton>();
                    if (btn)
                    {
                        EventDelegate ev = new EventDelegate(this, nameof(OnButton));

                        ev.parameters[0].argStringValue = button;

                        if (btn.onClick.List == null)
                            btn.onClick.List = new List<EventDelegate>();

                        EventDelegate.Add(btn.onClick.List, ev);

                        this.buttons.Add(btn);
                        btn.OnDeselect(true);
                    }
                }

                if (startIndex >= this.buttons.Count)
                    startIndex = 0;

                if (this.buttons.Count > startIndex)
                {
                    selectedButton = startIndex;
                    this.buttons[startIndex].OnSelect(true);
                }
            }

            height = Mathf.Min(height, Screen.height * .85F);
            
            if (windowAnchor)
                windowAnchor.sizeDelta = new Vector2(windowAnchor.sizeDelta.x, height);
            scrollRect.anchoredPosition = new Vector2(buttonsAnchor.anchoredPosition.x, -textBottom);
            scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, height - 25F);

            while (!isDone)
                yield return null;

            Destroy(gameObject);
        }

        public void NavigateUp() => UpdateSelection(selectedButton - 1);
        public void NavigateDown() => UpdateSelection(selectedButton + 1);

        public void Submit()
        {
            if (isDone || buttons.Count == 0)
                return;

            if (buttons[selectedButton])
            {
                buttons[selectedButton].OnSubmit();
                MenuAudio.Accept();
            }
        }

        public void Cancel()
        {
            OnButton("Cancelled");
            MenuAudio.Back();
        }

        private void UpdateSelection(int newSelection)
        {
            if (isDone || buttons.Count == 0)
                return;

            if (newSelection < 0)
                newSelection = buttons.Count - 1;
            if (newSelection >= buttons.Count)
                newSelection = 0;

            if (newSelection == selectedButton)
                return;

            if (selectedButton < buttons.Count)
                buttons[selectedButton].OnDeselect(false);

            selectedButton = newSelection;

            if (selectedButton < buttons.Count)
                buttons[selectedButton].OnSelect(false);

            // Scroll
            RectTransform rect = (RectTransform) buttons[selectedButton].transform;
            float scrolledPos = rect.anchoredPosition.y + buttonsAnchor.anchoredPosition.y;

            if (scrolledPos > -rect.sizeDelta.y)
                buttonsAnchor.anchoredPosition = new Vector2(0F, -rect.anchoredPosition.y - rect.sizeDelta.y);
            else if (scrolledPos < -scrollRect.sizeDelta.y)
                buttonsAnchor.anchoredPosition = new Vector2(0F, -scrollRect.sizeDelta.y - rect.anchoredPosition.y);
        }

        public void OnButton(string button)
        {
            if (isDone)
                return;

            Result = button;
            isDone = true;
        }
    }
}