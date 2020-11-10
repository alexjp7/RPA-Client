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

#region IMPORTS
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Assets.Scripts.Util;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.UI;
using Assets.Scripts.GameStates;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.RPA_Messages;
using System.Threading.Tasks;
using System.Threading;
#endregion IMPORTS

#pragma warning disable 1234
public class BattleState : MonoBehaviour
{
    [SerializeField] private GameObject player_horizontalLayout;
    [SerializeField] private GameObject monster_horizontalLayout;
    [SerializeField] private GameObject abilityBarLayout;
    [SerializeField] private Text currentTurnDisplayName;
    private List<AbilityButton> abilityButtons;

    //Client Player Alias
    private Player clientPlayer { get => Game.clientSidePlayer; }
    //Combat Controllers
    public TurnController turnController { get; private set;}

    /*---------------------------------------------------------------
                       GAME STATE INIATIALISATIONS
     ---------------------------------------------------------------
    * Called on Scene initialization
    **************************************************************/
    void Awake()
    {
        initEnvironment(); // Updates static references
        initControllers(); // Constructs turn controller - party leader sends to server 
        initCombat(); // Sends/Requests combat data to other clients     
    }


    /**************************************************************
    * Sets this script as the current active state script, 
      allowing for static acces to callback/ui handlers and
      TurnController.
    **************************************************************/
    private void initEnvironment()
    {
        if (TestSimulator.isDeveloping) //Flaggged by REQUIRE_TEST_DATA directive in TestSimulaotr
        {
            TestSimulator.initTestEnvironment(GameState.BATTLE_STATE);
        }

        //If using Test data, ensure test environment is executed before this
        StateManager.setStateScript();
    }

    /**************************************************************
    * Initialises controller responsible for delegating and enforcing
      the turn based logic for combat rounds.
    **************************************************************/
    private void initControllers()
    {
        //Combat-wide controllers....
        turnController = new TurnController();
    }

    private async Task initCombat()
    {
        if (Game.isSinglePlayer)
        {
            initBattleField(); // UI initialisation
        }
        else //Handle Multiplayer session communication
        {
            if (clientPlayer.isPartyLeader)
            {
                turnController.initTurnOrder();
                turnController.initMonsterParty(4);
                await Task.Run(() => Message.send(new BattleMessage(BattleInstruction.COMBAT_INIT)) );
                initBattleField();
            }
            else
            {
                await requestBattleData();
                initBattleField();
            }
        }
    }

    private async Task requestBattleData()
    {
        //Wait for party leader to send combat oder/monster party data
        BattleMessage message = await Task.Run(() => awaitCombatInit());
        
        turnController.initTurnOrder(message.turnOrder);
        turnController.initMonsterParty(message.monsters);
    }

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
    ---------------------------------------------------------------
    /**************************************************************
    * Initializes any UI components
    **************************************************************/
    private void initBattleField()
    {
        Debug.Log("in initBattleField()");
        AssetLoader.loadStaticAssets(GameState.BATTLE_STATE);
        generateCombatantSprites();
        initPlayerUI();
    }
    

    /***************************************************************f
    * Generates the monster and player party sprites to screen.
    **************************************************************/
    private void generateCombatantSprites()
    {
        foreach (Monster monster in turnController.monsterParty)
        {
            monster.combatSprite.transform.SetParent(monster_horizontalLayout.transform);
        }

        foreach (Adventurer player in turnController.playerParty)
        {
            player.combatSprite.transform.SetParent(player_horizontalLayout.transform);
        }
    }

    /***************************************************************
    * initPlayerUI() calls a load function to retrieve the ability
      icon textures from disk.
    **************************************************************/
    private void initPlayerUI()
    {
        abilityButtons = new List<AbilityButton>();

        //Load static assets (namely the lock for default ability icon image)
        try
        {
            clientPlayer.playerClass.loadAbilities(Adventurer.getAbilityPath(clientPlayer.playerClass.classId), true);
            for (int i = 0; i < Adventurer.ABILITY_LIMIT; i++)
            {
                AbilityButton button;
                if (i < clientPlayer.playerClass.abilities.Count)
                {
                    button = AbilityButton.create(clientPlayer.playerClass.abilities[i]);
                    button.icon.sprite = clientPlayer.playerClass.abilities[i].assetData.sprite;
                    button.transform.SetParent(abilityBarLayout.transform);
                }
                else
                {
                    button = AbilityButton.create(null);
                    button.transform.SetParent(abilityBarLayout.transform);
                }
                abilityButtons.Add(button);
            }
        }
        catch (NotImplementedException e) { Debug.Log("ERROR:" + e.Message); }

    }

