
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

    /// <summary>
    /// Acts a global scriptable object that stores refernces to prefabs.
    /// </summary>
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

        //Charactear creation
        public Transform playerPanelPrefab;

        //BattleState
        public Transform damagePopupPrefab;
        public Transform turnChevronPrefab;
        public Transform combatSpritePrefab;
        public Transform abilityButtonPrefab;
        public Transform buffBarPrefab;
        public Transform genericButtonPrefab;

        // Character Rigs
        public Transform warriorRig;
        public Transform clericRig;
        public Transform simpleRig;


    }
}
