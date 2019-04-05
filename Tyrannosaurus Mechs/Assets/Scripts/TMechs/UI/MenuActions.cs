using TMechs.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMechs.UI
{
    public class MenuActions : MonoBehaviour
    {
        public const string FIRST_SCENE = "Assets/Scenes/Demo_Whitebox.unity";

        public MenuController controller;

        public void StartGame()
        {
            LoadGame(null);
        }

        public void LoadGame(SaveSystem.LexiconEntry save)
        {
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
                data = new SaveSystem.SaveData() {sceneId = sceneId};
            
            SaveSpawner.Spawn(data);
        }

        public void RestartLevel()
        {
            SceneTransition.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenMenu(MenuController.Menu menu)
        {
            if (controller)
                controller.Open(menu);
        }

        public void MainMenu()
        {
            SceneTransition.LoadScene(0);
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