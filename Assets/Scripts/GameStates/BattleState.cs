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
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using Assets.Scripts.Util;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.Entities.Monsters;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Abilities;
using SimpleJSON;
using Assets.Scripts.UI;
#endregion IMPORTS

public class BattleState : MonoBehaviour
{
    [SerializeField] private GameObject player_horizontalLayout;
    [SerializeField] private GameObject monster_horizontalLayout;
    [SerializeField] private GameObject abilityBarLayout;
    [SerializeField] private Text currentPlayerName;
    private List<AbilityButton> abilityButtons;

    private List<Combatable> targets;
    private List<Monster> monsterParty;

    private static int turnCount = 0;
    private int selectedSkill = -1;
    private int currentMonster = 0;
    private int currentPlayer = 0;

    //Game Flags
    private static bool isTargetable = false;
    private bool hasValidTarget = false;

    private bool isClientPlayerTurn = false;
    private bool isEnemyTargetable = false;
    private bool isStandardTooltip = false;
    private bool hasNextCombatant = true;

    //Alias to  player -> isClientSidePlayer = true
    Player clientSidePlayer;
    Adventurer clientPlayerClass;

    /*---------------------------------------------------------------
                       GAME STATE INIATIALISATIONS
     ---------------------------------------------------------------*/
    /***************************************************************
    * Called on Scene initialization
    **************************************************************/
    private void Awake()
    {
        runTest(); //Development/Debug Only!
        initMonsterParty();
        initCombatOrder();
        initPlayerUI();
        takeTurn();
    }

    #region DEVELOPMENT/TEST FUNCTIONS
    //################################################################
    /*---------------------------------------------------------------
                            TEST-DATA/PROCEDURES
     ---------------------------------------------------------------*/
    //Test Data
    private Texture2D[] classIcons;
    private void runTest()
    {
        ViewController.INSTANCE.setStateScript(2);
        createTestPlayers();
        initClassData();
        //testNewAssetLoading();
    }

    /***************************************************************
    *  Creates a list of players for testing/debuggin purposes.
    **************************************************************/
    private void createTestPlayers()
    {
        Game.players = new List<Player>();
        //@Test Data
        Game.players.Add(new Player(6, "Alexjp", 3, true));
        Game.players.Add(new Player(8, "Frictionburn", 0, true));
        Game.players.Add(new Player(11, "Kozza", 2, true));
        Game.players.Add(new Player(4, "Wizzledonker", 3, true));

        Game.connectedPlayers = Game.players.Count;

        foreach (Player player in Game.players) player.applyClass();
        Game.players[0].isClientPlayer = true;
    }

    /***************************************************************
    *  Parses json file for class data and populates icon textures
    **************************************************************/
    private void initClassData()
    {
        const string CLASS_DATA_FILE = "assets/resources/data/meta_data.json";
        string classJSON = File.ReadAllText(CLASS_DATA_FILE);

        JSONNode json = JSON.Parse(classJSON);
        JSONArray classes = json["classes"].AsArray;

        int classCount = classes.Count;
        classIcons = new Texture2D[classCount];

        for (int i = 0; i < classCount; i++)
        {
            classIcons[i] = Resources.Load(classes[i]["icon_path"].Value) as Texture2D;
            Adventurer.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_path"].Value);
        }
    }
    //################################################################
    #endregion DEVELOPMENT/TEST FUNCTIONS

    /***************************************************************
    * Generates the monster party via the monster factory class.
   
    * Addtionally, this function sets the UI components relevant to
      the monsters loaded from the factory.
    **************************************************************/
    private void initMonsterParty()
    {
        int monsterPartySize = 3;
        MonsterFactory mFactory = new MonsterFactory();
        monsterParty = mFactory.createMonsterParty(monsterPartySize);

        foreach (Monster monster in monsterParty)
        {
            monster.combatSprite.transform.SetParent(monster_horizontalLayout.transform);
        }
    }

    /***************************************************************
    * Creates the initial combat order for the players and monsters.
    
    * The combat order is determined through randomising the order 
      of both  Game.players and the monster party generated in 
      initMonsterParty().
    
    * The targets dictionary is initialised in this function
      which is used to map the index of a sprite along with the
      data relating to its health, abilities and active conditions.
    **************************************************************/
    private void initCombatOrder()
    {
        targets = new List<Combatable>();
        System.Random r = new System.Random();
        Game.players = Game.players.OrderBy(player => r.Next()).ToList();
    }

