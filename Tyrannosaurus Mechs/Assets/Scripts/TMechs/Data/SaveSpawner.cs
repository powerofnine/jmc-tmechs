using TMechs.Environment;
using TMechs.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TMechs.Data
{
    public class SaveSpawner : MonoBehaviour
    {
        private SaveSystem.SaveData data;

        public static void Spawn(SaveSystem.SaveData data)
        {
            GameObject go = new GameObject("Player Spawner");
            DontDestroyOnLoad(go);

            go.AddComponent<SaveSpawner>().data = data;
        }

        private void Start()
        {
            int scene = SceneUtility.GetBuildIndexByScenePath(data.sceneId);

            SceneTransition.LoadScene(scene, SpawnPlayer);
        }

        private void SpawnPlayer()
        {
            if (!string.IsNullOrWhiteSpace(data.checkpointId))
                Checkpoint.CheckpointRegistry.Instance.MovePlayerTo(data.checkpointId);
            
            Player.Player.Instance.LoadPlayerData(data);

            Destroy(gameObject);
        }
    }
}