using fuj1n.MinimalDebugConsole;
using JetBrains.Annotations;
using TMechs.UI;
using UnityEngine;

namespace TMechs.Environment
{
    public class CharacterIntroTrigger : MonoBehaviour
    {
        public GameObject template;
        public GameObject enemyToEnable;
        public Transform playerAnchor;
    
        [UsedImplicitly]
        public void SpawnIntro()
        {
            CharacterIntroUtils utils = Instantiate(template).GetComponent<CharacterIntroUtils>();
            if (utils)
                utils.onIntroDone = OnEnd;
            
            MenuActions.SetPause(true, false);
            MenuActions.pauseLocked = true;
        }

        private void OnEnd()
        {
            enemyToEnable.SetActive(true);
            TeleportPlayer();
        }

        public void TeleportPlayer()
        {
            if (playerAnchor)
            {
                Player.Player.Instance.forces.Teleport(playerAnchor.position);
                Player.Player.Instance.movement.intendedY = playerAnchor.eulerAngles.y;
                Player.Player.Instance.transform.eulerAngles = Player.Player.Instance.transform.eulerAngles.Set(playerAnchor.eulerAngles.y, Utility.Axis.Y);
            }
        }
        
        [DebugConsoleCommand("coc")]
        private static void DebugTeleport(string name)
        {
            if (name.ToLower().Equals("list"))
            {
                DebugConsole.Instance.AddMessage("<#00FFFF>List of areas:</color>\nBossArea", Color.white);
                return;
            }
            
            if (!name.ToLower().Equals("bossarea"))
                return;
            //TODO better anchor system

            FindObjectOfType<CharacterIntroTrigger>().TeleportPlayer();
        }

        [DebugConsoleCommand("coc")]
        private static void DebugTeleport()
            => DebugTeleport("list");
    }
}
