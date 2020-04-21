/*---------------------------------------------------------------
                            MAIN-MENU STATE
 ---------------------------------------------------------------*/
/***************************************************************
* The Main menu state contains the event handlers for the
  New Game, Join Game, Options and Exit buttons on the title
  screen of the game.
**************************************************************/

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Util;

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
            if(isSinglePlayer.isOn)
            {
                Game.isSinglePlayer = true;
                Game.startOffline(name1.text);  
                SceneManager.LoadScene(1);
            }
            else
            {
                if (Game.start(name1.text, Game.NEW_GAME, -1) )
                {
                    SceneManager.LoadScene(1);
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
        if (!name2.text.Equals("") && !gameIdField.Equals("") )
        {
            if(Game.start(name2.text, Game.JOINED_GAME, Int32.Parse(gameIdField.text)))
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                notificationMessage.text = Game.gameMessage;
                togglePanel(joinGamePanel);
                togglePanel(notificationPanel);
            }
        }
    }

    /***************************************************************
    * Toggles the active state of the newGame and joinGame dialogue
      boxes.
    @param - panel: The Unity game object that represents the
    new game OR join game panels.
    **************************************************************/
    public void togglePanel(GameObject panel)
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
