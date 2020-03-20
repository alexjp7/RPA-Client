using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public GameObject newGamePanel;
    public GameObject joinGamePanel;
    public GameObject notificationPanel;
    public Text name1;
    public Text name2;
    public Text gameIdField;
    public Text notificationMessage;

    /****************Button Event Listeners*******************/
    public void enterNewGame()
    {
        if (!name1.text.Equals(""))
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
   
    public void togglePanel(GameObject panel)
    {
        if (panel != null)
            panel.SetActive(!panel.activeSelf);
    }

    public void exitClicked()
    {
        Application.Quit();
    }
}
