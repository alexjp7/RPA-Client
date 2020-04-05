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
    private bool isTargetable = false;
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
        initSpriteCallbacks();
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
        createTestPlayers();
        initClassData();
        //testNewAssetLoading();
    }

    private void testNewAssetLoading()
    {
        foreach (Player player in Game.players)
        {
            CombatSprite sprite = CombatSprite.create(player_horizontalLayout.transform.position, player.playerClass);
            sprite.transform.SetParent(player_horizontalLayout.transform);
        }

        foreach (Monster monster in monsterParty)
        {
            CombatSprite sprite = CombatSprite.create(monster_horizontalLayout.transform.position, monster);
            sprite.transform.SetParent(monster_horizontalLayout.transform);
        }
    }

    /***************************************************************
    *  Creates a list of players for testing/debuggin purposes.
    **************************************************************/
    private void createTestPlayers()
    {
        Game.players = new List<Player>();
        //@Test Data
        Game.players.Add(new Player(6, "Alexjp", 1, true));
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
            monster.combatSprite = CombatSprite.create(monster_horizontalLayout.transform.position, monster);
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
        int order = 0;
        Game.players.ForEach(player => player.playerClass.combatOrder = order++);
        order = 0;
        monsterParty.ForEach(monster => monster.combatOrder = order++);
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
            player.combatSprite = CombatSprite.create(player_horizontalLayout.transform.position, player);
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
    * Creates the event handlers for hovering and click events
      for the player party and monster party sprites.
       
    * These event handlers are programtically set to allow
      for dynamic arguments to represent the index of sprite
      that has the attached event, this allows varying 
      length player party and monster party.
    **************************************************************/
    private void initSpriteCallbacks()
    {
        EventTrigger.Entry mouseEnterEvent;
        EventTrigger.Entry mouseExitEvent;
        EventTrigger.Entry mouseClickeEvent;

        //Initialize Party Sprite Events
        foreach (Combatable combatant in Game.players.Select(player => player.playerClass))
        {
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onSpriteEnter(combatant));
            mouseExitEvent.callback.AddListener(evt => onSpriteExit(combatant));
            mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(combatant));

            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        //Initialize Monster Sprite Events
        foreach (Combatable combatant in monsterParty)
        {
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;
            mouseEnterEvent.callback.AddListener(evt => onSpriteEnter(combatant));
            mouseExitEvent.callback.AddListener(evt => onSpriteExit(combatant));
            mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(combatant));

            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            combatant.combatSprite.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }
        //Initialize Ability Handlers
        for (int i = 0; i < abilityButtons.Count; i++)
        {
            int j = i;
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onAbilityHoverEnter(j));
            mouseExitEvent.callback.AddListener(evt => onAbilityHoverExit());
            mouseClickeEvent.callback.AddListener(evt => onAbilityClicked(j));

            abilityButtons[i].GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            abilityButtons[i].GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            abilityButtons[i].GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }
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

        togglePlayerTurn(isPlayerTurn && Game.players[nextCombatant].id == clientSidePlayer.id);
        TurnChevron.setPosition(isPlayerTurn ? Game.players[nextCombatant].playerClass.combatSprite.transform
                                             : monsterParty[nextCombatant].combatSprite.transform);

        currentPlayerName.text = isPlayerTurn ? Game.players[nextCombatant].name : monsterParty[nextCombatant].name;
        if (!isPlayerTurn) takeMonsterTurn();
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
        if (isPlayerTurn) clientPlayerClass.updateAbilityCooldowns();
        
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
                if(ability.cooldownTracker > 0)
                {
                    cooldownValue = (ability.cooldownTracker).ToString();
                }
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
                    cooldownValue = (ability.cooldownTracker).ToString();
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
    private void takeMonsterTurn()
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
            attackTarget(Game.players[target].playerClass, 4, 5);
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
    public void onAbilityHoverEnter(int skillIndex)
    {
        try
        {
            AbilityTooltip.show(clientPlayerClass.abilities[skillIndex]);
            isStandardTooltip = false;
        }
        catch (ArgumentOutOfRangeException)
        {
            isStandardTooltip = true;
            Tooltip.show("Locked");
        }
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
          
    @param - skillIndex: a value between 0-4 indicating which
    ability from the ability bar has been clicked.
    **************************************************************/
    public void onAbilityClicked(int skillIndex)
    {
        if (!isClientPlayerTurn) return;
        if (clientPlayerClass.abilities[skillIndex].isOnCooldown) return;

        resetTargets();
        selectedSkill = skillIndex;
        Ability ability = clientPlayerClass.abilities[selectedSkill];

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
                monsterParty.ForEach(monster => monster.combatSprite.sprite.color = Color.red);
                for (int i = 0; i < monsterParty.Count; i++) targets.Add(monsterParty[i]);
                break;

            case AbilityTypes.MULTI_HEAL:
            case AbilityTypes.MULTI_BUFF:
                isTargetable = false;
                hasValidTarget = true;
                Game.players.ForEach(player => player.playerClass.combatSprite.sprite.color = Color.green);
                for (int i = 0; i < Game.players.Count; i++) targets.Add(Game.players[i].playerClass);
                break;
        }
    }

    private void resetTargets()
    {
        targets.Clear();
        Game.players.ForEach(player => player.playerClass.combatSprite.sprite.color = Color.white);
        monsterParty.ForEach(monster => monster.combatSprite.sprite.color = Color.white);
        hasValidTarget = false;
        isTargetable = false;
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
                        attackTarget(target, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
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
                    affectTarget(target, abilityUsed.statusEffect, abilityUsed.abilityStrength.max);
                }
            }
        }

        //Update cooldown /targets 
        abilityUsed.setLastTurnUsed(turnCount);
        setCooldownUI(selectedSkill, turnCount % 2 == 1);
        onSpriteExit(in combatant);
        resetTargets();
        takeTurn();
    }


    private void affectTarget(Combatable target, int statusEffect, int potency)
    {
        target.applyEffect(statusEffect, potency);
    }   
                
    /***************************************************************
    * Performs a healing action on a target; updates new hp value
      to the HP bar fill-amount and text value.

      @param - spriteIndex: a value between 0-players.count() OR
      0-monsterParty.count() which represents the sprite that was 
      hovered over. 
      @param - minHealing: The lower bound of the healing being applied
      that is used to calculate the actual amount healed.
      @param - maxHealing: The upper bound of the healing being applied
      that is used to calculate the actual amount healed.
      @param - isMonster: Used to determine which UI elements to update;
      between party or monster UI components
    **************************************************************/
    public void healTarget(in Combatable target, int minHealing, int maxHealing)
    {
        int healingAmount = target.applyHealing((int)minHealing, (int)maxHealing);
        target.combatSprite.healthBar.fillAmount = target.getHealthPercent();
        target.combatSprite.currentHealthValue.text = ((int)target.getCurrentHp()).ToString();
        DamagePopup.create(target.combatSprite.transform.position, healingAmount.ToString(), Color.green);
    }

    /***************************************************************
    * Performs a damaging action on a target; updates new hp value
      to the HP bar fill-amount and text value.
    
    * Additionaly, if the damage applied to a target causes their hp
      to fall below 0, the GameObject that contains the targets UI
      components will disabled, causing it to "dissapear" of the
      game scene.

      @param - spriteIndex: a value between 0-players.count() OR
      0-monsterParty.count() which represents the sprite that was 
      hovered over. 
      @param - minDamage: The lower bound of the damage being applied
      that is used to calculate the actual amount dealt.
      @param - maxDamage: The upper bound of the damage being applied
      that is used to calculate the actual amount dealt.
      @param - isMonster: Used to determine which UI elements to update;
      between party or monster UI components
    **************************************************************/
    public void attackTarget(in Combatable target, int minDamage, int maxDamage)
    {
        int damageDealt = target.applyDamage(minDamage, maxDamage);
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

        DamagePopup.create(target.combatSprite.transform.position, damageDealt.ToString(), Color.red);
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