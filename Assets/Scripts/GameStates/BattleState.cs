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
#endregion IMPORTS

public class BattleState : MonoBehaviour
{
    #region PARTY-MEMBER UI
    //Party textures/text
    [SerializeField] private GameObject[] playerPanels;
    [SerializeField] private SpriteRenderer[] playerSpirtes;
    [SerializeField] private Text[] partyMembers;
    [SerializeField] private Text[] maxHealthValues;
    [SerializeField] private Text[] currentHealthValues;
    //Ability UI
    [SerializeField] private Image[] partyHpBar;
    [SerializeField] private Button[] skillButtons;
    #endregion PARTY-MEMBER UI

    #region MONSTER-UI
    //Monster textures/text
    [SerializeField] private GameObject[] monsterPanels;
    [SerializeField] private Text[] monsterNames;
    [SerializeField] private SpriteRenderer[] monsterSprites;
    [SerializeField] private Image[] monsterHpBars;
    [SerializeField] private Text[] textHpValues;
    [SerializeField] private Text[] textMaxHealths;
    #endregion MONSTER-UI

    private Text currentPlayerName;
    private List<Monster> monsterParty;
    private Dictionary<int, Combatable> targets;

    private static int turnCount = 0;
    private int selectedSkill = -1;
    private int currentMonster = 0;
    private int currentPlayer = 0;

    //Game Flags
    private bool isTargetable = false;
    private bool hasValidTarget = false;

    private bool isEnemyTargetable = false;
    private bool isStandardTooltip = false;
    private bool isBattleInProgress = true;
    private bool hasNextCombatant = true;


    Player clientSidePlayer;
    AdventuringClass clientPlayerClass;


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
        //Game.players.Add(new Player(11, "Kozza", 2, true));
        //Game.players.Add(new Player(4, "Yomamma", 3, true));

        Game.connectedPlayers = Game.players.Count;

