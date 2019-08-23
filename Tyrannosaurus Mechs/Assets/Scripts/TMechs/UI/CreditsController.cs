using System.Collections;
using TMPro;
using UnityEngine;

namespace TMechs.UI
{
    public class CreditsController : MonoBehaviour
    {
        public AudioClip creditsAudio;
        
        public CanvasGroup canvas;
        public RectTransform scrollRect;
        public TextMeshProUGUI text;
        
        public float scrollSpeed = 5F;

        private float size;

        private bool ended;
        
        private void Awake()
        {
            size = text.preferredHeight + 2200F;
        }

        private void Update()
        {
            if (ended)
                return;
            
            scrollRect.anchoredPosition += scrollSpeed * Time.unscaledDeltaTime * Vector2.up;

            if (scrollRect.anchoredPosition.y > size)
            {
                ended = true;
                SceneTransition.LoadScene(1);
            }
        }

        public void RollCredits()
        {
            gameObject.SetActive(true);
            StartCoroutine(Fade());

            AudioSource src = GameObject.Find("Audio").GetComponent<AudioSource>();
            src.clip = creditsAudio;
            src.Play();
        }

        private IEnumerator Fade()
        {
            float time = 0F;

            while (time <= .5F)
            {
                time += Time.unscaledDeltaTime;
                canvas.alpha = time / .5F;

                yield return null;
            }
        }
    }
}
