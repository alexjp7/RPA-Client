using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.UI;
using UnityEngine;
using log4net;
using Assets.Scripts.Common;
using Assets.Scripts.Entities.Containers;

namespace Assets.Scripts.GameStates
{
    public class TurnController
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(TurnController));

        /// <summary>
        /// Tracks the turn count of a combat encounter. This value is incremented every time a new turn begins.
        /// </summary>
        public int turnCount { get; private set; }

        /// <summary>
        /// Alias for <see cref="Game.clientSidePlayer.playerClass"/>
        /// </summary>
        public Adventurer clientAdventurer { get => Game.clientSidePlayer.playerClass; }

        /// <summary>
        /// The combatant whos turn is active.
        /// </summary>
        public Combatant currentCombatant { get; private set; }

        /// <summary>
        /// The Combatant whos turn will proceed the <see cref="currentCombatant"/>
        /// </summary>
        public Combatant nextCombatant { get; private set; }

        /// <summary>
        /// The list of targets that have been considered by the <see cref="currentCombatant"/>
        /// </summary>
        public List<Combatant> targets { get; private set; }

        /// <summary>
        /// List of <see cref="Monster">Monsters</see> that are in a current combat encounter.
        /// </summary>
        public CombatParty monsterParty;

        /// <summary>
        /// List of <see cref="Adventurer">Adventurers</see> that are connected to the game.
        /// </summary>
        /// <remarks>Alias for <see cref="Adventurer.playerClass"/></remarks>
        public CombatParty playerParty;

        /// <summary>
        /// The ability that was used by the <see cref="currentCombatant"/> on their turn.
        /// </summary>
        public Ability lastAbilityUsed { get; set; }

        /// <summary>
        /// The monster/player's name whos turn it is.
        /// </summary>
        /// <remarks>Alias for  <see cref="currentCombatant.name"/></remarks>
        public string currentTurnNameDisplay { get => currentCombatant.name; }

        //Flags
        /// <summary>
        /// Flag for determining whether a combant encounter has begun.
        /// </summary>
        /// <remarks>
        /// This is used to stop repeated execution of end combat logic in the battle states <c>Update()</c>
        /// </remarks>
        public bool hasCombat { get; set; }

        /// <summary>
        /// Flag for determining whether the next turn of a combat encounter can be taken.
        /// </summary>
        /// <remarks>
        /// This is used to stop repeated execution of UI updates in battle state in <c>Update()</c>
        /// </remarks>
        public bool hasNextTurn { get; set; }

        /// <summary>
        /// Flag for determining whether combat is over.
        /// </summary>
        /// <remarks>
        /// This used during BattleState <c>Update()</c> to begin processing of end-of-combat logic.
        /// </remarks>
        public bool hasCombatEnded;


        /// <summary>
        /// Flag for determining whether the <see cref="currentCombatant"/> is the client side player.
        /// </summary>
        public bool isClientPlayerTurn { get; private set; }

        /// <summary>
        /// Flag for determining whether the current turn is a monster or player turn. 
        /// </summary>
        public bool isPlayerTurn { get => turnCount % 2 == 1; }

        /// <summary>
        /// Validation flag to determining of a valid target has been acquired based on the <see cref="lastAbilityUsed"/> targeting type.
        /// </summary>
        public bool hasValidTarget { get; set; }

        /// <summary>
        /// Flags whether the player or monster team has won in combat;
        /// </summary>
        public bool hasPlayerTeamWon { get; private set; }

        public void resetCombat()
        {
            if(turnCount > 0)
            {
                playerParty.reset(Game.players.Select(player => player.playerClass as Combatant).ToList());
            }

            turnCount = 0;
            targets = new List<Combatant>();
            hasValidTarget = false;
            hasCombat = true;
        }

        /// <summary>
        /// <para>
        /// Creates the initial combat order for the players and monsters 
        /// </para>
        /// The combat order is determined through randomising the order  of both  Game.players and the monster party generated in initMonsterParty()
        /// </summary>
        /// <remarks>
        ///  Should only be called by the game's <b>party-leader</b>
        /// </remarks>
        public void generateTurnOrder()
        {
            if (Game.clientSidePlayer.isPartyLeader)
            {
                System.Random rand = new System.Random();
                Game.players = Game.players.OrderBy(player => rand.Next()).ToList();
                playerParty = new CombatParty(Game.players.Select(player => player.playerClass as Combatant).ToList());


            }
            else
            {
                InvalidPartyRoleException exception = new InvalidPartyRoleException("Combat initialization violdation found. Client player must be the party leader.");
                log.Error(exception.Message, exception);
                throw exception;
            }
        }

        /// <summary>
        /// Called for non-party leader clients to initialize their turn order which was generated by party leader.
        /// </summary>
        /// <remarks>
        /// Should only be called by <b>party-member</b>
        /// </remarks>
        /// <param name="turnOrder">List of player IDs that has been passed in from server.</param>
        public void generateTurnOrder(List<int> turnOrder)
        {
            if (!Game.clientSidePlayer.isPartyLeader)
            {
                Game.players = Game.players.OrderBy(player => turnOrder.IndexOf(player.id)).ToList();
            }
            else
            {
                InvalidPartyRoleException exception = new InvalidPartyRoleException("Combat initialization violdation found. Client player cannot be party leader.");
                log.Error(exception.Message, exception);
                throw exception;
            }
        }

        /// <summary>
        /// Generates the monster party via monster factory class.  <see cref="Assets.Scripts.Entities.Monsters.MonsterFactory">MonsterFactory</see>
        /// </summary>
        /// <remarks> 
        /// Should only be called by <b>party-leader</b> 
        /// </remarks>
        /// <param name="partySize">The amount of monsters to spawn in an encounter</param>
        public void initMonsterParty(int partySize)
        {
            if (Game.clientSidePlayer.isPartyLeader)
            {
                MonsterFactory mFactory = new MonsterFactory();
                monsterParty = new CombatParty(mFactory.createMonsterParty(partySize).ToList<Combatant>());
            }
            else
            {
                InvalidPartyRoleException exception = new InvalidPartyRoleException("Combat initialization violdation found. Client player must be the party leader.");
                log.Error(exception.Message, exception);
                throw exception;
            }
        }

        /// <summary>
        /// Generates the monster party via server response message of pre-populated monsters
        /// </summary>
        /// <remarks>
        /// Should only be called by <b>party-member</b>
        /// </remarks>
        /// <param name="monsters">The list of monsters pre-populated by party-leader's client</param>
        public void initMonsterParty(List<KeyValuePair<string, string>> monsters)
        {
            if (!Game.clientSidePlayer.isPartyLeader)
            {
                MonsterFactory mFactory = new MonsterFactory();
                monsterParty = new CombatParty(mFactory.createMonsterParty(monsters).ToList<Combatant>());
            }
            else
            {
                InvalidPartyRoleException exception = new InvalidPartyRoleException("Combat initialization violdation found. Client player must be the party leader.");
                log.Error(exception.Message, exception);
                throw exception;
            }
        }

        /// <summary>
        /// Makes the call to start the first turn of combat 
        /// </summary>
        public void startCombat()
        {
            hasCombatEnded = false;
            logBattleStartup();
            takeTurn();
        }

        /// <summary>
        /// Logs the initialized player and monster partys
        /// </summary>
        private void logBattleStartup()
        {
            log.Debug("Players:");
            foreach (Adventurer player in playerParty.asList())
            {
                log.Debug(player.ToString());
            }

            log.Debug("Monsters:");
            foreach (Monster monster in monsterParty.asList())
            {
                log.Debug(monster.ToString());
            }

            log.Debug("Combat begin!");
        }


        /*---------------------------------------------------------------
                                COMBAT-LOGIC
         ---------------------------------------------------------------*/
        /// <summary>
        /// Responsible for determining the next turn in combat. This function checks for client side player's turn and toggles  the active/inactive state of the player ability bar.
        /// </summary>
        /// <remarks>
        /// The combatant with an active turn has their green turn chevron indicator enabled.
        /// </remarks>
        public void takeTurn()
        {
            if (playerParty.checkDefeatedAndRemoveDead() || monsterParty.checkDefeatedAndRemoveDead())
            {
                hasPlayerTeamWon = currentCombatant.type == CombatantType.PLAYER;
                hasCombatEnded = true;
            }
            else
            {
                turnCount++;
                currentCombatant = isPlayerTurn ? playerParty.cycleNext() : monsterParty.cycleNext();
                nextCombatant = !isPlayerTurn ? playerParty.peekNext() : monsterParty.peekNext();

                if (currentCombatant != null)
                {
                    hasNextTurn = true;
                    log.Debug($"Current Turn: {currentCombatant.id}:\"{currentCombatant.name}\"");

                    currentCombatant.updateAbilityCooldowns();
                    StateManager.battleState.applyBeforeEffects(currentCombatant);

                    //Check for turn imparing status effects
                    if (currentCombatant.isImpaired)
                    {
                        log.Debug($" ID:{currentCombatant.id} name:{currentCombatant.name} is impared. Turn Skipped!");
                        return;
                    }

                    if (isPlayerTurn)
                    {
                        isClientPlayerTurn = currentCombatant.id == Game.clientSidePlayer.id;
                    }
                    else
                    {
                        isClientPlayerTurn = false;
                        takeMonsterTurn(currentCombatant as Monster);
                        StateManager.battleState.updateConditionUI();
                        takeTurn();
                    }

                }
            }
        }


        /// <summary>
        /// Called when a monster has its turn. This function will get the monster's selected target, choose an ability to use then apply it to a target.
        /// </summary>
        /// <param name="monster">The monster who's turn it is.</param>
        private void takeMonsterTurn(Monster monster)
        {
            targets.Clear();
            Monster currentMonster = currentCombatant as Monster;
            Ability ability = null;
            targets = monster.getTargets(out ability, monsterParty.asList(), playerParty.asList());

            try
            {
                MetaType metaType = AbilityFactory.getMetaType(ability.typeIds[0]);

                foreach (var target in targets)
                {
                    if (metaType == MetaType.DAMAGE)
                    {
                        StateManager.battleState.attackTarget(target, currentMonster, ability.abilityStrength.min, ability.abilityStrength.max);

                    }
                    else if (metaType == MetaType.EFFECT)
                    {
                        StateManager.battleState.affectTarget(target, ability.statusEffect, ability.conditionStrength.potency, ability.conditionStrength.turnsApplied);
                    }
                    else if (metaType == MetaType.HEALING)
                    {
                        StateManager.battleState.healTarget(target, ability.abilityStrength.min, ability.abilityStrength.max);
                    }
                }

                FloatingPopup.create(monster.combatSprite.transform.position, ability.name, Color.gray);
            }
            catch (NullReferenceException ex)
            {
                Debug.LogError($"Error during {currentMonster.name}'s turn - Likely cause is due to missing ability instance");
            }

            ability.setLastTurnUsed(turnCount);
            currentMonster.updateAbilityCooldowns();
        }

        /// <summary>
        ///Resets targeting colors to default, and clears any exsiting targets from the previous turn. 
        /// </summary>
        public void resetTargets()
        {
            hasValidTarget = false;
            AbilityButton.selectedAbilityIndex = -1;
            targets.Clear();
            playerParty.asList()
            .FindAll(player => player.isAlive())
            .ForEach(player =>
            {
                player.combatSprite.sprite.color = Color.white;
            });

            monsterParty.asList()
            .FindAll(monster => monster.isAlive())
            .ForEach(monster =>
            {
                monster.combatSprite.sprite.color = Color.white;
            });
        }

    }
}
