/*---------------------------------------------------------------
                            MAIN-MENU STATE
 ---------------------------------------------------------------*/
/***************************************************************
* The Main menu state contains the event handlers for the
  New Game, Join Game, Options and Exit buttons on the title
  screen of the game.
**************************************************************/


namespace Assets.Scripts.GameStates
{
    using System;
    using UnityEngine;
    using UnityEngine.UI;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.Util;
    using log4net;


    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private Toggle isSinglePlayer;
        public GameObject newGamePanel;
        public GameObject joinGamePanel;
        public GameObject notificationPanel;
        public Text name1;
        public Text name2;
        public Text gameIdField;
        public Text notificationMessage;

        private static readonly ILog log = LogManager.GetLogger(typeof(MainMenu));


        private void Start()
        {
            StateManager.currentState = GameState.MAIN_MENU;
            log.Info("Main menu startup complete.");
        }
        /*---------------------------------------------------------------
                            CLIENT EVENT HANDLERS
         ---------------------------------------------------------------*/
        /***************************************************************
        * Event handler for starting a new game, 
          tests the name entered is valid, and bubbles up any 
          error messages during Game.start().
        **************************************************************/
        public void enterNewGame()
        {
            if (!name1.text.Equals(""))
            {
                if (isSinglePlayer.isOn)
                {
                    Game.startOffline(name1.text);
                    StateManager.changeScene(GameState.CHARACTER_CREATION);
                }
                else
                {
                    if (Game.start(name1.text, Game.NEW_GAME, Game.NEW_GAME_ID))
                    {
                        StateManager.changeScene(GameState.CHARACTER_CREATION);
                    }
                    else
                    {
                        notificationMessage.text = Game.gameMessage;
                        togglePanel(newGamePanel);
                        togglePanel(notificationPanel);
                    }
                }
            }
        }

        /***************************************************************
        * Event handler for joining a current game, 
          tests the name entered is valid, and bubbles up any 
          error messages during Game.start().
        **************************************************************/
        public void enterJoinGame()
        {
            if (!name2.text.Equals("") && !gameIdField.Equals(""))
            {
                int gameIdEntered = -1;
                if (!Int32.TryParse(gameIdField.text, out gameIdEntered))
                {
                    log.Info("Non-integer value provided for gameID");
                    notificationMessage.text = "Game ID must be a number.";
                    togglePanel(notificationPanel);

                    return;
                }


                if (Game.start(name2.text, Game.JOINED_GAME, gameIdEntered))
                {
                    StateManager.changeScene(GameState.CHARACTER_CREATION);
                }
                else
                {
                    notificationMessage.text = Game.gameMessage;
                    togglePanel(joinGamePanel);
                    togglePanel(notificationPanel);
                }
            }
            else
            {
                notificationMessage.text = "Please enter a name and Game ID.";
                togglePanel(notificationPanel);
            }
        }

        /***************************************************************
        * Toggles the active state of the newGame and joinGame dialogue
          boxes.
        @param - panel: The Unity game object that represents the
        new game OR join game panels.
        **************************************************************/
        private void togglePanel(GameObject panel)
        {
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }

        /***************************************************************
        * Exits the application.
        **************************************************************/
        public void exitClicked()
        {
            Application.Quit();
        }
    }
}

