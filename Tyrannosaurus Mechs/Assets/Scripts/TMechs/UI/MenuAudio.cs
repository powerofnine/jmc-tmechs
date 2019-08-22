using UnityEngine;

namespace TMechs.UI
{
    public class MenuAudio : MonoBehaviour
    {
        private static MenuAudio instance;

        [Header("Audio")]
        public AudioSource accept;
        public AudioSource back;
        public AudioSource scroll;

        private void Awake()
        {
            if (instance)
                return;

            instance = this;
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            if (accept)
                accept.ignoreListenerPause = true;
            if (back)
                back.ignoreListenerPause = true;
            if (scroll)
                scroll.ignoreListenerPause = true;
        }

        public static void Accept()
            => instance.Play(instance.accept);

        public static void Back()
            => instance.Play(instance.back);   
        
        public static void Scroll()
            => instance.Play(instance.scroll);

        private void Play(AudioSource source)
        {
            if (source)
            {
                source.time = 0F;
                source.Play();
            }
        }
    }
}