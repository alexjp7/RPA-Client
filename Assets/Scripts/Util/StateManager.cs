namespace Assets.Scripts.Util
{
    using System;
    using UnityEngine;
    using UnityEngine.SceneManagement;
    using log4net;

    using Assets.Scripts.GameStates;

    public enum GameState
    {
        MAIN_MENU = 0,
        CHARACTER_CREATION = 1,
        BATTLE_STATE = 2,
    }

    public class StateManager : MonoBehaviour
    {
        public static GameState currentState;
        private static readonly ILog log = LogManager.GetLogger(typeof(StateManager));

        public static void changeScene(GameState scene)
        {
            log.Debug("Changing to: " + scene);
            currentState = scene;
            SceneManager.LoadScene((int)scene, LoadSceneMode.Single);
        }

        public static void setStateScript()
        {
            if (currentState == GameState.CHARACTER_CREATION) // Character creation
            {
                GameObject parent = GameObject.Find("UI");
                characterCreator = parent.GetComponent<CharacterCreator>();
            }
            else if (currentState == GameState.BATTLE_STATE) // Battle State
            {
                GameObject parent = GameObject.Find("UI");
                battleState = parent.GetComponent<BattleState>();
            }
        }

        //Game State refernces
        private static BattleState _battleState;
        public static BattleState battleState
        {
            get
            {
                if (_battleState == null)
                {
                    throw new NullReferenceException("Battle-State refernce is set to null, ensure during game state instantiation (during Awake()/Start()) call StateManager.setStateScript()");
                }

                return _battleState;
            }
            private set
            {
                _battleState = value;
            }
        }

        public static CharacterCreator characterCreator;
    }
}
