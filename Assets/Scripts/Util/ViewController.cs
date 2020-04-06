using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Util
{
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
            setStateScript(scene);
        }

        public void setStateScript(int scene)
        {
            if(scene == 0) //Main Menu
            {

            }
            else if(scene == 1) // Character creation
            {

            }
            else if(scene == 2) // Battle State
            {
                GameObject parent = GameObject.Find("UI");
                battleState = parent.GetComponent<BattleState>();
            }
        }

        //Game State refernces
        public BattleState battleState;
    }
}
