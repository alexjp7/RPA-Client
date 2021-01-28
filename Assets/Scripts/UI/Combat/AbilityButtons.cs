namespace Assets.Scripts.UI.Combat
{
    using Assets.Scripts.UI.Common;
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    using Assets.Scripts.Entities.Containers;
    using Assets.Scripts.Entities.Players;
    using UnityEngine.UI;

    /// <summary>
    /// UI container class for implementing a tab component to be added to the combat panel
    /// </summary>
    class AbilityButtons : TabComponent
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(AbilityButtons));

        public static List<AbilityButton> buttons;

        public Abilities playerAbilities;

        /// <summary>
        /// The abilitiy that was last clicked on, used for toggling UI indicators.
        /// </summary>
        private static int lastSelected = -1;

        public static AbilityButtons create(in Abilities abilities)
        {
            GameObject gameObject = new GameObject("ability_buttons");
            AbilityButtons buttons = gameObject.AddComponent<AbilityButtons>();
            buttons.setData(abilities);

            return buttons;
        }

        /// <summary>
        /// Sets horizontal layout group for abilitiy buttons.
        /// </summary>
        /// <param name="panel"></param>
        private void setData(in Abilities abilities)
        {
            this.playerAbilities = abilities;
            this.tabName = "Abilites";

            buttons = new List<AbilityButton>();

            HorizontalLayoutGroup layoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = false;
            layoutGroup.childControlWidth = false;
            layoutGroup.spacing = 10;

            generateAbilityButtons();
        }


        /// <summary>
        /// Generates <see cref="AbilityButton"/> UI components for each of client side players abilities.
        /// </summary>
        public void generateAbilityButtons()
        {
            if (buttons.Any())
            {
                return;
            }

            try
            {
                for (int i = 0; i < Adventurer.ABILITY_LIMIT; i++)
                {
                    AbilityButton button;
                    if (i < playerAbilities.Count)
                    {
                        button = AbilityButton.create(playerAbilities[i]);
                        button.icon.sprite = playerAbilities[i].assetData.sprite;
                        button.transform.SetParent(this.transform);
                    }
                    else
                    {
                        button = AbilityButton.create(null);
                        button.transform.SetParent(this.transform);
                    }
                    buttons.Add(button);
                }
            }
            catch (NotImplementedException e)
            {
                log.Error(e.Message);
            }
        }

        /// <summary>
        /// Marks an ability as selected/clicked on.
        /// </summary>
        /// <param name="selected">The ability slot that was selected</param>
        public static void setSelected(int selected)
        {
            if (lastSelected == -1)
            {
                lastSelected = selected;
            }
            else if (lastSelected != selected)
            {
                buttons[lastSelected].setSelected(false);
                lastSelected = selected;
            }

            buttons[selected].setSelected(true);
        }
    }
}
