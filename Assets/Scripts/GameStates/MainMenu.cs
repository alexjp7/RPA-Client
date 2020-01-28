using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    public GameObject newGamePanel;
    public GameObject joinGamePanel;
    public Text name1;
    public Text name2;
    public Text gameIdField;


    public void newGameClicked()
    {
        if (!name1.text.Equals(""))
        {
            Game.start(name1.text, Game.NEW_GAME, -1);
            SceneManager.LoadScene(1);
        }
    }
    
    public void joinGameClicked()
    {
        if (!name2.text.Equals("") && !gameIdField.Equals(""))
        {
            Game.start(name2.text,Game.JOINED_GAME ,Int32.Parse(gameIdField.text));
            SceneManager.LoadScene(1);
        }
    }

    public void openNewGamePanel()
    {
        if(newGamePanel != null)
        {
            newGamePanel.SetActive(true);
        }
    }

    public void openJoinGamePanel()
    {
        if(joinGamePanel != null)
        {
            joinGamePanel.SetActive(true);
        }
    }
    
    public void closeNewGamePanel()
    {
        newGamePanel.SetActive(false);
    }

    public void closeJoinGamePanel()
    {
        joinGamePanel.SetActive(false);
    }



    void Awake()
    {

    }
    private void Update()
    {

    }
    
    private void Start()
    {
    }


}
