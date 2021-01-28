

/*---------------------------------------------------------------
                            BATTLE-STATE
 ---------------------------------------------------------------*/
/***************************************************************
* The Battle state includes the UI handlers and event callbacks
  relating to the combat encounters for RPA.

  The initialisation of this state includes the generation of
  the monster party, turn order player UI and turn-base logic,
  while defering combat changes to the related monster and 
  party instances. 
**************************************************************/

#pragma warning disable 1234
namespace Assets.Scripts.GameStates
{
    #region IMPORTS
    using System;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Linq;
    using log4net;
    using System.Threading.Tasks;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Players;
    using Assets.Scripts.Entities.Monsters;
    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.UI.Common;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.RPA_Messages;
    using Assets.Scripts.Entities.Containers;
    using Assets.Scripts.Combat;
    using Assets.Scripts.UI.Combat;
    #endregion IMPORTS

    public class BattleState : MonoBehaviour
    {
        [SerializeField] private GameObject player_horizontalLayout;
        [SerializeField] private GameObject monster_horizontalLayout;
        [SerializeField] private Text currentTurnDisplayName;
        [SerializeField] private GameObject notificationPanel;
        [SerializeField] private GameObject combatPanelLayout;
        [SerializeField] private GameObject combatPanelButtons;

        private CombatPanel combatPanel;

        private static readonly ILog log = LogManager.GetLogger(typeof(BattleState));

        //Client Player Alias
        private Player clientPlayer { get => Game.clientSidePlayer; }
        //Combat Controllers
        public CombatController combatController { get; private set; }

        void Awake()
        {
            initEnvironment(); // Updates static references
            initControllers(); // Constructs turn controller - party leader sends to server 
            startCombat();
        }

        private async void startCombat()
        {
            await initCombat(); // Sends/Requests combat data to other clients     
            combatController.startCombat();
        }

        /// <summary>
        /// Sets this script as the current active state script, allowing for static acces to callback/ui handlers and TurnController.
        /// </summary>
        private void initEnvironment()
        {
            if (TestSimulator.isDeveloping) //Flaggged by REQUIRE_TEST_DATA directive in TestSimulaotr
            {
                TestSimulator.initTestEnvironment(GameState.BATTLE_STATE);
            }

            //If using Test data, ensure test environment is executed before this
            StateManager.setStateScript();
        }


        /// <summary>
        /// Initialises controller responsible for delegating and enforcing the turn based logic for combat rounds.
        /// </summary>
        private void initControllers()
        {
            combatController = new CombatController();
        }

        /// <summary>
        /// Initializes combat order, monster party and handles any server/client communication
        /// </summary>
        /// <returns>The initialisation task which awaits the return of any server communication.</returns>
        public async Task initCombat()
        {
            if (Game.isSinglePlayer)
            {
                combatController.resetCombat();
                combatController.generateTurnOrder();
                combatController.initMonsterParty(4);
                initBattleField(); // UI initialisation
            }
            else //Handle Multiplayer session communication
            {
                if (clientPlayer.isPartyLeader)
                {
                    combatController.generateTurnOrder();
                    combatController.initMonsterParty(4);
                    await Task.Run(() => Message.send(new BattleMessage(BattleInstruction.COMBAT_INIT)));
                    initBattleField();
                }
                else
                {
                    await requestBattleData();
                    initBattleField();
                }
            }
        }

        /// <summary>
        /// Initializes turnorder and monster party based on server response
        /// </summary>
        /// <returns>A task containing the battle state and data</returns>
        private async Task requestBattleData()
        {
            //Wait for party leader to send combat oder/monster party data
            BattleMessage message = await Task.Run(() => awaitCombatInit());

            combatController.generateTurnOrder(message.turnOrder);
            combatController.initMonsterParty(message.monsters);
        }

        /// <summary>
        /// Waits for server to reply with a battle message that will be used to initialzie combat.
        /// </summary>
        /// <returns>The battlemessage containing the list of monsters and their turn order.</returns>
        private BattleMessage awaitCombatInit()
        {
            bool hasData = false;
            BattleMessage battleMessage = null;
            string serverMessage;

            while (!hasData)
            {
                if (Game.gameClient.ready())
                {
                    serverMessage = Game.gameClient.read();

                    if (serverMessage != "a")
                    {
                        battleMessage = new BattleMessage(serverMessage);
                        hasData = true;
                    }
                }
            }

            return battleMessage;
        }

        /*---------------------------------------------------------------
                            UI-INIATIALISATIONS
        ---------------------------------------------------------------*/
        /// <summary>
        /// Initializes any UI components
        /// </summary>
        private void initBattleField()
        {
            AssetLoader.loadStaticAssets(GameState.BATTLE_STATE);
            generateCombatantSprites();
            initPlayerUI();

        }

