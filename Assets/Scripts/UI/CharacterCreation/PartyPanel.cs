﻿namespace Assets.Scripts.UI.CharacterCreation
{
    using UnityEngine.UI;
    using UnityEngine;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Players;

    /// <summary>
    /// UI component for creating party members, to display their class, ready status and player name in the character creation lobby.
    /// </summary>
    public class PartyPanel : MonoBehaviour
    {
        private Text playerName { get; set; }
        public int playerId { get; private set; }
        public Image classIcon { get; set; }
        public Image readyCheck { get; set; }

        public static PartyPanel create(in Player player)
        {
            Transform panelTransform = Instantiate(GameAssets.INSTANCE.playerPanelPrefab, Vector2.zero, Quaternion.identity);
            PartyPanel panel = panelTransform.GetComponent<PartyPanel>();
            panel.setData(player);

            return panel;
        }

        private void setData(Player player)
        {
            if (player.isClientPlayer)
            {
                gameObject.GetComponent<Image>().color = new Color(255, 226, 0);
            }

            playerId = player.id;
            playerName.text = player.name;
            readyCheck.sprite = AssetLoader.getSprite("tick");

            setReadyStatus(player.ready);
            setClass(player.adventuringClass);
        }

        public void setReadyStatus(bool isReady)
        {
            readyCheck.gameObject.SetActive(isReady);
        }

        public void setClass(int classChoice)
        {
            bool isValidChoice;
            if (isValidChoice = classChoice > -1)
            {
                classIcon.enabled = isValidChoice;
                classIcon.sprite = AssetLoader.getSprite(((PlayerClasses)classChoice).ToString().ToLower() + "_icon", null, true);
            }
            else
            {
                classIcon.enabled = isValidChoice;
            }
        }

        void Awake()
        {
            playerName = gameObject.transform.Find("text_name").GetComponent<Text>();
            classIcon = gameObject.transform.Find("class_icon").GetComponent<Image>();
            readyCheck = gameObject.transform.Find("ready_check").GetComponent<Image>();
        }

        void Update()
        {

        }
    }
}

