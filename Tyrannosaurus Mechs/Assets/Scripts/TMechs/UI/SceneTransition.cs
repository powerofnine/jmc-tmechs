using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace TMechs.UI
{
    public class SceneTransition : MonoBehaviour
    {
        private const string LOADING = "Assets/Scenes/Utility/Loading.unity";

        private static SceneTransition instance;

        public float transitionTime = 1F;
        private Image render;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            instance = this;
            render = GetComponentInChildren<Image>();
            if (render)
                render.color = Color.clear;
        }

        public static void LoadScene(int index, Action sceneLoaded = null)
        {
            if (!instance)
            {
                Debug.LogError("No scene transition object exists");
                SceneManager.LoadScene(index);
                if (sceneLoaded != null)
                    sceneLoaded();
                return;
            }

            instance.StartCoroutine(instance._LoadScene(index, sceneLoaded));
        }

        public static IEnumerator LoadSceneManual(int index, Action sceneLoaded = null)
        {
            if (!instance)
            {
                Debug.LogError("No scene transition object exists");
                SceneManager.LoadScene(index);
                if (sceneLoaded != null)
                    sceneLoaded();
                return null;
            }

            return instance._LoadScene(index, sceneLoaded);
        }

        private IEnumerator _LoadScene(int index, Action sceneLoaded)
        {
            MenuActions.SetPause(true);

            yield return StartCoroutine(Fade(1F));
            yield return SceneManager.LoadSceneAsync(LOADING);
            yield return StartCoroutine(Fade(0F));

            AsyncOperation mainSceneLoad = SceneManager.LoadSceneAsync(index);
            mainSceneLoad.allowSceneActivation = false;
            while (mainSceneLoad.progress < 0.9F)
                yield return null;

            yield return StartCoroutine(Fade(1F));
            mainSceneLoad.allowSceneActivation = true;

            yield return mainSceneLoad;
            if (sceneLoaded != null)
                sceneLoaded();

            yield return new WaitForSecondsRealtime(.25F);
            MenuActions.SetPause(false);
            yield return StartCoroutine(Fade(0F));
        }

        private IEnumerator Fade(float alpha)
        {
            float time = 0F;

            Color start = Color.black;
            start.a = render.color.a;
            Color end = start;
            end.a = alpha;

            while (time < transitionTime)
            {
                time += Time.unscaledDeltaTime;
                render.color = Color.Lerp(start, end, time / transitionTime);
                yield return null;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Instantiate(Resources.Load<GameObject>("UI/Transition"));
        }
    }
}