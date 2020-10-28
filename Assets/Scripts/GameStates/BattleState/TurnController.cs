﻿
using System.Collections.Generic;
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
    /// <summary>
    /// Responsible object for cycling through player and monster turns during combat/.
    /// </summary>
    public interface TurnController
    {
        /// <summary>
        /// <remark>
        ///     ########### PARTY-LEADER ONLY ###########
        /// </remark>
        /// <para> 
        ///     Creates the initial combat order for the players and monsters.
        /// </para>
        /// <para>
        ///     The combat order is determined through randomising the order of both  Game.players and the monster party generated in  initMonsterParty().
        /// </para>
        /// </summary>
        public void initTurnOrder();

        /// <summary>
        /// <remark>
        ///     ########### PARTY-MEMBER ONLY ###########
        /// </remark>
        /// <para></para>
        ///     Called for non-party leader clients to initialize their turn order which was generated by party leader.
        /// </summary>
        /// <param name="turnOrder"> The list combatants IDs where the element order is the turn order. </param>
        public void initTurnOrder(List<int> turnOrder);


        /// <summary>
        /// <para> 
        ///     takeTurn() is responsible for determining the next turn in combat.
        /// </para>
        /// <para>
        ///     This function checks for client side player's turn and toggles the active/inactive state of the player ability bar.
        /// </para>       
        /// <para>
        ///     The combatant with an active turn has their green turn chevron indicator enabled.
        /// </para>
        /// </summary>
        public void takeTurn();

        /// <summary>
        ///   Gets the combatant whos turn is active
        /// </summary>
        /// <returns> The combatant whos turn it currently is. </returns>
        public Combatant getCurrentCombatant();
        
        /// <summary>
        ///  Gets the turn counter, which is incremented every time the active turn advances
        /// </summary>
        /// <returns> The amount of turns ellapsed since combat started </returns>
        public int getTurnCount();

        /// <summary>
        /// <list type="bullet">
        /// <item>
        /// <term>True</term>
        /// <description>The current turn is a player combatant</description>
        /// </item>
        /// <item>
        /// <term>False</term>
        /// <description>The current turn is a monster combatant</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <returns> 
        /// </returns>
        public bool isPlayerTurn();

    }
}
