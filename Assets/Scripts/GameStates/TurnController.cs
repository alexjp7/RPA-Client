using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameStates
{
    public class TurnController
    {

        private static TurnController _instance;
        public static TurnController INSTANCE
        {
            get =>_instance == null ? _instance = new TurnController() : _instance;
        }
        public static int turnCount { get; private set; }
        public Player clientPlayer { get; private set; }
        public Adventurer clientAdventurer { get =>  clientPlayer.playerClass; }
        //Combatants
        public Combatable currentCombatant { get; private set;}
        public List<Combatable> targets { get; private set; }
        public List<Monster> monsterParty { get; set; }
        public List<Adventurer> playerParty => Game.players.Select(player => player.playerClass).ToList();


        public string currentTurnNameDisplay { get => currentCombatant.name; }
        //Flags
        public bool hasNextTurn { get; set; }
        public bool isClientPlayerTurn { get; private set; }
        public bool isPlayerTurn { get => turnCount % 2 == 1; }
        public bool hasValidTarget { get; set; }
        private bool hasNextCombatant = true;


        private bool _hasNextTurn;
        private int currentMonster = 0;
        private int currentPlayer = 0;
        private System.Random rand;

        private TurnController()
        {
            rand = new System.Random();
            targets = new List<Combatable>();
            turnCount = 0;
            hasValidTarget = false;
        }

        public void init()
        {
            //Set Turn order for players
            Game.players = Game.players.OrderBy(player => rand.Next()).ToList();
            clientPlayer = Game.players.Find(player => player.isClientPlayer);
            initMonsterParty();
        }

        /***************************************************************f
        * Generates the monster party via the monster factory class.

        * Addtionally, this function sets the UI components relevant to
          the monsters loaded from the factory.
        **************************************************************/
        private void initMonsterParty()
        {
            int monsterPartySize = 3;
            MonsterFactory mFactory = new MonsterFactory();
            monsterParty = mFactory.createMonsterParty(monsterPartySize);
        }

        /***************************************************************
        * Creates the initial combat order for the players and monsters.

        * The combat order is determined through randomising the order 
          of both  Game.players and the monster party generated in 
          initMonsterParty().

        * The targets dictionary is initialised in this function
          which is used to map the index of a sprite along with the
          data relating to its health, abilities and active conditions.
        **************************************************************/
        public void startCombat()
        {
            takeTurn();
        }


        /*---------------------------------------------------------------
                                COMBAT-LOGIC
         ---------------------------------------------------------------
        * takeTurn() is responsible for determining the next turn in combat. 

        * This function checks for client side player's turn and toggles 
          the active/inactive state of the player ability bar.

        * The combatant with an active turn has their green turn
          chevron indicator enabled.
        **************************************************************/
        public void takeTurn()
        {
            turnCount++;
            int nextCombatant = getCombatant(isPlayerTurn);
            if(hasNextCombatant = nextCombatant > -1)
            {
                if (isPlayerTurn)
                {
                    currentCombatant = playerParty[nextCombatant];
                    isClientPlayerTurn = Game.players[nextCombatant].id == clientPlayer.id;
                }
                else
                {
                    currentCombatant = monsterParty[nextCombatant]; 
                    isClientPlayerTurn = false;
                    takeMonsterTurn(nextCombatant);
                }

                hasNextTurn = true;
            }
        }

        /***************************************************************
        @param - isPlayerTurn : flags whether to get the next
        turn from the player(true) or monster-party(false).

        @return - The index of the next combatant
        **************************************************************/
        private int getCombatant(bool isPlayerTurn)
        {
            int nextComtatant = 0;
            int deathCounter = 0;

            if (isPlayerTurn)
            {
                nextComtatant = currentPlayer % Game.players.Count;

                while (!Game.players[nextComtatant].playerClass.isAlive()) //Skip over dead players
                {
                    nextComtatant = ++currentPlayer % Game.players.Count;

                    if (++deathCounter == Game.players.Count)
                    {
                        nextComtatant = -1;
                        break;
                    }
                }
                currentPlayer++;
            }
            else
            {
                nextComtatant = currentMonster % monsterParty.Count;

                while (!monsterParty[nextComtatant].isAlive()) //Skip over dead monsters
                {
                    nextComtatant = ++currentMonster % monsterParty.Count;

                    if (++deathCounter == monsterParty.Count)
                    {
                        nextComtatant = -1;
                        break;
                    }
                }
                currentMonster++;
            }

            return nextComtatant;
        }

        /***************************************************************
            * Called when a monster has its turn.

            * The logic relating to a monster's turn priority is refernced
              throgh the member functions of that monster.

            @param - nextCombatant: the combat properties relating to the
            monster whos turn it currently is.
            **************************************************************/
        private void takeMonsterTurn(int monsterIndex)
        {
            targets.Clear();
            Monster currentMonster = currentCombatant as Monster;
            Ability ability = null;
            targets = monsterParty[monsterIndex].getTargets(out ability);

            try
            {
                MetaTypes metaType = AbilityFactory.getMetaType(ability.typeIds[0]);

                foreach(var target in targets)
                {
                    if (metaType == MetaTypes.DAMAGE)
                    {
                        ViewController.battleState.attackTarget(target, currentMonster, ability.abilityStrength.min, ability.abilityStrength.max);

                    }
                    else if (metaType == MetaTypes.EFFECT)
                    {
                        ViewController.battleState.affectTarget(target, ability.statusEffect, ability.abilityStrength.max, ability.abilityStrength.min);
                    }
                    else if(metaType == MetaTypes.HEALING)
                    {
                        ViewController.battleState.healTarget(target, ability.abilityStrength.min , ability.abilityStrength.max);
                    }
                }

                FloatingPopup.create(monsterParty[monsterIndex].combatSprite.transform.position, ability.name, Color.gray);
            }
            catch(NullReferenceException ex)
            {
                Debug.LogError($"Error during {currentMonster.name}'s turn - Likely cause is due to missing ability instance");
            }

            currentMonster.updateAbilityCooldowns();
            takeTurn(); //Progress to next combatant's turn
        }

        /***************************************************************
        * Resets targeting colors to default, and clears any exsiting
          targets from the previous turn.
        **************************************************************/
        public void resetTargets()
        {
            hasValidTarget = false;
            AbilityButton.selectedAbilityIndex = -1;
            targets.Clear();
            playerParty
            .FindAll(player => player.isAlive())
            .ForEach(player => {
                player.combatSprite.sprite.color = Color.white;
            });

            monsterParty
            .FindAll(monster => monster.isAlive())
            .ForEach(monster => {
                monster.combatSprite.sprite.color = Color.white;
            });
        }

    }
}