    void Start()
    {
        turnController.startCombat();
    }

    /*---------------------------------------------------------------
                    BATTLE-STATE CONTROL FLOWS
     ---------------------------------------------------------------*/
    //START OF TURN
    /***************************************************************
    * Client side processing of any status-effects (buffs/debuffs)
      that require exeucting at the start of a turn
    **************************************************************/
    public void applyBeforeEffects(in Combatable combatant)
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

    //END OF TURN
    /***************************************************************
    * Client side processing of any special-case scenarios
      when applying certian status effects. 
    **************************************************************/
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
    /***************************************************************
    * Performs a damaging action on a target; updates new hp value
      to the HP bar fill-amount and text value.
    
    * Additionaly, if the damage applied to a target causes their hp
      to fall below 0, the combat sprite is destroyed.

      @param - target: The combatant of which the ability is applied too.
      @param - minDamage: The lower bound of the damage being applied
      that is used to calculate the actual amount dealt.
      @param - maxDamage: The upper bound of the damage being applied
      that is used to calculate the actual amount dealt.
    **************************************************************/
    public void attackTarget(in Combatable target, in Combatable caster, int minDamage, int maxDamage)
    {
        int damageDealt = target.applyDamage(minDamage, maxDamage);

        //Reflect damage is applies
        if (target.conditions.ContainsKey((int)StatusEffect.REFLECT_DAMAGE))
        {
            float damage = (float)damageDealt * ((float)target.conditions[(int)StatusEffect.REFLECT_DAMAGE].potency / 100);
            attackTarget(caster, (int)damage);
        }

        if(caster.conditions.ContainsKey((int) StatusEffect.POISON_WEAPON) )
        {
            affectTarget(target, (int) StatusEffect.POISON, 6, 3);
        }


        if (!target.isAlive())
        {
            Destroy(target.combatSprite.gameObject);
        }
        else
        {
            //Remove Sleep if exists
            if (target.conditions.ContainsKey((int)StatusEffect.SLEEP) ) 
            {
                target.conditions.Remove((int)StatusEffect.SLEEP);
            }

            target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
            target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
        }

        FloatingPopup.create(target.combatSprite.transform.position, damageDealt.ToString(), Color.red);
    }


