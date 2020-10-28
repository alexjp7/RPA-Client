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
    public class BattleEngine
    {
        public TurnController turnController { get; private set; }

        public Adventurer clientAdventurer { get => Game.clientSidePlayer.playerClass; }
        public Combatant currentCombatant { get; private set; }
        public List<Combatant> targets { get; private set; }

        public List<Monster> monsterParty;
        public List<Adventurer> playerParty => Game.players.Select(player => player.playerClass).ToList();
        public Ability lastAbilityUsed { get; set; }

        public int monsterPartySize { get; set; }


        /// <summary>
        ///  Makes the call to start the first turn of combat
        /// </summary>
        public void startCombat()
        {
            turnController = new TurnControllerImpl(monsterParty, playerParty);
            initMonsterParty();
            turnController.takeTurn();
        }

        /// <summary>
        /// <remark>
        ///   ###########  PARTY-LEADER ONLY ###########
        /// </remark>
        /// <para> 
        ///    Generates the monster party via the monster factory class.
        /// </para>
        /// <para>
        ///     Addtionally, this function sets the UI components relevant to the monsters loaded from the factory.
        /// </para>
        /// </summary>
        /// <param name="partySize"> The amount of monsters generated on the battlefield. </param>
        private void initMonsterParty()
        {
            MonsterFactory mFactory = new MonsterFactory();
            monsterParty = mFactory.createMonsterParty(monsterPartySize);
        }

        /// <summary>
        /// <remark>
        ///     ########### PARTY-MEMBER ###########
        /// </remark>
        /// <para> 
        ///   Generates the monster party via the monster factory class.
        /// </para>
        /// <para>
        ///    Addtionally, this function sets the UI components relevant to the monsters loaded from the factory.
        /// </para>
        /// <example> E.g:
        ///     <code> 
        ///         "GoblinFighter":"Crazy Goblin Fighter" 
        ///     </code>
        /// </example>
        /// </summary>
        /// <param name="monsters"> A key-value mapping of monster type to monster name. </param>
        public void initMonsterParty(List<KeyValuePair<string, string>> mosnters)
        {
            MonsterFactory mFactory = new MonsterFactory();
            monsterParty = mFactory.createMonsterParty(mosnters);
        }


        /// <summary>
        ///  Resets targeting colors to default, and clears any exsiting targets from the previous turn.
        /// </summary>
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