        /// <summary>
        /// Generates the monster and player party sprites to screen
        /// </summary>
        private void generateCombatantSprites()
        {
            foreach (Monster monster in combatController.monsterParty.asList())
            {
                monster.combatSprite.transform.SetParent(monster_horizontalLayout.transform);
            }

            foreach (Adventurer player in combatController.playerParty.asList())
            {
                player.combatSprite.transform.SetParent(player_horizontalLayout.transform);
            }
        }

        /// <summary>
        /// Calls a load function to retrieve the ability icon textures from disk.
        /// </summary>
        private void initPlayerUI()
        {
            if (combatPanel == null)
            {
                combatPanel = CombatPanel.create(combatPanelLayout.GetComponent<RectTransform>());
                TabbedPanelControl.create(combatPanel, combatPanelButtons.GetComponent<RectTransform>());
            }
        }

        /*---------------------------------------------------------------
                           UI-CALLBACKS 
         ---------------------------------------------------------------*/
        /// <summary>
        /// The combatant with an active turn has their green turn chevron indicator enabled, name dispalyed in bottom left
        /// </summary>
        public void updateTurnUI()
        {
            Combatant currentCombatant = combatController.currentCombatant;
            Combatant nextCombatant = combatController.nextCombatant;

            TurnChevron.updateTurnChevrons(currentCombatant.combatSprite.transform, nextCombatant.combatSprite.transform, combatController.isPlayerTurn);
            currentTurnDisplayName.text = currentCombatant.name;
        }

        /// <summary>
        /// Update condition effect duractions for current combatant and redraw changes.
        /// </summary>
        public void updateConditionUI()
        {
            Combatant currentCombatant = combatController.currentCombatant;
            List<int> removedConditions = currentCombatant.updateConditionDurations();
            if (removedConditions.Count > 0)
            {
                foreach (var condition in removedConditions)
                {
                    FloatingPopup.create(currentCombatant.combatSprite.transform.position, EffectProcessor.getEffectLabel(condition), Color.blue);
                }
            }

            foreach (var players in combatController.playerParty.asList().FindAll(player => player.isAlive()))
            {
                players.combatSprite.updateConditions();
            }

            foreach (var monsters in combatController.monsterParty.asList().FindAll(monster => monster.isAlive()))
            {
                monsters.combatSprite.updateConditions();
            }
        }

        /// <summary>
        /// Calls for cooldown trackers to be updated 
        /// </summary>
        private void updateCooldownUI()
        {
            for (int i = 0; i < clientPlayer.playerClass.abilities.Count; i++)
            {
                setCooldownUI(i);
            }
        }

        /// <summary>
        /// Updates the UI components to reflect the current ability cooldowns in progres 
        /// </summary>
        /// <param name="abilityIndex">The index of the ability that was used on a combatant's turn.</param>
        public void setCooldownUI(int abilityIndex)
        {
            bool isPlayerTurn = combatController.isClientPlayerTurn;

            Ability ability = clientPlayer.playerClass.abilities[abilityIndex];
            AbilityButton abilityButton = AbilityButtons.buttons[abilityIndex];
            Color color = abilityButton.button.GetComponent<Image>().color;
            Text cooldownText = abilityButton.cooldownText;
            string cooldownValue = "";

            if (isPlayerTurn)
            {
                AbilityButton.selectedAbilityIndex = -1;
                if (ability.isOnCooldown)
                {
                    color.a = .3f;
                    cooldownValue = (ability.cooldownTracker + 1).ToString();
                }
                else
                {
                    color.a = 1f;
                    abilityButton.cooldownText.text = "";
                }
            }
            else
            {
                color.a = .3f;
                if (ability.isOnCooldown)
                {
                    if (ability.cooldownTracker > 1)
                    {
                        cooldownValue = (ability.cooldownTracker + 1).ToString();
                    }
                }
            }
            abilityButton.button.GetComponent<Image>().color = color;
            abilityButton.cooldownText.text = cooldownValue;
        }

        /*---------------------------------------------------------------
                            TEST/DEBUG FUNCTION
        /***************************************************************
        **************************************************************/
        /// <summary>
        /// Used to 'skip'/progress the turn order to next combatant is used as an event handler for End Turn button.
        /// </summary>
        /// <remarks> 
        /// This should only be used during <b>debug/development</b>.
        /// </remarks>
        public void skipTurn()
        {
            combatController.takeTurn();
        }


        /// <summary>
        /// Updates The UI to reflect a new turn in the combat order. 
        /// </summary>
        private void updateUI()
        {
            updateSpriteUI();
            updateTurnUI();
            updateCooldownUI();

            if (combatController.turnCount > 1)
            {
                updateConditionUI();
            }

            combatController.hasNextTurn = false;

            if (combatController.currentCombatant.isImpaired)
            {
                FloatingPopup.create(combatController.currentCombatant.combatSprite.transform.position, "Turn skipped", Color.black);
                combatController.takeTurn();

            }
        }

