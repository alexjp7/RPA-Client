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
#endregion IMPORTS

#pragma warning disable 1234
public class BattleState : MonoBehaviour
{
    [SerializeField] private GameObject player_horizontalLayout;
    [SerializeField] private GameObject monster_horizontalLayout;
    [SerializeField] private GameObject abilityBarLayout;
    [SerializeField] private Text currentTurnDisplayName;


    private List<AbilityButton> abilityButtons;
    private Player clientPlayer { get => Game.clientSidePlayer; }


    /*---------------------------------------------------------------
                       GAME STATE INIATIALISATIONS
     ---------------------------------------------------------------
    * Called on Scene initialization
    **************************************************************/
    void Awake()
    {
        if (TestSimulator.isDeveloping)
            TestSimulator.initTestEnvironment(GameState.BATTLE_STATE);

        StateManager.setStateScript();
        TurnController.INSTANCE.init();
        AssetLoader.loadStaticAssets(GameState.BATTLE_STATE);
        generateCombatantSprites();
        initPlayerUI();
    }

    void Start()
    {
        TurnController.INSTANCE.startCombat();
    }


    /***************************************************************f
    * Generates the monster and player party sprites to screen.
    **************************************************************/
    private void generateCombatantSprites()
    {
        foreach (Monster monster in TurnController.INSTANCE.monsterParty)
        {
            monster.combatSprite.transform.SetParent(monster_horizontalLayout.transform);
        }

        foreach (Adventurer player in TurnController.INSTANCE.playerParty)
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
        Combatable currentCombatant = TurnController.INSTANCE.currentCombatant;
        TurnChevron.setPosition(currentCombatant.combatSprite.transform);
        currentTurnDisplayName.text = currentCombatant.name;
    }

    public void updateConditionUI()
    {
        //Update condition effect duractions for current combatant
        Combatable currentCombatant = TurnController.INSTANCE.currentCombatant;
        List<int> removedConditions = currentCombatant.updateConditionDurations();
        if (removedConditions.Count > 0)
        {
            foreach (var condition in removedConditions)
            {
                FloatingPopup.create(currentCombatant.combatSprite.transform.position, EffectProcessor.getEffectLabel(condition), Color.blue);
            }
        }

        //Redraw conditions for all Combatants
        List<Combatable> allCombatants = new List<Combatable>( TurnController.INSTANCE.playerParty.FindAll( player => player.isAlive() ) );
        allCombatants.AddRange(TurnController.INSTANCE.monsterParty.FindAll(monster => monster.isAlive() ) );
        foreach (var combatants in allCombatants)
        {
            combatants.combatSprite.updateConditions();
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
        bool isPlayerTurn = TurnController.INSTANCE.isClientPlayerTurn;

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
        TurnController.INSTANCE.takeTurn();
    }


    /***************************************************************
    * Updates The UI to reflect a new turn in the combat order.
    **************************************************************/
    private void updateUI()
    {
        updateTurnUi();
        updateCooldownUI();
        updateConditionUI();
        TurnController.INSTANCE.hasNextTurn = false;
    }

    /***************************************************************
    * Checks for a new turn comamnd to be isused by the TurnController
     and executes various callbacks to reflect the current turn.
    **************************************************************/
    void Update()
    {
        if (TurnController.INSTANCE.hasNextTurn)
        {
            updateUI();
        }
    }
}