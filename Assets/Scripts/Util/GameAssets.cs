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
        //BattleState
        public Transform damagePopupPrefab;
        public Transform turnChevronPrefab;
        public Transform combatSpritePrefab;
        public Transform abilityButtonPrefab;
        public Transform buffBarPrefab;
        //Charactear creation
        public Transform playerPanelPrefab;
    }
}
