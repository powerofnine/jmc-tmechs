using JetBrains.Annotations;
using TMechs.UI;
using UnityEngine;

namespace TMechs.Environment
{
    public class CharacterIntroTrigger : MonoBehaviour
    {
        public GameObject template;
        public GameObject enemyToEnable;
    
        [UsedImplicitly]
        public void SpawnIntro()
        {
            CharacterIntroUtils utils = Instantiate(template).GetComponent<CharacterIntroUtils>();
            if (utils)
                utils.onIntroDone = () => enemyToEnable.SetActive(true);
            
            MenuActions.SetPause(true, false);
            MenuActions.pauseLocked = true;
        }
    }
}