        foreach (Player player in Game.players) player.applyClass();
        //Test for healing
        //Game.players[0].playerClass.healthProperties.currentHealth -= 30;
        //Test for client side player
        Game.players[0].isClientPlayer = true;
    }

    /***************************************************************
    *  Parses json file for class data and populates icon textures
    **************************************************************/
    private void initClassData()
    {
        const string CLASS_DATA_FILE  =  "assets/resources/data/meta_data.json";
        string classJSON = File.ReadAllText(CLASS_DATA_FILE);

        JSONNode json = JSON.Parse(classJSON);
        JSONArray classes = json["classes"].AsArray;

        int classCount = classes.Count;
        classIcons = new Texture2D[classCount];

        for (int i = 0; i < classCount; i++)
        {
            classIcons[i] = Resources.Load(classes[i]["icon_path"].Value) as Texture2D;
            AdventuringClass.setDataPaths(classes[i]["id"].AsInt, classes[i]["ability_path"].Value);
        }

        AdventuringClass.setClassSprites(Game.players);
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
        int monsterPartySize = 2;
        MonsterFactory mFactory = new MonsterFactory();
        monsterParty = mFactory.createMonsterParty(monsterPartySize);

        for (int i = 0; i < monsterPartySize; i++)
        {
            monsterPanels[i].SetActive(true);
            monsterNames[i].text = monsterParty[i].name;
            monsterHpBars[i].fillAmount = monsterParty[i].getHealthPercent();
            textHpValues[i].text = monsterParty[i].healthProperties.currentHealth.ToString();
            textMaxHealths[i].text = "/" + monsterParty[i].healthProperties.maxHealth.ToString();
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
        targets = new Dictionary<int, Combatable>();
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
        clientSidePlayer = Game.players.Find(player => player.isClientPlayer);
        clientPlayerClass = clientSidePlayer.playerClass;
        currentPlayerName = GameObject.Find("text_player_turn").GetComponent<Text>();

        for (int i = 0; i < Game.players.Count; i++)
        {
            Player player = Game.players[i];
            //Party names and spirte
            playerSpirtes[i].sprite = AdventuringClass.getClassSprite(player.adventuringClass);
            partyMembers[i].text = Game.players[i].name;
            //Setting HP values
            partyHpBar[i].fillAmount = player.playerClass.getHealthPercent();
            currentHealthValues[i].text = " " + ((int)player.playerClass.getCurrentHp()).ToString();
            maxHealthValues[i].text = "/" + ((int)player.playerClass.getMaxHp()).ToString();
            playerPanels[i].SetActive(true);
        }

        //Load client side players abilities
        try
        {
            clientPlayerClass.loadAbilities(AdventuringClass.getAbilityPath(clientPlayerClass.classId), true);
            for (int i = 0; i < AdventuringClass.ABILITY_LIMIT; i++)
            {
                if (i < clientPlayerClass.abilities.Count)
                {
                    skillButtons[i].GetComponent<Image>().sprite = clientPlayerClass.abilities[i].assetData.model;
                }
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
        for (int i = 0; i < Game.players.Count; i++)
        {
            int j = i;
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onSpriteEnter(j));
            mouseExitEvent.callback.AddListener(evt => onSpriteExit(j));
            mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(j));

            playerPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            playerPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            playerPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        //Initialize Party Monster Sprite Events
        for (int i = 0; i < monsterParty.Count; i++)
        {
            int j = i + Game.players.Count;
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onSpriteEnter(j));
            mouseExitEvent.callback.AddListener(evt => onSpriteExit(j));
            mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(j));

            monsterPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            monsterPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            monsterPanels[i].GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
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

        Debug.Log(playerPanels[0].transform.position.x + "  " + playerPanels[0].transform.position.y);

        TurnChevron.setPosition(isPlayerTurn ? playerPanels[nextCombatant].transform.position
                                             : monsterPanels[nextCombatant].transform.position);

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
                if(++deathCounter == Game.players.Count) break;
            }
            currentPlayer++;
        }
        else
        {
            nextComtatant = currentMonster % monsterParty.Count;
            while (!monsterParty[nextComtatant].isAlive()) //Skip over dead monsters
            {
                nextComtatant = ++currentMonster % monsterParty.Count;

                if (++deathCounter ==  monsterParty.Count) break;
            }
            currentMonster++;
        }
        return nextComtatant;
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
        for(int i = 0; i < potentialTargets.Length; i++)
        {
            if(Game.players[potentialTargets[i]].playerClass.isAlive())
            {
                target = potentialTargets[i];
                break;
            }
        }
     
        if(target == -1)
        {
            hasNextCombatant = false;
        }
        else
        {
            targets.Add(target, Game.players[target].playerClass);
            attackTarget(target, 4, 5, false);
        }
    }

    /***************************************************************
    * Sets the visual properties relating to the active/inactive state
      of the players turn including updating ability cooldowns.

    @param - isPalyerTurn - if true, the ability buttons functionality 
    will become active aswel as setting the icons completley opaque.
    If false, the ability icon's functionality will be disabled
    and setting the buttons to 30% opaque.
    **************************************************************/
    private void togglePlayerTurn(bool isPlayerTurn)
    {
        if(isPlayerTurn) clientPlayerClass.updateAbilityCooldowns(turnCount - (Game.players.Count + monsterParty.Count()));

        for (int i = 0; i < clientPlayerClass.abilities.Count; i++)
        {
            Button button = skillButtons[i];
            Color color = button.GetComponent<Image>().color;
            Ability ability = clientPlayerClass.abilities[i];
            if (isPlayerTurn)
            {
                if (ability.isOnCooldown)
                {
                    color.a = .3f;
                    button.enabled = false;
                    skillButtons[i].GetComponentInChildren<Text>().text = (ability.cooldownTracker).ToString();
                }
                else
                {
                    color.a = 1f;
                    button.enabled = true;
                    skillButtons[i].GetComponentInChildren<Text>().text = "";
                    ability.lastTurnUsed = -1;
                }
            }
            else
            {
                color.a = .3f;
                button.enabled = false;
                if (ability.isOnCooldown)
                {
                    skillButtons[i].GetComponentInChildren<Text>().text = (ability.cooldownTracker).ToString();
                }
            }

            button.GetComponent<Image>().color = color;
        }
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
    public void onSkillHoverEnter(int skillIndex)
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
    public void skillSkillHoverExit()
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
        resetTargets();
        selectedSkill = skillIndex;
        Ability ability = clientPlayerClass.abilities[selectedSkill];

        switch ((AbilityTypes)ability.typeIds[0])
        {
            //Self Target
            case AbilityTypes.SELF_HEAL: case AbilityTypes.SELF_BUFF:
                isTargetable = false;
                targets.Add(0, clientPlayerClass);
                playerSpirtes[0].color = Color.green;
                break;

            //Single Target - note: Defers targeting
            case AbilityTypes.SINGLE_DAMAGE: case AbilityTypes.SINGLE_DEBUFF:
                isTargetable = true;
                isEnemyTargetable = true;
                break;

            case AbilityTypes.SINGLE_BUFF: case AbilityTypes.SINGLE_HEAL:
                isTargetable = true;
                isEnemyTargetable = false;
                break;

            //Multi Target 
            case AbilityTypes.MULTI_DAMAGE: case AbilityTypes.MULTI_DEBUFF:
                isTargetable = false;
                hasValidTarget = true;
                Array.ForEach(monsterSprites, monster => monster.color = Color.red);
                for (int i = 0; i < monsterParty.Count; i++) targets.Add(i, monsterParty[i]);

                break;
            case AbilityTypes.MULTI_HEAL: case AbilityTypes.MULTI_BUFF:
                isTargetable = false;
                hasValidTarget = true;
                Array.ForEach(playerSpirtes, player => player.color = Color.green);
                for (int i = 0; i < Game.players.Count; i++) targets.Add(i, Game.players[i].playerClass);
                break;
        }
    }

    private void resetTargets()
    {
        targets.Clear();
        Array.ForEach(playerSpirtes, player => player.color = Color.white);
        Array.ForEach(monsterSprites, monster => monster.color = Color.white);
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
    public void onSpriteEnter(int spriteIndex)
    {
        if (!isTargetable) return;
        targets.Clear();
        hasValidTarget = false;

        //Party Member hovered over
        if (!hasHoveredOveryEnemy(spriteIndex))
        {
            if (!isEnemyTargetable)
            {
                playerSpirtes[spriteIndex].color = Color.green;
                hasValidTarget = true;
                targets.Add(spriteIndex, Game.players[spriteIndex].playerClass);
            }
        }//Enemy sprite hovered over
        else if (hasHoveredOveryEnemy(spriteIndex))
        {
            if (isEnemyTargetable)
            {
                int monsterSpriteIndex = spriteIndex - (Game.players.Count);
                monsterSprites[monsterSpriteIndex].color = Color.red;
                hasValidTarget = true;
                targets.Add(monsterSpriteIndex, monsterParty[monsterSpriteIndex]);
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
    public void onSpriteExit(int spriteIndex)
    {
        if (!isTargetable) return;
        targets.Clear();
        hasValidTarget = false;

        if (!hasHoveredOveryEnemy(spriteIndex))
        {
            if (!isEnemyTargetable)
            {
                playerSpirtes[spriteIndex].color = Color.white;
                hasValidTarget = false;
            }
        }
        else if (hasHoveredOveryEnemy(spriteIndex))
        {
            if (isEnemyTargetable)
            {
                int monsterSpriteIndex = spriteIndex - (Game.players.Count);
                monsterSprites[monsterSpriteIndex].color = Color.white;
                hasValidTarget = false;
                targets.Add(monsterSpriteIndex, monsterParty[monsterSpriteIndex]);
            }
        }
    }

    /***************************************************************
    * Helper function which determines whether the mouse has hovered
      over an enemy or player sprite.
    
    * the outcome of this function is determine if the index
      passed in is within the bounds of the party or monster size.

      @param - spriteIndex: a value between 0-players.count() OR
      0-monsterParty.count() which represents the sprite that was 
      hovered over. 

      @return - true: The mouse has hovered over an enemy.
              - false: The mouse has hovered over an ally.
    **************************************************************/
    private bool hasHoveredOveryEnemy(int spriteIndex)
    {
        bool isEnemyTarget = false;
        if (spriteIndex >= 0 && spriteIndex < Game.players.Count) isEnemyTarget = false;
        else if (spriteIndex >= Game.players.Count && spriteIndex < (monsterParty.Count + Game.players.Count)) isEnemyTarget = true;
        return isEnemyTarget;
    }

    /***************************************************************
    * Event handler for when a sprite is clicked after selecting
      a valid target with valid ability.

    * Applies the abilities effects to the target. 
     
      @param - spriteIndex: a value between 0-players.count() OR
      0-monsterParty.count() which represents the sprite that was 
      hovered over. 
    **************************************************************/
    public void onSpriteClicked(int spriteIndex)
    {
        if (!hasValidTarget) return;
        if (selectedSkill == -1) return;

        Ability abilityUsed = clientPlayerClass.abilities[selectedSkill];

        if (hasHoveredOveryEnemy(spriteIndex))
        {
            foreach (var combatant in targets)
            {
                foreach (var abilityType in abilityUsed.typeIds)
                {
                    MetaTypes metaType = AbilityFactory.getMetaType(abilityType);

                    if (metaType == MetaTypes.DAMAGE)
                    {
                        attackTarget(combatant.Key, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max * 2, true);
                    }
                    else if (metaType == MetaTypes.EFFECT)
                    {

                    }
                }
            }
        }
        else
        {
            foreach (var combatant in targets)
            {
                foreach (var abilityType in abilityUsed.typeIds)
                {
                    MetaTypes metaType = AbilityFactory.getMetaType(abilityType);

                    if (metaType == MetaTypes.HEALING)
                    {
                        healTarget(combatant.Key, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max, false);
                    }
                    else if (metaType == MetaTypes.EFFECT)
                    {

                    }
                }
            }
        }
        abilityUsed.lastTurnUsed = turnCount;
        onSpriteExit(spriteIndex);

        if(Game.players.Count > 1 ) clientPlayerClass.abilities[selectedSkill].updateCooldown(turnCount - (Game.players.Count + monsterParty.Count()));

        resetTargets();
        takeTurn(); 
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
    public void healTarget(int spriteIndex, int minHealing, int maxHealing, bool isMonster)
    {
        Image hpBar = isMonster ? monsterHpBars[spriteIndex] : partyHpBar[spriteIndex];
        Text hpValue = isMonster ? textHpValues[spriteIndex] : currentHealthValues[spriteIndex];
        Combatable target = targets[spriteIndex];

        target.applyHealing((int)minHealing, (int)maxHealing);
        hpBar.fillAmount = target.getHealthPercent();
        hpValue.text = ((int)target.getCurrentHp()).ToString();
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
    public void attackTarget(int spriteIndex, int minDamage, int maxDamage, bool isMonster)
    {
        Image hpBar = isMonster ? monsterHpBars[spriteIndex] : partyHpBar[spriteIndex];
        Text hpValue = isMonster ? textHpValues[spriteIndex] : currentHealthValues[spriteIndex];
        GameObject spritePanel = isMonster ? monsterPanels[spriteIndex] : playerPanels[spriteIndex];
        Combatable target = targets[spriteIndex];
        int damageDealt = target.applyDamage(minDamage, maxDamage);

        DamagePopup.create(spritePanel.transform.position, damageDealt);

        if (!target.isAlive())
        {
            spritePanel.SetActive(false);
            checkBattleState();
        }
        else
        {
            hpBar.fillAmount = target.getHealthPercent();
            hpValue.text = ((int)target.getCurrentHp()).ToString();
        }
    }

    private void checkBattleState()
    {
        bool isInProgress = false;

        foreach(Player player in Game.players)
        {
            if (player.playerClass.isAlive())
            {
                isInProgress = true;
                break;
            }
        }

        if(isInProgress)
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

    void Update()
    {
        if (isBattleInProgress)
        {

        }
    }
}