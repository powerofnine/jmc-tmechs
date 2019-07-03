using System;
using JetBrains.Annotations;
using TMechs.UI;
using UnityEngine;

namespace TMechs
{
    public class CharacterIntroUtils : MonoBehaviour
    {
        public Action onIntroDone;

        [UsedImplicitly]
        public void OnIntroDone()
        {
            MenuActions.pauseLocked = false;
            MenuActions.SetPause(false, false);
            Destroy(gameObject);

            if (onIntroDone != null)
                onIntroDone();
        }
    }
}
