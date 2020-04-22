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

        public int turnCount { get; private set; }
        public Adventurer clientAdventurer { get =>  Game.clientSidePlayer.playerClass; }
        //Combatants
        public Combatable currentCombatant { get; private set;}
        public List<Combatable> targets { get; private set; }
        public List<Monster> monsterParty;
        public List<Adventurer> playerParty => Game.players.Select(player => player.playerClass).ToList();

        public string currentTurnNameDisplay { get => currentCombatant.name; }
        //Flags
        public bool hasNextTurn { get; set; }
        public bool isClientPlayerTurn { get; private set; }
        public bool isPlayerTurn { get => turnCount % 2 == 1; }
        public bool hasValidTarget { get; set; }
        private bool hasNextCombatant = true;


        private bool _hasNextTurn;
        private int currentMonster;
        private int currentPlayer;

        public TurnController()
        {
            turnCount = 0;
            currentMonster = 0;
            currentPlayer = 0;

            targets = new List<Combatable>();
            hasValidTarget = false;
        }

        /***************************************************************
        * Creates the initial combat order for the players and monsters.

        * The combat order is determined through randomising the order 
          of both  Game.players and the monster party generated in 
          initMonsterParty().
        **************************************************************/
        public void initTurnOrder()
        {
            //Set Turn order for players
            System.Random rand = new System.Random();
            Game.players = Game.players.OrderBy(player => rand.Next()).ToList();
        }

        /***************************************************************f
        * Generates the monster party via the monster factory class.

        * Addtionally, this function sets the UI components relevant to
          the monsters loaded from the factory.
        **************************************************************/
        public void initMonsterParty(int partySize)
        {
            int monsterPartySize = 4;
            MonsterFactory mFactory = new MonsterFactory();
            monsterParty = mFactory.createMonsterParty(monsterPartySize);
        }

        /***************************************************************f
        * Makes the call to start the first turn of combat
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
                hasNextTurn = true;
                currentCombatant = isPlayerTurn ? currentCombatant = playerParty[nextCombatant] 
                                                : currentCombatant = monsterParty[nextCombatant];

                currentCombatant.updateAbilityCooldowns();
                StateManager.battleState.applyBeforeEffects(currentCombatant);
                //Check for turn imparing status effects
                if (currentCombatant.isImpaired)
                {
                    takeTurn();
                }

                if (isPlayerTurn)
                {
                    isClientPlayerTurn = Game.players[nextCombatant].id == Game.clientSidePlayer.id;
                }
                else
                {
                    isClientPlayerTurn = false;
                    takeMonsterTurn(nextCombatant);
                    StateManager.battleState.updateConditionUI();
                    takeTurn();
                }

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
            targets = monsterParty[monsterIndex].getTargets(out ability, in monsterParty, playerParty);

            try
            {
                MetaTypes metaType = AbilityFactory.getMetaType(ability.typeIds[0]);

                foreach(var target in targets)
                {
                    if (metaType == MetaTypes.DAMAGE)
                    {
                        StateManager.battleState.attackTarget(target, currentMonster, ability.abilityStrength.min, ability.abilityStrength.max);

                    }
                    else if (metaType == MetaTypes.EFFECT)
                    {
                        StateManager.battleState.affectTarget(target, ability.statusEffect, ability.conditionStrength.potency, ability.conditionStrength.turnsApplied);
                    }
                    else if(metaType == MetaTypes.HEALING)
                    {
                        StateManager.battleState.healTarget(target, ability.abilityStrength.min , ability.abilityStrength.max);
                    }
                }

                FloatingPopup.create(monsterParty[monsterIndex].combatSprite.transform.position, ability.name, Color.gray);
            }
            catch(NullReferenceException ex)
            {
                Debug.LogError($"Error during {currentMonster.name}'s turn - Likely cause is due to missing ability instance");
            }

            currentMonster.updateAbilityCooldowns();
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
