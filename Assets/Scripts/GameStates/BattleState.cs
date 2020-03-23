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
#region Imports
using System.Collections.Generic;
using UnityEngine;
using Assets.Scripts.RPA_Player;
using UnityEngine.UI;
using SimpleJSON;
using System.IO;
using Assets.Scripts.Player_Classes;
using System;
using Assets.Scripts.Util;
using System.Linq;
using Assets.Scripts.Monsters;
using Assets.Scripts.Player.Abilities;
using Assets.Scripts.Entities.Components;
using System.Threading;
using UnityEngine.EventSystems;
using UnityEngine.Events;
#endregion

public class BattleState : MonoBehaviour
{
    #region Party Member UI
    //Party textures/text
    [SerializeField]
    public GameObject[] playerPanels;
    [SerializeField]
    public SpriteRenderer[] playerSpirtes;
    [SerializeField]
    public Text[] partyMembers;
    [SerializeField]
    public Text[] maxHealthValues;
    [SerializeField]
    public Text[] currentHealthValues;
    [SerializeField]
    public Image[] partyHpBar;
    [SerializeField]
    public Button[] skillButtons;
    [SerializeField]
    public GameObject[] turnChevrons;
    #endregion

    #region Monster UI
    //Monster textures/text
    [SerializeField]
    public GameObject[] monsterPanels;
    [SerializeField]
    public Text[] monsterNames;
    [SerializeField]
    public SpriteRenderer[] monsterSprites;
    [SerializeField]
    public Image[] monsterHpBars;
    [SerializeField]
    public Text[] textHpValues;
    [SerializeField]
    public Text[] textMaxHealths;
    #endregion

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


    Player clientSidePlayer;
    AdventuringClass clientPlayerClass;

    //@TEST
    //Statically loaded data
    private Texture2D[] classIcons;
    private const string CLASS_DATA_FILE = "assets/resources/data/meta_data.json";

    private void Awake()
    {
        initTestData();  //@Test
        initClassData(); //@Test
        initPlayerData();//@Test

        initMonsterParty();
        initCombatOrder();
        initPlayerUI();
        initSpriteCallbacks();
        takeTurn();
    }
    #region Development/Test Functions
    private void initTestData()
    {
        Game.players = new List<Player>();
        //@Test Data
        Game.players.Add(new Player(6, "Alexjp", 3, true));
        Game.players.Add(new Player(8, "Frictionburn", 0, true));
        Game.players.Add(new Player(11, "Kozza", 2, true));
        Game.players.Add(new Player(4, "Yomamma", 3, true));

        Game.connectedPlayers = Game.players.Count;

        foreach (Player player in Game.players) player.applyClass();
        //Test for healing
        Game.players[0].playerClass.healthProperties.currentHealth -= 30;
        //Test for client side player
        Game.players[0].isClientPlayer = true;
    }