        /// <summary>
        /// Destroys any combatants who have been defeated
        /// </summary>
        private void updateSpriteUI()
        {
            foreach (var player in combatController.playerParty.defeated)
            {
                if (player.combatSprite != null)
                {
                    Destroy(player.combatSprite.gameObject);
                }
            }

            foreach (var monster in combatController.monsterParty.defeated)
            {
                if (monster.combatSprite != null)
                {
                    Destroy(monster.combatSprite.gameObject);
                }
            }

        }

        /// <summary>
        /// Checks for a new turn comamnd to be isused by the TurnController and executes various callbacks to reflect the current turn. 
        /// </summary>
        void Update()
        {
            // Multiplayer server/client communication instruction checks
            if (!Game.isSinglePlayer)
            {
                if (Game.gameClient.ready())
                {
                    processServerInstructions(Game.gameClient.read());
                }
            }

            // No combat round has started
            if (!combatController.hasCombat)
            {
                return;
            }

            //Combat is over
            if (combatController.hasCombatEnded)
            {
                processEndOfCombat();
            }
            else
            {
                // Next turn can be taken
                if (combatController.hasNextTurn)
                {
                    updateUI();
                }
            }
        }

        /// <summary>
        /// Displays victory/defeat panel and resets the combat encounter
        /// </summary>
        private void processEndOfCombat()
        {
            updateSpriteUI();

            string panelHeading = "";
            string panelDescription = "Game will reset in:";

            if (combatController.hasPlayerTeamWon)
            {
                panelHeading = "Victory";
            }
            else
            {
                panelHeading = "Defeat";
            }

            combatController.hasCombatEnded = false;
            combatController.hasCombat = false;

            TimerCallBack callBack = startCombat;

            TimedPanel endOfCombatPanel = TimedPanel.create(5, callBack, panelHeading, panelDescription);
            endOfCombatPanel.transform.SetParent(notificationPanel.transform);
        }

        private void processServerInstructions(string instructions)
        {
            //Ignore non-valid or server keep-alive token
            if (string.IsNullOrEmpty(instructions) || instructions[0] == Client.SERVER_ALIVE_TOKEN)
            {
                return;
            }

            //Begin processing
            BattleMessage message = new BattleMessage(instructions);

            switch ((BattleInstruction)message.instructionType)
            {
                case BattleInstruction.TURN_PROGRESSED:
                    combatController.takeTurn();
                    break;
                case BattleInstruction.TURN_ACTION:
                    processTurnAction(ref message);

                    break;

                default:
                    break;
            }
        }

        private void processTurnAction(ref BattleMessage message)
        {
            //Apply ability on player turn
            if (combatController.isPlayerTurn)
            {
                //Buff/heal ally
                if (message.abilityTargeting == TargetingType.ALLIED)
                {
                    foreach (var target in combatController.playerParty)
                    {
                        if (target.isAlive())
                        {
                            if (message.targets.ContainsKey(target.id))
                            {
                                Combatant allyPlayer = Game.getPlayerById(target.id).playerClass;
                            }
                        }
                    }
                }
                else if (message.abilityTargeting == TargetingType.ENEMY)
                {
                    Combatant caster = Game.getPlayerById(message.clientId).playerClass;
                    Debug.Log("Caster = " + caster.name);
                    FloatingPopup.create(caster.combatSprite.transform.position, message.abilityName, Color.black);
                    foreach (var target in message.targets)
                    {
                        Combatant enemyMonster = combatController.monsterParty.asList()[target.Key];
                        //Calculate damage taken as this clients value for the mosnter's hp - the new value
                        int damageDealt = (int)enemyMonster.healthProperties.currentHealth - (int)target.Value;
                        enemyMonster.applyDamage(damageDealt);
                    }
                }
                else if (message.abilityTargeting == TargetingType.AUTO)
                {
                    Combatant caster = Game.getPlayerById(message.clientId).playerClass;
                    Debug.Log("Caster = " + caster.name);
                    FloatingPopup.create(caster.combatSprite.transform.position, message.abilityName, Color.black);

                    foreach (var target in message.targets)
                    {
                        Combatant enemyMonster = combatController.monsterParty.asList()[target.Key];
                        //Calculate damage taken as this clients value for the mosnter's hp - the new value
                        int damageDealt = (int)enemyMonster.healthProperties.currentHealth - (int)target.Value;
                        enemyMonster.applyDamage(damageDealt);
                    }
                }
            }//Apply ability on monster turn
            else
            {

            }

        }
    }
}
