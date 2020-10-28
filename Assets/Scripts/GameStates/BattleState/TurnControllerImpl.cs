using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.RPA_Messages;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameStates.BattleState
{
    public class TurnControllerImpl : TurnController
    {
        //Combatants
        public Ability lastAbilityUsed { get; set;}

        public List<Combatant> monsterParty;
        public List<Combatant> playerParty;
        
        // Turn related stuff
        public Combatant currentCombatant { get; private set;}
        public bool hasNextTurn { get; set; }
        public bool isClientPlayerTurn { get; private set; }
       
        
        public int turnCount;
        public bool hasValidTarget { get; set; }

        private bool hasNextCombatant = true;
        private bool _hasNextTurn;
        private int currentMonster;
        private int currentPlayer;
        

        public TurnControllerImpl(List<Combatant> monsterParty, List<Combatant> playerParty)
        {
            this.monsterParty = monsterParty;
            this.playerParty = playerParty;

            turnCount = 0;
            currentMonster = 0;
            currentPlayer = 0;
            hasValidTarget = false;
        }

        /// <inheritdoc/>
        public Combatant getCurrentCombatant()
        {
            return currentCombatant;
        }

        /// <inheritdoc/>
        public int getTurnCount()
        {
            return turnCount;
        }

        /// <inheritdoc/>
        public bool isPlayerTurn()
        {
            return turnCount % 2 == 0;
        }

        /// <inheritdoc/>
        public void initTurnOrder()
        {
            //Set Turn order for players
            System.Random rand = new System.Random();
            Game.players = Game.players.OrderBy(player => rand.Next()).ToList();
        }

        /// <inheritdoc/>
        public void initTurnOrder(List<int> turnOrder)
        { 
            Game.players = Game.players.OrderBy(player => turnOrder.IndexOf(player.id)).ToList();
        }

        /// <inheritdoc/>
        public void takeTurn()
        {
            turnCount++;
            int nextCombatant = getCombatant(isPlayerTurn());
            if(hasNextCombatant = nextCombatant > -1)
            {
                hasNextTurn = true;
                currentCombatant = isPlayerTurn() ? currentCombatant = playerParty[nextCombatant] 
                                                : currentCombatant = monsterParty[nextCombatant];

                currentCombatant.updateAbilityCooldowns();
                StateManager.battleState.applyBeforeEffects(currentCombatant);
                //Check for turn imparing status effects
                if (currentCombatant.isImpaired)
                {
                    takeTurn();
                }

                if (isPlayerTurn())
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
                MetaType metaType = AbilityFactory.getMetaType(ability.typeIds[0]);

                foreach(var target in targets)
                {
                    if (metaType == MetaType.DAMAGE)
                    {
                        StateManager.battleState.attackTarget(target, currentMonster, ability.abilityStrength.min, ability.abilityStrength.max);

                    }
                    else if (metaType == MetaType.EFFECT)
                    {
                        StateManager.battleState.affectTarget(target, ability.statusEffect, ability.conditionStrength.potency, ability.conditionStrength.turnsApplied);
                    }
                    else if(metaType == MetaType.HEALING)
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

 

    }
}
