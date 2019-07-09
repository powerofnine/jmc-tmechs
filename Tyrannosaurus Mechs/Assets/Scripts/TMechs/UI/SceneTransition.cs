using System;
using System.Collections;
using System.IO;
using fuj1n.MinimalDebugConsole;
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

        public static bool alreadyLoading { get; private set; } = false;

        private void Awake()
        {
            DontDestroyOnLoad(this);

            instance = this;
            render = GetComponentInChildren<Image>();
            if (render)
                render.color = Color.clear;
        }

        [DebugConsoleCommand("level")]
        public static void LoadScene(string name)
        {
            int index = SceneUtility.GetBuildIndexByScenePath(name);
            
            if(index == -1)
                throw new FileNotFoundException($"Scene {name} not found");
            
            LoadScene(SceneUtility.GetBuildIndexByScenePath(name));
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

            if (alreadyLoading)
                throw new Exception("Attempted to load a scene whilst a scene was already loading");
            
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

            if (alreadyLoading)
                throw new Exception("Attempted to load a scene whilst a scene was already loading");

            return instance._LoadScene(index, sceneLoaded);
        }

        private IEnumerator _LoadScene(int index, Action sceneLoaded)
        {
            MenuActions.SetPause(true);
            alreadyLoading = true;

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
            alreadyLoading = false;
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