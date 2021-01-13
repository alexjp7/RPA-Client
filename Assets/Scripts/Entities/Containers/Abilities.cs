namespace Assets.Scripts.Entities.Containers
{
    using log4net;
    using SimpleJSON;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using UnityEngine;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.Entities.Players;
    using Assets.Scripts.UI.Common;


    /// <summary>
    /// Container for accessing ability members and procedures
    /// </summary>
    public class Abilities
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(Abilities));

        public Ability this[int index]
        {
            get
            {
                return this.get(index);
            }
        }

        public static List<AbilityButton> buttons = new List<AbilityButton>();

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

        public int Count => abilities.Count;

        /// <summary>
        /// The list of abilities that a combatant has during combat.
        /// </summary>
        public List<Ability> abilities { get; private set; }

        /// <summary>
        /// The abilitiy that was last clicked on, used for toggling UI indicators.
        /// </summary>
        private static int lastSelected = -1;

        /// <summary>
        /// For startup loading of abilties
        /// </summary>
        /// <param name="path">The data path to load a combatants abilities from</param>
        /// <param name="type"> <c>MONSTER</c> or <c>PLAYER</c> <see cref="CombatantType"/></param>
        /// <param name="isInitialLoad">Determines whether to load a players starting abilities</param>
        public Abilities(string path, CombatantType type, bool isInitialLoad = false)
        {
            abilities = new List<Ability>();
            loadAbilityData(path, type, isInitialLoad);
        }

        /// <summary>
        /// Sets an ability container to a pre-loaded ability list.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This constructor is called to prevent repeated reading and parsing 
        /// of the ability JSON on disk.
        /// </para>
        /// Monsters will have a single static instance of <c>Abilities</c> and will 
        /// pass in a reference that will be copied for each monster of the same type.
        /// </remarks>
        /// <param name="preLoaded"> The pre-loaded abilities from disk.</param>
        public Abilities(Abilities preLoaded)
        {
            this.abilities = new List<Ability>(preLoaded.abilities);
        }

        /// <summary>
        /// Get an ability
        /// </summary>
        /// <param name="i">The ability index that represents the ability object to be returned.</param>
        /// <returns>An ability at the index of <c>i</c></returns>
        public Ability get(int i)
        {
            if (i >= abilities.Count())
            {
                ArgumentOutOfRangeException exception = new ArgumentOutOfRangeException($"Index [{i}] exceeds container [Abilities] size of [{abilities.Count}]");
                log.Error(exception.Message);
                throw exception;
            }
            return abilities[i];
        }

        /// <summary>
        /// Updates an ability slot to a new ability at location <c>slotIndex</c>.
        /// </summary>
        /// <param name="slotIndex">The ability slot that will be updated to a new ability</param>
        /// <param name="ability">The new ability</param>
        /// <returns>The old ability that was replaced.</returns>
        public Ability updateSlot(int slotIndex, Ability ability)
        {
            Ability replacedAbility = abilities[slotIndex];
            abilities[slotIndex] = ability;
            return replacedAbility;
        }

        /// <summary>
        /// Resets the cooldowns of all abilities
        /// </summary>
        public void resetCooldowns()
        {
            abilities.ForEach(ability => ability.resetCooldown());
        }

        /// <summary>
        /// Updates the cooldown of all abilities
        /// </summary>
        public void updateCooldowns()
        {
            abilities.ForEach(ability => ability.updateCooldown());
        }



        public List<Ability> getAll()
        {
            return this.abilities;
        }

        /// <summary>
        /// Loads ability data from disk, constuct the ability and adds to ability list.
        /// </summary>
        /// <param name="path">path on disk where ability data is.</param>
        /// <param name="isInitialLoad">whether the loading proces should grab starting abiltiies or all abilities</param>
        private void loadAbilityData(string path, CombatantType type, bool isInitialLoad)
        {
            string abilityText = File.ReadAllText(type == CombatantType.MONSTER ? path += ".json" : path);
            JSONNode json = JSON.Parse(abilityText);
            string entityName = json["entity"];

            if (type == CombatantType.PLAYER)
            {
                loadPlayerAbilities(json, isInitialLoad, entityName);

            }
            else if (type == CombatantType.MONSTER)
            {
                loadMonsterAbilities(json, entityName);
            }
            else
            {
                throw new ArgumentException("Could not resolve [CombatantType] during ability loading");
            }
        }

        /// <summary>
        /// Loads the client side player's abilities. 
        /// </summary>
        /// <param name="json"></param>
        /// <param name="isInitialLoad"></param>
        private void loadPlayerAbilities(JSONNode json, bool isInitialLoad, string entityName)
        {
            int skillLevel = 0;

            JSONArray abilityJson = isInitialLoad ? json["starting_abilities"].AsArray : json["abilities"].AsArray;
            AbilityBuilder builder = new AbilityBuilder();

            foreach (JSONNode jsonNode in abilityJson)
            {
                abilities.Add(constructPlayerAbility(jsonNode, skillLevel, entityName));
            }
        }

        /// <summary>
        /// Loads a monsters ability
        /// </summary>
        /// <param name="json"></param>
        private void loadMonsterAbilities(JSONNode json, string entityName)
        {
            JSONArray abilityJson = json["abilities"].AsArray;
            int abilityCount = abilityJson.Count;

            foreach (JSONNode jsonNode in abilityJson)
            {
                abilities.Add(constructMonsterAbility(jsonNode, entityName));
            }
        }

        /// <summary>
        /// Builds a player ability.
        /// </summary>
        /// <param name="jsonNode">The node of the json array which represents an ability</param>
        /// <param name="skillLevel">The skill level of an ability</param>
        /// <returns>The fully constructed ability</returns>
        private Ability constructPlayerAbility(JSONNode jsonNode, int skillLevel, string entityName)
        {
            return new AbilityBuilder().json(jsonNode, entityName)
                    .skillLevel(skillLevel)
                    .name()
                    .id()
                    .typeIds()
                    .cooldown()
                    .statusEffect()
                    .assetData()
                    .toolTip()
                    .abilityStrength()
                    .conditionStrength()
                    .build();
        }

        /// <summary>
        /// Builds a monster ability
        /// </summary>
        /// <param name="jsonNode">The node of the json array which represents an ability</param>
        /// <returns></returns>
        private Ability constructMonsterAbility(JSONNode jsonNode, string entityName)
        {
            return new AbilityBuilder().json(jsonNode, entityName)
                    .name()
                    .typeIds()
                    .cooldown()
                    .statusEffect()
                    .abilityStrength()
                    .assetData()
                    .conditionStrength()
                    .build();
        }

        /// <summary>
        /// Generates <see cref="AbilityButton"/> UI components for each of client side players abilities.
        /// </summary>
        public void generateAbilityButtons(in Transform abilityBar)
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
                    if (i < abilities.Count)
                    {
                        button = AbilityButton.create(abilities[i]);
                        button.icon.sprite = abilities[i].assetData.sprite;
                        button.transform.SetParent(abilityBar);
                    }
                    else
                    {
                        button = AbilityButton.create(null);
                        button.transform.SetParent(abilityBar);
                    }
                    buttons.Add(button);
                }
            }
            catch (NotImplementedException e)
            {
                log.Error(e.Message);
            }

        }
    }
}
