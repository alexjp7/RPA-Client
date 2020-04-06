/*---------------------------------------------------------------
                       GAME-ASSETS
 ---------------------------------------------------------------*/
/***************************************************************
 *  Scriptable object for storing references to prefabs in
    the project
**************************************************************/

using UnityEngine;

namespace Assets.Scripts.Util
{
    public class GameAssets: MonoBehaviour
    {
        private static GameAssets _instance;

        public static GameAssets INSTANCE
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Instantiate(Resources.Load< GameAssets>("prefabs/GameAssets"));
                }

                return _instance;
            }
        }

        public Transform damagePopupPrefab;
        public Transform turnChevron;
        public Transform combatSpritePrefab;
        public Transform abilityButtonPrefab;
    }
}