    /***************************************************************
    @Overload - Allows for precalculated damage to be done to a target
    in special combat conditions.
    **************************************************************/
    public void attackTarget(in Combatable target, int damage, string prefix = "")
    {
        int damageDealt = target.applyDamage(damage);
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
    /***************************************************************
    * Applies a combat status-effect to a target and displays
      text to user to indicate the abilitiy's effects.


    @param - target: The combatant of which the ability is applied too
    @param - statusEffect: numeric ID for an ability status effect.
    see EffectProcess.cs.
    @param - potency: The strength or duration if applicable of the 
    status effect
    **************************************************************/
    public void affectTarget(Combatable target, int statusEffect, int potency, int turnsApplied)
    {
        target.applyEffect(statusEffect, potency, turnsApplied);
        FloatingPopup.create(target.combatSprite.transform.position, EffectProcessor.getEffectLabel(statusEffect, potency), Color.blue);
    }

    /***************************************************************
    * Performs a healing action on a target; updates new hp value
      to the HP bar fill-amount and text value.

      @param - target: The combatant of which the ability is applied too
      @param - minHealing: The lower bound of the healing being applied
      that is used to calculate the actual amount healed.
      @param - maxHealing: The upper bound of the healing being applied
      that is used to calculate the actual amount healed.
    **************************************************************/
    public void healTarget(in Combatable target, int minHealing, int maxHealing)
    {
        int healingAmount = target.applyHealing((int)minHealing, (int)maxHealing);
        target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
        target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
        FloatingPopup.create(target.combatSprite.transform.position, healingAmount.ToString(), new Color(0, 100, 0));
    }

    /*---------------------------------------------------------------
                       UI-CALLBACKS 
     ---------------------------------------------------------------
   * The combatant with an active turn has their green turn
     chevron indicator enabled, name dispalyed in bottom left
   **************************************************************/
    public void updateTurnUi()
    {
        Combatable currentCombatant = turnController.currentCombatant;
        TurnChevron.setPosition(currentCombatant.combatSprite.transform);
        currentTurnDisplayName.text = currentCombatant.name;
    }

    public void updateConditionUI()
    {
        //Update condition effect duractions for current combatant
        Combatable currentCombatant = turnController.currentCombatant;
        List<int> removedConditions = currentCombatant.updateConditionDurations();
        if (removedConditions.Count > 0)
        {
            foreach (var condition in removedConditions)
            {
                FloatingPopup.create(currentCombatant.combatSprite.transform.position, EffectProcessor.getEffectLabel(condition), Color.blue);
            }
        }

        //Redraw conditions for all Combatants
        foreach (var players in turnController.playerParty.FindAll(player => player.isAlive()))
        {
            players.combatSprite.updateConditions();
        }

        foreach (var monsters in turnController.monsterParty.FindAll(monster => monster.isAlive()))
        {
            monsters.combatSprite.updateConditions();
        }
    }

    /***************************************************************
    *  Calls for cooldown trackers to be updated

    @param - isPlayerTurn - passed in to setCooldownUI to determine
    what colors and values to update the coodlown UI with.
    **************************************************************/
    private void updateCooldownUI()
    {
        for (int i = 0; i < clientPlayer.playerClass.abilities.Count; i++)
        {
            setCooldownUI(i);
        }
    }

    /***************************************************************
    * Updates the UI components to reflect the current ability
      cooldowns in progres

    @param - isPlayerTurn - if true, all non-cooldowned abilities
    will become visually active - if false the  ability panel
    will be visually disabled.
    **************************************************************/
    public void setCooldownUI(int abilityIndex)
    {
        bool isPlayerTurn = turnController.isClientPlayerTurn;

        Ability ability = clientPlayer.playerClass.abilities[abilityIndex];
        AbilityButton abilityButton = abilityButtons[abilityIndex];
        Color color = abilityButton.button.GetComponent<Image>().color;
        Text cooldownText = abilityButtons[abilityIndex].cooldownText;
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
                ability.setLastTurnUsed(-1);
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
    * Used to 'skip'/progress the turn order to next combatant
     is used as an event handler for End Turn button,
     and should only be used during debug/development.
    **************************************************************/
    public void skipTurn()
    {
        turnController.takeTurn();
    }


    /***************************************************************
    * Updates The UI to reflect a new turn in the combat order.
    **************************************************************/
    private void updateUI()
    {
        updateTurnUi();
        updateCooldownUI();

        if(turnController.turnCount > 1)
        {
            updateConditionUI();

        }
        turnController.hasNextTurn = false;
    }



    /***************************************************************
    * Checks for a new turn comamnd to be isused by the TurnController
     and executes various callbacks to reflect the current turn.
    **************************************************************/
    void Update()
    {
        if (!Game.isSinglePlayer)
        {
            if (Game.gameClient.ready())
            {
                processServerInstructions(Game.gameClient.read());
            }
        }

        if (turnController.hasNextTurn)
        {
            updateUI();
        }
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
        if(turnController.isPlayerTurn)
        {
            //Buff/heal ally
            if (message.abilityTargeting == TargetingType.ALLIED)
            {
                foreach (var target in turnController.playerParty)
                {
                    if(target.isAlive())
                    {
                        if(message.targets.ContainsKey(target.id))
                        {
                            Combatable allyPlayer = Game.getPlayerById(target.id).playerClass;

                        }
                    }
                }
            }
            else if (message.abilityTargeting == TargetingType.ENEMY)
            {
                Combatable caster = Game.getPlayerById(message.clientId).playerClass;
                Debug.Log("Caster = " + caster.name);
                FloatingPopup.create(caster.combatSprite.transform.position, message.abilityName, Color.black);
                foreach (var target in message.targets)
                {
                    Combatable enemyMonster = turnController.monsterParty[target.Key];
                    //Calculate damage taken as this clients value for the mosnter's hp - the new value
                    int damageDealt = (int)enemyMonster.healthProperties.currentHealth - (int)target.Value;
                    attackTarget(enemyMonster, damageDealt);
                }
            }
            else if(message.abilityTargeting == TargetingType.AUTO)
            {
                Combatable caster = Game.getPlayerById(message.clientId).playerClass;
                Debug.Log("Caster = "+caster.name);
                FloatingPopup.create(caster.combatSprite.transform.position, message.abilityName, Color.black);

                foreach (var target in message.targets)
                {
                    Combatable enemyMonster = turnController.monsterParty[target.Key];
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

