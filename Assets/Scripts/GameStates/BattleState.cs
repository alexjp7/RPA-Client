
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
        public TurnController turnController { get; private set; }

        void Awake()
        {
            initEnvironment(); // Updates static references
            initControllers(); // Constructs turn controller - party leader sends to server 
            startCombat();
        }

        private async void startCombat()
        {
            await initCombat(); // Sends/Requests combat data to other clients     
            turnController.startCombat();
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
            turnController = new TurnController();
        }

        /// <summary>
        /// Initializes combat order, monster party and handles any server/client communication
        /// </summary>
        /// <returns>The initialisation task which awaits the return of any server communication.</returns>
        public async Task initCombat()
        {
            if (Game.isSinglePlayer)
            {
                turnController.resetCombat();
                turnController.generateTurnOrder();
                turnController.initMonsterParty(4);
                initBattleField(); // UI initialisation
            }
            else //Handle Multiplayer session communication
            {
                if (clientPlayer.isPartyLeader)
                {
                    turnController.generateTurnOrder();
                    turnController.initMonsterParty(4);
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

            turnController.generateTurnOrder(message.turnOrder);
            turnController.initMonsterParty(message.monsters);
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
            foreach (Monster monster in turnController.monsterParty.asList())
            {
                monster.combatSprite.transform.SetParent(monster_horizontalLayout.transform);
            }

            foreach (Adventurer player in turnController.playerParty.asList())
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
                        BATTLE-STATE CONTROL FLOWS
         ---------------------------------------------------------------*/
        /// <summary>
        /// Client side processing of any status-effects(buffs/debuffs) that require exeucting at the start of a turn
        /// </summary>
        /// <param name="combatant"> The combatant whos turn it is.</param>
        public void applyBeforeEffects(in Combatant combatant)
        {
            //Select All the precondition effects
            List<Condition> preConditions = new List<Condition>();

            foreach (var condition in combatant.conditions)
            {
                if (EffectProcessor.PreConditionEffects.Contains((StatusEffect)condition.Key))
                {
                    preConditions.Add(condition.Value);
                }
            }

            //Apply Effect to target - Only works for Damage over time effects (bleedd/poison)
            foreach (var condition in preConditions)
            {
                attackTarget(combatant, condition.potency * condition.stacks, EffectProcessor.getEffectLabel(condition.effectId, 0));
            }
        }

        /// <summary>
        /// Client side processing of any special-case scenarios when applying certian status effects
        /// </summary>
        /// <param name="ability">The ability which was used on a turn.</param>
        public void applyAfterEffect(ref Ability ability)
        {
            switch ((StatusEffect)ability.statusEffect)
            {
                case StatusEffect.COOLDOWN_CHANGE:  //The ability that casts a cooldown reduction, should not have the CDR applied.
                    ability.cooldownTracker -= ability.abilityStrength.max;
                    break;
            }
        }


        //DURING TURN
        /// <summary>
        /// <para>
        /// Performs a damaging action on a target; updates new hp value to the HP bar fill-amount and text value.
        /// </para>
        /// Additionaly, if the damage applied to a target causes their hp  to fall below 0, the combat sprite is destroyed.
        /// </summary>
        /// <param name="target">The combatant of which the ability is applied too.</param>
        /// <param name="caster">The combatant which used the ability on the target.</param>
        /// <param name="minDamage">The lower bound of the damage being applied that is used to calculate the actual amount dealt.</param>
        /// <param name="maxDamage">The upper bound of the damage being applied that is used to calculate the actual amount dealt.</param>
        public void attackTarget(in Combatant target, in Combatant caster, int minDamage, int maxDamage)
        {
            int damageDealt = target.applyDamage(minDamage, maxDamage);

            log.Debug($"<b><color=red>[ATTACK]</color></b> - {turnController.currentCombatant.id}:\"{turnController.currentCombatant.name}\" attacks {target.id}:\"{target.name}\" with \"{turnController.lastAbilityUsed.name}\" for <color=red><b>{damageDealt}</b></color>");

            //Reflect damage is applies
            if (target.conditions.ContainsKey((int)StatusEffect.REFLECT_DAMAGE))
            {
                float damage = (float)damageDealt * ((float)target.conditions[(int)StatusEffect.REFLECT_DAMAGE].potency / 100);
                attackTarget(caster, (int)damage);
            }

            if (caster.conditions.ContainsKey((int)StatusEffect.POISON_WEAPON))
            {
                affectTarget(target, (int)StatusEffect.POISON, 6, 3);
            }


            if (!target.isAlive())
            {
                Destroy(target.combatSprite.gameObject);
            }
            else
            {
                //Remove Sleep if exists
                if (target.conditions.ContainsKey((int)StatusEffect.SLEEP))
                {
                    target.conditions.Remove((int)StatusEffect.SLEEP);
                }

                target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
                target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
            }

            FloatingPopup.create(target.combatSprite.transform.position, damageDealt.ToString(), Color.red);
        }


        /// <summary>
        /// Allows for precalculated damage to be done to a target in special combat conditions.
        /// </summary>
        /// <param name="target">The combatant of which the ability is applied too.</param>
        /// <param name="damage">The amount of damage dealt to the target</param>
        /// <param name="prefix">The text prefix that will be displayed alongside the damage number</param>
        public void attackTarget(in Combatant target, int damage, string prefix = "")
        {
            int damageDealt = target.applyDamage(damage);

            log.Debug($"<b><color=red>[ATTACK]</color></b> - {turnController.currentCombatant.id}:\"{turnController.currentCombatant.name}\" attacks {target.id}:\"{target.name}\"  for <color=red><b>{damageDealt}</b></color>");

            if (!target.isAlive())
            {
                Destroy(target.combatSprite.gameObject);
            }
            else
            {
                target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
                target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
            }

            FloatingPopup.create(target.combatSprite.transform.position, $"{prefix}  {damageDealt.ToString()}", Color.red);
        }

        /// <summary>
        /// Applies a combat status-effect to a target and displays text to user to indicate the abilitiy's effects.
        /// </summary>
        /// <param name="target">The combatant of which the ability is applied too</param>
        /// <param name="statusEffect">ID for an ability status effect. <see cref="Assets.Scripts.Entities.Combat.EffectProcessor">EffectProcessor</see></param>
        /// <param name="potency">The strength or duration if applicable of the status effect</param>
        /// <param name="turnsApplied"></param>
        public void affectTarget(Combatant target, int statusEffect, int potency, int turnsApplied)
        {
            String conditionLabel = EffectProcessor.getEffectLabel(statusEffect, potency);
            log.Debug($"<b><color=blue>[CONDITION]</color></b> - {turnController.currentCombatant.id}:\"{turnController.currentCombatant.name}\" affects {target.id}:\"{target.name}\" with <color=blue><b>{conditionLabel}</b></color>");

            target.applyEffect(statusEffect, potency, turnsApplied);
            FloatingPopup.create(target.combatSprite.transform.position, conditionLabel, Color.blue);
        }


        /// <summary>
        /// Performs a healing action on a target; updates new hp value to the HP bar fill-amount and text value.
        /// </summary>
        /// <param name="target">The combatant of which the ability is applied too</param>
        /// <param name="minHealing">The lower bound of the healing being applied that is used to calculate the actual amount healed.</param>
        /// <param name="maxHealing">The upper bound of the healing being applied that is used to calculate the actual amount healed.</param>
        public void healTarget(in Combatant target, int minHealing, int maxHealing)
        {
            int healingAmount = target.applyHealing((int)minHealing, (int)maxHealing);
            log.Debug($"<b><color=green>[HEAL]</color></b> - {turnController.currentCombatant.id}:\"{turnController.currentCombatant.name}\" heals {target.id}:\"{target.name}\" with \"{turnController.lastAbilityUsed.name}\" for <color=green><b>{healingAmount}</b></color>");
            target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
            target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
            FloatingPopup.create(target.combatSprite.transform.position, healingAmount.ToString(), new Color(0, 100, 0));
        }

        /*---------------------------------------------------------------
                           UI-CALLBACKS 
         ---------------------------------------------------------------*/
        /// <summary>
        /// The combatant with an active turn has their green turn chevron indicator enabled, name dispalyed in bottom left
        /// </summary>
        public void updateTurnUI()
        {
            Combatant currentCombatant = turnController.currentCombatant;
            Combatant nextCombatant = turnController.nextCombatant;

            TurnChevron.updateTurnChevrons(currentCombatant.combatSprite.transform, nextCombatant.combatSprite.transform, turnController.isPlayerTurn);
            currentTurnDisplayName.text = currentCombatant.name;
        }

        /// <summary>
        /// Update condition effect duractions for current combatant and redraw changes.
        /// </summary>
        public void updateConditionUI()
        {
            Combatant currentCombatant = turnController.currentCombatant;
            List<int> removedConditions = currentCombatant.updateConditionDurations();
            if (removedConditions.Count > 0)
            {
                foreach (var condition in removedConditions)
                {
                    FloatingPopup.create(currentCombatant.combatSprite.transform.position, EffectProcessor.getEffectLabel(condition), Color.blue);
                }
            }

            foreach (var players in turnController.playerParty.asList().FindAll(player => player.isAlive()))
            {
                players.combatSprite.updateConditions();
            }

            foreach (var monsters in turnController.monsterParty.asList().FindAll(monster => monster.isAlive()))
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
            bool isPlayerTurn = turnController.isClientPlayerTurn;

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
            turnController.takeTurn();
        }


        /// <summary>
        /// Updates The UI to reflect a new turn in the combat order. 
        /// </summary>
        private void updateUI()
        {
            updateTurnUI();
            updateCooldownUI();

            if (turnController.turnCount > 1)
            {
                updateConditionUI();
            }

            turnController.hasNextTurn = false;

            if (turnController.currentCombatant.isImpaired)
            {
                FloatingPopup.create(turnController.currentCombatant.combatSprite.transform.position, "Turn skipped", Color.black);
                turnController.takeTurn();

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
            if (!turnController.hasCombat)
            {
                return;
            }

            //Combat is over
            if (turnController.hasCombatEnded)
            {
                processEndOfCombat();
            }
            else
            {
                // Next turn can be taken
                if (turnController.hasNextTurn)
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
            string panelHeading = "";
            string panelDescription = "Game will reset in:";

            if (turnController.hasPlayerTeamWon)
            {
                panelHeading = "Victory";
            }
            else
            {
                panelHeading = "Defeat";
            }

            turnController.hasCombatEnded = false;
            turnController.hasCombat = false;

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
                    turnController.takeTurn();
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
            if (turnController.isPlayerTurn)
            {
                //Buff/heal ally
                if (message.abilityTargeting == TargetingType.ALLIED)
                {
                    foreach (var target in turnController.playerParty.asList())
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
                        Combatant enemyMonster = turnController.monsterParty.asList()[target.Key];
                        //Calculate damage taken as this clients value for the mosnter's hp - the new value
                        int damageDealt = (int)enemyMonster.healthProperties.currentHealth - (int)target.Value;
                        attackTarget(enemyMonster, damageDealt);
                    }
                }
                else if (message.abilityTargeting == TargetingType.AUTO)
                {
                    Combatant caster = Game.getPlayerById(message.clientId).playerClass;
                    Debug.Log("Caster = " + caster.name);
                    FloatingPopup.create(caster.combatSprite.transform.position, message.abilityName, Color.black);

                    foreach (var target in message.targets)
                    {
                        Combatant enemyMonster = turnController.monsterParty.asList()[target.Key];
                        //Calculate damage taken as this clients value for the mosnter's hp - the new value
                        int damageDealt = (int)enemyMonster.healthProperties.currentHealth - (int)target.Value;
                        attackTarget(enemyMonster, damageDealt);
                    }
                }
            }//Apply ability on monster turn
            else
            {

            }

        }
    }
}
