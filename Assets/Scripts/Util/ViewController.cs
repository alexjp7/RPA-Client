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

    public class ViewController 
    {
        private static ViewController _instace;
        public static ViewController INSTANCE
        {
            get
            {
                if(_instace == null)
                {
                    _instace = new ViewController();
                }
                return _instace;
            }
        }

        public void changeScene(int scene)
        {
            SceneManager.LoadScene(scene, LoadSceneMode.Single);
            setStateScript((GameState)scene);
        }

        public void setStateScript(GameState scene)
        {
            if(scene == GameState.MAIN_MENU) //Main Menu
            {

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
        public BattleState battleState;
        public CharacterCreator characterCreator;
        public MainMenu mainMenu;
    }
}
