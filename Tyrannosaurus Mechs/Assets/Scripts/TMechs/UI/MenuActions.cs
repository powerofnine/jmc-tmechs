using System;
using System.Collections;
using System.Linq;
using fuj1n.MinimalDebugConsole;
using Rewired;
using TMechs.Data;
using TMechs.FX;
using TMechs.UI.Components;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMechs.UI
{
    public class MenuActions : MonoBehaviour
    {
        public static bool pauseLocked;
        public const string FIRST_SCENE = "Assets/Scenes/Level1.unity";

        public MenuController controller;

        public void StartGame()
        {
            LoadGame(null);
        }

        public void LoadLatest()
        {
            LoadGame(SaveSystem.GetLexicon().FirstOrDefault());
        }
        
        public void LoadGame(SaveSystem.LexiconEntry save)
        {
            if (SceneTransition.alreadyLoading)
                return;
            
            SaveSystem.SaveData data = null;
            if (save != null)
                data = SaveSystem.LoadSave(save);

            string sceneId = FIRST_SCENE;

            if (data != null)
                sceneId = data.sceneId;

            if (SceneUtility.GetBuildIndexByScenePath(sceneId) == -1)
            {
                Debug.LogErrorFormat("Scene ID {0} does not exist", sceneId);
                return;
            }

            if (data == null)
                data = new SaveSystem.SaveData {sceneId = sceneId};

            SaveSpawner.Spawn(data);
        }

        public void OpenMenu(MenuController.Menu menu)
        {
            if (controller)
                controller.Open(menu);
        }

        public void RestartLevel(bool bypassConfirm = false)
        {
            StartCoroutine(Confirm(() => SceneTransition.LoadScene(SceneManager.GetActiveScene().buildIndex), bypassConfirm, "restart the current level from the beginning"));
        }

        public void MainMenu(bool bypassConfirm = false)
        {
            StartCoroutine(Confirm(() => SceneTransition.LoadScene(0), bypassConfirm, "return to main menu"));
        }

        public IEnumerator Confirm(Action action, bool bypass, string message)
        {
            if (!bypass && controller)
            {
                UiNavigation menu = controller.GetCurrentMenu().GetComponent<UiNavigation>();

                if (menu)
                    yield return menu.ModalWindow($"Are you sure you want to {message}?", new[] {"No", "Yes"}, null);

                if (!"Yes".Equals(UiModal.Result))
                    yield break;
            }

            action?.Invoke();
        }

        public void _SetPause(bool pause)
        {
            SetPause(pause);
        }
        
        [DebugConsoleCommand("pause")]
        public static void SetPause(bool pause) => SetPause(pause, true);
        
        public static void SetPause(bool pause, bool doBlur)
        {
            if (pauseLocked)
                return;
            
            Time.timeScale = pause ? 0F : 1F;
            ReInput.players.GetPlayer(Controls.Player.MAIN_PLAYER).controllers.maps.SetMapsEnabled(!pause, Controls.Category.DEFAULT);

            if(doBlur)
                BlurFade.Fade(pause);
        }

        public void ExitGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}