    //@Test Function - will be set to player.assetData in by character creation state
    private void initClassData()
    {
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

    //@Test fucnction - would of been done in character creation 
    private void initPlayerData()
    {
        for (int i = 0; i < Game.players.Count; i++)
        {
            Player player = Game.players[i];
            player.assetData.icon = classIcons[player.adventuringClass];
        }
    }

    #endregion


    /*---------------------------------------------------------------
                       GAME STATE INIATIALISATIONS
     ---------------------------------------------------------------*/
    /***************************************************************
    * Generates the monster party via the monster factory class 
    * Addtionally, this function sets the UI components relevant to
      the monsters loaded from the factory.
    **************************************************************/
    private void initMonsterParty()
    {
        int monsterPartySize = 2;
        MonsterFactory mFactory = new MonsterFactory();
        monsterParty = mFactory.initMonsterParty(monsterPartySize);

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
        int order = 0;
        Game.players = Game.players.OrderBy(player => r.Next()).ToList();

        Game.players.ForEach(player => player.playerClass.combatOrder = order++);
        order = Game.PARTY_LIMIT;
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
    * This function utilises the Combatable component of player
      and mosnter types, which serves as the base type which
      both inherit from.
      
      THE COMBATABLE TYPE
      -------------------
      Combatables have fields relating to combat
      including; turn order, health properties (Damageable), abilities
      and the base logic for applying damage, healing and status 
      effects to these types of objects. The combatable base
      provides a abstract type to allow for generic handling
      of both player and monster types.
    **************************************************************/

    public void takeTurn()
    {
        bool isPlayerTurn = turnCount % 2 == 0;
        Combatable nextCombatant = getCombatant(isPlayerTurn, true);
        Combatable previousCombatant = getCombatant(isPlayerTurn, false);

        togglePlayerTurn(isPlayerTurn && Game.players[nextCombatant.combatOrder].id == clientSidePlayer.id);
        if (turnCount > 0) turnChevrons[previousCombatant.combatOrder].SetActive(false);
        turnChevrons[nextCombatant.combatOrder].SetActive(true);
        currentPlayerName.text = nextCombatant.name;
        turnCount++;

        if (!isPlayerTurn) takeMonsterTurn(in nextCombatant);
    }

    /***************************************************************
    @param - isPlayerTurn : flags whether to get the next
    turn from the player(true) or monster-party(false).
    @param  - isNext : flags whether the next(true) or previous (false)
    combatant is to be returned.
    @return - The combatant data
    for the player/monster that is having its next turn.
    **************************************************************/
    private Combatable getCombatant(bool isPlayerTurn, bool isNext)
    {
        Combatable combatant;
        if (isNext)
        {
            int nextComtatant;
            if (isPlayerTurn)
            {
                nextComtatant = currentPlayer % Game.players.Count;
                if (!Game.players[nextComtatant].playerClass.isAlive()) //Skip over dead players
                    nextComtatant = ++currentPlayer % Game.players.Count;

                combatant = Game.players[nextComtatant].playerClass;
                currentPlayer++;
            }
            else
            {
                nextComtatant = currentMonster % monsterParty.Count;
                if (!monsterParty[nextComtatant].isAlive()) //Skip over dead monsters
                    nextComtatant = ++currentMonster % monsterParty.Count;

                combatant = monsterParty[nextComtatant];
                currentMonster++;
            }
        }
        else
        {   //Get previous combatant
            if (turnCount > 0)
            {
                int previousCombatant;
                if (isPlayerTurn)
                {
                    previousCombatant = (currentMonster - 1) % monsterParty.Count;
                    combatant = monsterParty[previousCombatant];
                }
                else
                {
                    previousCombatant = (currentPlayer - 1) % Game.players.Count;
                    combatant = Game.players[previousCombatant].playerClass;
                }
            }
            else combatant = null;
        }
        return combatant;
    }

    /***************************************************************
    * Called when a monster has its turn.
    * The logic relating to a monster's turn priority is refernced
      throgh the member functions of that monster.

    @param - nextCombatant: the combat properties relating to the
    monster whos turn it currently is.
    **************************************************************/
    private void takeMonsterTurn(in Combatable nextCombatant)
    {
        targets.Clear();

        int target = 0;
        targets.Add(target, Game.players[target].playerClass);
        attackTarget(target, 12, 16, false);
        takeTurn();

    }

    /***************************************************************
    * Sets the visual properties relating to the active/inactive state
      of the players turn

    @param - isPalyerTurn - if true, the ability buttons functionality 
    will become active aswel as setting the icons completley opaque.
    If false, the ability icon's functionality will be disabled
    and setting the buttons to 30% opaque.
    **************************************************************/
    private void togglePlayerTurn(bool isPlayerTurn)
    {
        Color color = skillButtons[0].GetComponent<Image>().color;
        color.a = isPlayerTurn ? 1f : .3f;

        foreach (Button button in skillButtons)
        {
            button.GetComponent<Image>().color = color;
            button.enabled = isPlayerTurn;
        }
    }


    /*---------------------------------------------------------------
                     CALLBACKS/EVENT HANDLERS
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

      ABILITY TARGETING TYPES:
      ------------------------
    * Self-Target: doesn't not provide a variable target selection
      and enfornces the target to be the caster.
    * Single-Target: allows for discretionary targeting of a single
      Combatant. Target highlighting is difered to mouse hover event
      after selecting these types of abilities.
    * Multi-Target: highlights multiple targets and populates the 
      target selection dictionary with either all of the monster
      or player party depending on the meta-type (effect, damage, heal).

    @param - skillIndex: a value between 0-4 indicating which
    ability from the ability bar has been clicked.
    **************************************************************/
    public void onAbilityClicked(int skillIndex)
    {
        targets.Clear();
        selectedSkill = skillIndex;
        Array.ForEach(playerSpirtes, player => player.color = Color.white);
        Array.ForEach(monsterSprites, monster => monster.color = Color.white);

        Ability ability = clientPlayerClass.abilities[selectedSkill];

        switch ((AbilityTypes)ability.typeIds[0])
        {
            //Self Target
            case AbilityTypes.SELF_HEAL:
                isTargetable = false;
                targets.Add(0, clientPlayerClass);
                playerSpirtes[0].color = Color.green;
                break;

            case AbilityTypes.SELF_BUFF:
                isTargetable = false;
                targets.Add(0, clientPlayerClass);
                playerSpirtes[0].color = Color.green;
                break;

            //Single Target - note: Defers targeting
            case AbilityTypes.SINGLE_DAMAGE:
                isTargetable = true;
                isEnemyTargetable = true;
                break;

            case AbilityTypes.SINGLE_DEBUFF:
                isTargetable = true;
                isEnemyTargetable = true;
                break;

            case AbilityTypes.SINGLE_BUFF:
                isTargetable = true;
                isEnemyTargetable = false;
                break;

            case AbilityTypes.SINGLE_HEAL:
                isTargetable = true;
                isEnemyTargetable = false;
                break;

            //Multi Target 
            case AbilityTypes.MULTI_DAMAGE:
                isTargetable = false;
                hasValidTarget = true;
                Array.ForEach(monsterSprites, monster => monster.color = Color.red);
                for (int i = 0; i < monsterParty.Count; i++) targets.Add(i, monsterParty[i]);

                break;
            case AbilityTypes.MULTI_HEAL:
                isTargetable = false;
                hasValidTarget = true;
                Array.ForEach(playerSpirtes, player => player.color = Color.green);
                for (int i = 0; i < Game.players.Count; i++) targets.Add(i, Game.players[i].playerClass);
                break;
            case AbilityTypes.MULTI_BUFF:
                isTargetable = false;
                hasValidTarget = true;
                for (int i = 0; i < Game.players.Count; i++) targets.Add(i, Game.players[i].playerClass);
                Array.ForEach(playerSpirtes, player => player.color = Color.green);

                break;
            case AbilityTypes.MULTI_DEBUFF:
                isTargetable = false;
                hasValidTarget = true;
                Array.ForEach(monsterSprites, monster => monster.color = Color.red);
                for (int i = 0; i < monsterParty.Count; i++) targets.Add(i, monsterParty[i]);

                break;
        }
    }

    //SPRITE HANDLERS
    /***************************************************************
    * Event handler for defered targeting types (single-target 
      abiltiies).
    * Includes the logic for ensuring the ability type's targeting
      type has the desired effect on displaying/highlighting
      the target selection.

      e.g. A single-damage ability was clicked which enables it 
          to be targetable to monsters, the intended effect
          would to allow for mouse over events to highlight
          the monsters active in the battle, additionally mousing 
          over an ally/player should NOT highlight that sprite.
    
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
                        attackTarget(combatant.Key, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max, true);
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

        onSpriteExit(spriteIndex);
        isTargetable = false;
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
        Combatable target = targets[spriteIndex];

        if (target.applyDamage(minDamage, maxDamage))
        {
            if(isMonster) monsterPanels[spriteIndex].SetActive(false);
            else playerPanels[spriteIndex].SetActive(false);
        }
        else
        {
            hpBar.fillAmount = target.getHealthPercent();
            hpValue.text = ((int)target.getCurrentHp()).ToString();
        }

    }

    void Update()
    {
        if (isBattleInProgress)
        {

        }
    }
}