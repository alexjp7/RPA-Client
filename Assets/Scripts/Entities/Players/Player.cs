/*---------------------------------------------------------------
                        PLAYER
 ---------------------------------------------------------------*/
/***************************************************************
* The playerr class defines and implements functions and fields
  relating to client-side/remote player's name, server-side
  id, and a multi-use 'ready' boolean, for synronizing
  player-party events.

* Player objects represent the top level object in the dynamic
  player instance hierachy:
    A Player has an: 
        - Adventurer instance, which is a sub-class
          of Combatant.

        - Combatables are the top-level object which both
          monsters and Adventurer types inherit from.

        - Comtabales have a Damageable and Renderable
          components which include health point managers
          and asset allocation/assignment, while also providing
          a top-level implementation for combat handlers.

        - An Adventurer has sub-classes which inherit
          it; Warrior, Wizard, Rogue and Cleric, these represnet
          the playable archetypes which are selectable within
          character creation and include distinct abilities and
          HP pools.
          
        - This structure allows for generic event handling
          and shared logic that can be executed throughout 
          the appropraite layers of the Player object hierachy.
**************************************************************/

using Assets.Scripts.RPA_Game;
using System;
using UnityEngine;

namespace Assets.Scripts.Entities.Players
{
    public class Player
    {
        public int id { get; set; }
        public string name { get; set; }
        public bool ready { get; set; }
        public bool isPartyLeader { get => id == Game.partyLeaderId; }
        public bool isClientPlayer { set; get; } //flag for marking the client side player

        public Adventurer playerClass { get; set; }

        //Check if all players is ready
        public static bool hasAllReady {get =>  checkAllReady();}

        private static bool checkAllReady()
        {
            int result = 0;
            foreach(var player in Game.players)
            {
                if(player.ready)
                {
                    result++;
                }
            }

            return result == Game.connectedPlayers;
        }


        /***************************************************************
        * Used to indicate class in character creation screen, allowing 
          for the instantiation for the Adventurer Object to be
          done after Character Creation is done.
        **************************************************************/
        public int adventuringClass;

        /***************************************************************
        * Default construction of 4 Player objects is done after
          starting or joining a Game from the Main Menu state.
          When new players join, or when the existing player data
          is sent to this client, the other player's fields are publicly
          set.

        * This is done to bulk instantiate player objects while there
          is limited loading occuring in the current game state.

        * After the Character Creation state progresses into the BattleState
          the Player objects no in use are deconstructed.  
        **************************************************************/
        public Player()
        {
            this.name = "";
            this.adventuringClass = -1;
            this.id = -1;
            this.ready = false;
        }

        /***************************************************************
        * Allows for simulated consturction of Player objects
          for running client-side tests.

        @param - id: unique player ID (would otherwise be provided by server).
        @param - name: dispaly name given to a player(would otherwise 
        be provided by player).
        @param -adventuringClass: int between 0-3 representing a class 
        choice,  Warrior(0), Wizard(1), Rogue(2), Cleric(3). used
        in applyClass() member function,
        @paran - ready: flag used to indicate whether the player
        is ready or in favour of a party wide action.
        **************************************************************/
        public Player(int id, string name, int adventuringClass, bool ready = true)
        {
            this.id = id;
            this.name = name;
            this.adventuringClass = adventuringClass;
            this.ready = ready;
        }

        public Player(string name, PlayerClasses adventuringClass, bool ready = true)
        {
            this.id = Game.connectedPlayers;
            this.name = name;
            this.adventuringClass = (int)adventuringClass;
            this.ready = ready;
        }

        /***************************************************************
        * Instantiates the Adventuring class object based on the selected 
          class during character creation.

        * This function seperates the bulk of the Adventurer
          instantiation until after the character creation state is over
          to defer uncessary processing when changing classes during
          character creaiotn.
        **************************************************************/
        public void applyClass()
        {
            switch ((PlayerClasses)adventuringClass)
            {
                case PlayerClasses.WARRIOR:
                    playerClass = new Warrior(name, id);
                    break;

                case PlayerClasses.WIZARD:
                    playerClass = new Wizard(name, id);
                    break;

                case PlayerClasses.ROGUE:
                    playerClass = new Rogue(name, id);
                    break;

                case PlayerClasses.CLERIC:
                    playerClass = new Cleric(name, id);
                    break;
                default:
                    throw new NotImplementedException("ERROR - Either; Adventuring Class is not implemented or an unexpected class ID was encountered during player.applyClass()");
            }
        }


        public string toString()
        {
            return "id: " + this.id + " name: " + this.name + " class: " + this.adventuringClass + " ready:" + ready;
        }
    }
}
