using TMechs.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMechs.UI
{
    public class MenuActions : MonoBehaviour
    {
        public MenuController controller;
        
        public void StartGame()
        {
            LoadGame(null);
        }

        public void LoadGame(SaveSystem.LexiconEntry save)
        {
            
        }

        public void RestartLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void OpenMenu(MenuController.Menu menu)
        {
            if(controller)
                controller.Open(menu);
        }

        public void MainMenu()
        {
            SceneManager.LoadScene("0MainMenu");
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
