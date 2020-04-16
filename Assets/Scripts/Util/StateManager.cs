using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Util
{
    public enum GameState
    {
        MAIN_MENU = 0 ,
        CHARACTER_CREATION = 1,
        BATTLE_STATE = 2,
    }

    public class StateManager: MonoBehaviour
    {
        public static void changeScene(int scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
            setStateScript((GameState)scene);
        }

        public static void setStateScript(GameState scene)
        {
            if(scene == GameState.MAIN_MENU) //Main Menu
            {
                GameObject parent = GameObject.Find("Canvas");
                mainMenu = parent.GetComponent<MainMenu>();
            }
            else if(scene == GameState.CHARACTER_CREATION) // Character creation
            {

            }
            else if(scene == GameState.BATTLE_STATE) // Battle State
            {
                GameObject parent = GameObject.Find("UI");
                battleState = parent.GetComponent<BattleState>();
            }
        }

        //Game State refernces
        public static BattleState battleState;
        public static CharacterCreator characterCreator;
        public static MainMenu mainMenu;
    }
}