    /***************************************************************
    * Populates the client side player's personal and party UI.
    
    * Assigns the components responsible for  players/party sprites, 
      HP bar/values and name plates.
   
    * initPlayerUI() calls a load function to retrieve the ability
      icon textures from disk.
    **************************************************************/
    private void initPlayerUI()
    {
        abilityButtons = new List<AbilityButton>();
        clientSidePlayer = Game.players.Find(player => player.isClientPlayer);
        clientPlayerClass = clientSidePlayer.playerClass;
        foreach (Adventurer player in Game.players.Select(player => player.playerClass))
        {
            player.combatSprite.transform.SetParent(player_horizontalLayout.transform);
        }

        //Load static assets (namely the lock for default ability icon image)
        AssetLoader.loadStaticAssets();
        try
        {
            clientPlayerClass.loadAbilities(Adventurer.getAbilityPath(clientPlayerClass.classId), true);
            for (int i = 0; i < Adventurer.ABILITY_LIMIT; i++)
            {
                AbilityButton button;
                if (i < clientPlayerClass.abilities.Count)
                {
                    button = AbilityButton.create(clientPlayerClass.abilities[i]);
                    button.icon.sprite = clientPlayerClass.abilities[i].assetData.sprite;
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

    /***************************************************************
    * takeTurn() is responsible for determining the next turn in combat. 
    
    * This function checks for client side player's turn and toggles 
      the active/inactive state of the player ability bar.
    
    * The combatant with an active turn has their green turn
      chevron indicator enabled.
     
    **************************************************************/
    public void takeTurn()
    {
        if (!hasNextCombatant) return;


        bool isPlayerTurn = ++turnCount % 2 == 1;
        int nextCombatant = getCombatant(isPlayerTurn);
        Combatable combatant = isPlayerTurn ?  (Combatable) Game.players[nextCombatant].playerClass : monsterParty[nextCombatant];
         
        togglePlayerTurn(isPlayerTurn && Game.players[nextCombatant].id == clientSidePlayer.id);
        TurnChevron.setPosition(combatant.combatSprite.transform.transform);
        currentPlayerName.text = combatant.name;

        //Update condition effect duractions
        List<int> removedConditions = combatant.updateConditionDurations();
        if (removedConditions.Count > 0)
        {
            foreach (var condition in removedConditions)
            {
                FloatingPopup.create(combatant.combatSprite.transform.position, EffectProcessor.getEffectLabel(condition), Color.blue);
            }

        }

        if (!isPlayerTurn) takeMonsterTurn(nextCombatant);
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
                    break;
                }
            }
            currentMonster++;
        }

        return nextComtatant;
    }

    /***************************************************************
    *  Calls for cooldown trackers to be updated

    @param - isPlayerTurn - passed in to setCooldownUI to determine
    what colors and values to update the coodlown UI with.
    **************************************************************/
    private void togglePlayerTurn(bool isPlayerTurn)
    {
        isClientPlayerTurn = isPlayerTurn;

        for (int i = 0; i < clientPlayerClass.abilities.Count; i++)
        {
            setCooldownUI(i, isPlayerTurn);
        }
    }

    /***************************************************************
    * Updates the UI components to reflect the current ability
      cooldowns in progres

    @param - isPlayerTurn - if true, all non-cooldowned abilities
    will become visually active - if false the  ability panel
    will be visually disabled.
    **************************************************************/
    private void setCooldownUI(int abilityIndex, bool isPlayerTurn)
    {
        Ability ability = clientPlayerClass.abilities[abilityIndex];
        AbilityButton abilityButton = abilityButtons[abilityIndex];
        Color color = abilityButton.button.GetComponent<Image>().color;
        Text cooldownText = abilityButtons[abilityIndex].cooldownText;

        string cooldownValue = "";
        if(isPlayerTurn)
        {
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
        System.Random r = new System.Random();

        int[] potentialTargets = Enumerable.Range(0, Game.players.Count).OrderBy(player => r.Next()).ToArray();
        int target = -1;
        for (int i = 0; i < potentialTargets.Length; i++)
        {
            if (Game.players[potentialTargets[i]].playerClass.isAlive())
            {
                target = potentialTargets[i];
                break;
            }
        }

        if (target == -1)
        {
            hasNextCombatant = false;
        }
        else
        {
            targets.Add(Game.players[target].playerClass);
            attackTarget(Game.players[target].playerClass, monsterParty[monsterIndex], 4, 5);
        }
        takeTurn();
    }

    /*---------------------------------------------------------------
                        CLIENT EVENT HANDLERS
     ---------------------------------------------------------------*/
    //ABILITY HANDLERS
    /*************************************************************** 
    * On mouse hover over ability icons, the tooltip for that ability
      is displayed.

    * if an ability is locked or not loaded, the called function 
      will throw an ArgumentOutOfRangeException which indicates
      that a standard tooltip should be displayed.

    @param - skillIndex: a value between 0-4 indicating which
    ability from the ability bar is being hovered over.
    **************************************************************/
    public void onAbilityHoverEnter(in Ability ability)
    {
        isStandardTooltip = ability == null;
        if(isStandardTooltip) Tooltip.show("Locked");
        else AbilityTooltip.show(ability);  
    }

    /***************************************************************
    * Event handler for hiding the ability tooltips upon mouse exit.
    **************************************************************/
    public void onAbilityHoverExit()
    {
        if (isStandardTooltip) Tooltip.hide();
        else AbilityTooltip.hide();
    }

    /***************************************************************
    * Event handler for click action in ability icon.
    
    * Determines the ability type of the ability that was clicked
      and provides appropriate operations for populating the targets
      dictionary or otherwise displaying targeting information
      through setting the tint of targeteable sprites.
          
    @param - ability: The ability that was clicked
    **************************************************************/
    public void onAbilityClicked(Ability ability)
    {
        if (ability == null) return;
        if (!isClientPlayerTurn) return;
        if (ability.isOnCooldown) return;

        resetTargets();
        selectedSkill = clientPlayerClass.abilities.FindIndex(_ability => _ability.id == ability.id);

        switch ((AbilityTypes)ability.typeIds[0])
        {
            //Self Target
            case AbilityTypes.SELF_HEAL:
            case AbilityTypes.SELF_BUFF:
                isTargetable = false;
                hasValidTarget = true;
                targets.Add(clientPlayerClass);
                int clientIndex = Game.getPlayerIndex(clientSidePlayer.id);
                Game.players[clientIndex].playerClass.combatSprite.sprite.color = Color.green;
                break;

            //Single Target - note: Defers targeting
            case AbilityTypes.SINGLE_DAMAGE:
            case AbilityTypes.SINGLE_DEBUFF:
                isTargetable = true;
                isEnemyTargetable = true;
                break;

            case AbilityTypes.SINGLE_BUFF:
            case AbilityTypes.SINGLE_HEAL:
                isTargetable = true;
                isEnemyTargetable = false;
                break;

            //Multi Target 
            case AbilityTypes.MULTI_DAMAGE:
            case AbilityTypes.MULTI_DEBUFF:
                isTargetable = false;
                hasValidTarget = true;
                
                monsterParty //Update monster sprite colors 
                .FindAll(monster=> monster.isAlive())
                .ForEach(monster => {
                    monster.combatSprite.sprite.color = Color.red;
                    targets.Add(monster);
                });
                break;

            case AbilityTypes.MULTI_HEAL:
            case AbilityTypes.MULTI_BUFF:
                isTargetable = false;
                hasValidTarget = true;
           
                Game.players //Update Player sprite colors
                .FindAll(player => player.playerClass.isAlive())
                .ForEach(player => {
                    player.playerClass.combatSprite.sprite.color = Color.green;
                    targets.Add(player.playerClass);
                });
                break;
        }
    }

    /***************************************************************
    * Resets targeting colors to default, and clears any exsiting
      targets from the previous turn.
    **************************************************************/
    private void resetTargets()
    {
        hasValidTarget = false;
        isTargetable = false;
        targets.Clear();

        Game.players 
        .FindAll(player => player.playerClass.isAlive())
        .ForEach(player => {
            player.playerClass.combatSprite.sprite.color = Color.white;
        });

        monsterParty 
        .FindAll(monster => monster.isAlive())
        .ForEach(monster => {
            monster.combatSprite.sprite.color = Color.white;
        });

    }

    //SPRITE HANDLERS
    /***************************************************************
    * Event handler for defered targeting types (single-target 
      abiltiies).
    
    * Includes the logic for ensuring the ability type's targeting
      type has the desired effect on displaying/highlighting
      the target selection.
    
    @param - spriteIndex: a value between 0-players.count() OR
    0-monsterParty.count() which represents the sprite that was 
    hovered over. 
    **************************************************************/
    public void onSpriteEnter(in Combatable combatant)
    {
        if (!isTargetable) return;
        targets.Clear();
        hasValidTarget = false;

        //Party Member hovered over
        if (!combatant.combatSprite.isMonster)
        {
            if (!isEnemyTargetable)
            {
                combatant.combatSprite.sprite.color = Color.green;
                hasValidTarget = true;
                targets.Add(combatant);
            }
        }//Enemy sprite hovered over
        else
        {
            if (isEnemyTargetable)
            {
                combatant.combatSprite.sprite.color = Color.red;
                hasValidTarget = true;
                targets.Add(combatant);
            }
        }
    }

    /***************************************************************
    * Provides the opposite functionality to onSpriteEnter(),
      while having selected a defered targeting type the mouse exit
      event will set the sprite tint back to the sprite's 
      original color.

    @param - spriteIndex: a value between 0-players.count() OR
    0-monsterParty.count() which represents the sprite that was 
    hovered over. 
    **************************************************************/
    public void onSpriteExit(in Combatable combatant)
    {
        if (!isTargetable) return;
        targets.Clear();
        hasValidTarget = false;

        if (!combatant.combatSprite.isMonster)
        {
            if (!isEnemyTargetable)
            {
                combatant.combatSprite.sprite.color = Color.white;
                hasValidTarget = false;
            }
        }
        else
        {
            if (isEnemyTargetable)
            {
                combatant.combatSprite.sprite.color = Color.white;
                hasValidTarget = false;
            }
        }
    }

    /***************************************************************
    * Event handler for when a sprite is clicked after selecting
      a valid target with valid ability.

    * Applies the abilities effects to the target. 
     
      @param - spriteIndex: a value between 0-players.count() OR
      0-monsterParty.count() which represents the sprite that was 
      hovered over. 
    **************************************************************/
    public void onSpriteClicked(in Combatable combatant)
    {
        if (!hasValidTarget) return;
        if (selectedSkill == -1) return;

        Ability abilityUsed = clientPlayerClass.abilities[selectedSkill];
        foreach (var target in targets)
        {
            foreach (var abilityType in abilityUsed.typeIds)
            {
                MetaTypes metaType = AbilityFactory.getMetaType(abilityType);
                if(target.combatSprite.isMonster)
                {
                    if (metaType == MetaTypes.DAMAGE)
                    {
                        attackTarget(target, clientPlayerClass, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                    }
                }
                else
                {
                    if (metaType == MetaTypes.HEALING)
                    {
                        healTarget(target, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                    }
                }
               
                if (metaType == MetaTypes.EFFECT)
                {
                    affectTarget(target, abilityUsed.statusEffect, abilityUsed.abilityStrength.max, abilityUsed.abilityStrength.min);
                    appyAfterEffect(ref abilityUsed); //For client side caster of special case abilities
                }
            }
        }

        //Update Cooldown / Targets 
        abilityUsed.setLastTurnUsed(turnCount); //Flagging whether cooldown
        abilityUsed.cooldownTracker++;
        clientPlayerClass.updateAbilityCooldowns(); 
        setCooldownUI(selectedSkill, turnCount % 2 == 1);
        onSpriteExit(in combatant);
        resetTargets();
        takeTurn();
    }


    /***************************************************************
    * Client side processing of any special-case scenarios
      when applying certian status effects. 
    **************************************************************/
    private void appyAfterEffect(ref Ability ability)
    {
        switch((StatusEffect)ability.statusEffect)
        {  
            case StatusEffect.COOLDOWN_CHANGE:  //The ability that casts a cooldown reduction, should not have the CDR applied.
                ability.cooldownTracker -= ability.abilityStrength.max;
                break;
        }
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
    private void affectTarget(Combatable target, int statusEffect, int potency, int turnsApplied)
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

        foreach (Condition condition in target.conditions)
        {
            switch ((StatusEffect)condition.effectId)
            {
                case StatusEffect.REFLECT_DAMAGE:
                    float damage = (float)damageDealt * ((float)condition.potency / 100);
                    attackTarget(caster, (int)damage);
                    break;
            }
        }

        if (!target.isAlive())
        {
            Destroy(target.combatSprite.gameObject);
            checkBattleState();
        }
        else
        {
            target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
            target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
        }

        FloatingPopup.create(target.combatSprite.transform.position, damageDealt.ToString(), Color.red);
    }

    /***************************************************************
    @Overload - Allows for precalculated damage to be done to a target
    in special combat conditions.
    **************************************************************/
    public void attackTarget(in Combatable target, int damage)
    {
        int damageDealt = target.applyDamage(damage);
        if (!target.isAlive())
        {
            Destroy(target.combatSprite.gameObject);
            checkBattleState();
        }
        else
        {
            target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
            target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
        }

        FloatingPopup.create(target.combatSprite.transform.position, damageDealt.ToString(), Color.red);
    }

    private void checkBattleState()
    {
        bool isInProgress = false;

        foreach (Player player in Game.players)
        {
            if (player.playerClass.isAlive())
            {
                isInProgress = true;
                break;
            }
        }

        if (isInProgress)
        {
            isInProgress = false;
            foreach (Monster monster in monsterParty)
            {
                if (monster.isAlive())
                {
                    isInProgress = true;
                    break;
                }
            }
        }

        hasNextCombatant = isInProgress;
    }
}