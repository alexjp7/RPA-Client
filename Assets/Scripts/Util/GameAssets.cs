/*---------------------------------------------------------------
                       GAME-ASSETS
 ---------------------------------------------------------------*/
/***************************************************************
 *  Scriptable object for storing references to prefabs in
    the project
**************************************************************/
namespace Assets.Scripts.Util
{
    using UnityEngine;

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

        //General
        public Transform timedPanelPrefab;
        public Transform inventorySlotPrefab;

        //BattleState
        public Transform damagePopupPrefab;
        public Transform turnChevronPrefab;
        public Transform combatSpritePrefab;
        public Transform abilityButtonPrefab;
        public Transform buffBarPrefab;
        public Transform genericButtonPrefab;
        //Charactear creation
        public Transform playerPanelPrefab;
    }
}
