/*---------------------------------------------------------------
                        COMBAT-SPRITE
 ---------------------------------------------------------------*/
/***************************************************************
* Combat Sprite contains all the UI components relevant
  for displaying player and monster sprites, hp and nameplates.

 * This script is attached to the CombaSprite prefab.
**************************************************************/
using Assets.Scripts.Entities.Abilities;
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Entities.Players;
using Assets.Scripts.GameStates;
using Assets.Scripts.RPA_Game;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CombatSprite : MonoBehaviour
    {
        public int popupCount { get; set; } // Have independant tracking of text-popups to make it more readable
        public static bool hasValidTarget;
        public Combatable combatantRef;
        public SpriteRenderer sprite { get; private set;}
        public Transform buffBar { get; private set;}
        public Text displayName { get; private set; }
        public Text maxHealthValue { get; private set; }
        public Text currentHealthValue { get; private set; }
        public Image healthBar { get; set; }
        public bool isMonster { get; private set; }


        private static TurnController turnController = TurnController.INSTANCE;
        
        /***************************************************************
        * instantiates and returns the a CombatSpriute instance.
        
        @param - position: Set to Vector2.zero.
        @param - combatant: The monster or player that is to be displayed

        @return - A combat sprite reflecting that of the passed in
        combatant.
        **************************************************************/
        public static CombatSprite create(in Combatable combatant)
        {
            Transform spriteTransform = Instantiate(GameAssets.INSTANCE.combatSpritePrefab, Vector2.zero, Quaternion.identity);
            CombatSprite cbSprite = spriteTransform.GetComponent<CombatSprite>();
            cbSprite.setData(combatant);
            return cbSprite;
        }

        //Initialize Components
        void Awake()
        {
            displayName = gameObject.transform.Find("text_player_name").GetComponent<Text>();
            sprite = gameObject.transform.Find("sprite_player").GetComponent<SpriteRenderer>();
            healthBar = gameObject.transform.Find("image_hp_bar").GetComponent<Image>();
            maxHealthValue = gameObject.transform.Find("text_max_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
            buffBar = gameObject.transform.Find("buff_bar");
        }


        /***************************************************************
        * Sets the values of each GameObject that makes up the CombatSprite.
        
        @param - combatant: The monster or player that is to be displayed
        **************************************************************/
        private void setData(in Combatable combatant)
        {
            popupCount = 0;
            combatantRef = combatant;
            displayName.text = combatant.name;
            sprite.sprite =  combatant.assetData.sprite;
            currentHealthValue.text =  ( (int) combatant.healthProperties.currentHealth).ToString();
            maxHealthValue.text = "/" +  (int )combatant.healthProperties.maxHealth;
            setEventHandlers();

            if (combatant.type == CombatantType.PLAYER) isMonster = false;
            else if (combatant.type == CombatantType.MONSTER) isMonster = true;
        }


        /***************************************************************
        * Creates the event handlers for hovering and click events
          for the player party and monster party sprites.

        @param - combatant: The monster or player that the events
        are being initialised for.
        **************************************************************/
        private void setEventHandlers()
        {
            EventTrigger.Entry mouseEnterEvent;
            EventTrigger.Entry mouseExitEvent;
            EventTrigger.Entry mouseClickeEvent;

            //Initialize Party Sprite Events
            mouseEnterEvent = new EventTrigger.Entry();
            mouseExitEvent = new EventTrigger.Entry();
            mouseClickeEvent = new EventTrigger.Entry();

            mouseEnterEvent.eventID = EventTriggerType.PointerEnter;
            mouseExitEvent.eventID = EventTriggerType.PointerExit;
            mouseClickeEvent.eventID = EventTriggerType.PointerClick;

            mouseEnterEvent.callback.AddListener(evt => onSpriteEnter(combatantRef));
            mouseExitEvent.callback.AddListener(evt => onSpriteExit(combatantRef));
            mouseClickeEvent.callback.AddListener(evt => onSpriteClicked(combatantRef));

            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        /***************************************************************
                            CONDITION-BAR HANDLERS
        **************************************************************/
        public void updateConditions()
        {
            int childs = buffBar.transform.childCount;
            for (var i = childs - 1; i >= 0; i--)
            {
                Destroy(buffBar.GetChild(i).gameObject);
            }

            if (combatantRef.conditions.Count > 0)
            {
                foreach(var condition in combatantRef.conditions)
                {
                    string conditionName = ((StatusEffect)(condition.Key)).ToString();
                    GameObject newCondition = new GameObject(conditionName);
                    Image conditionIcon = newCondition.AddComponent<Image>();
                    conditionIcon.sprite = AssetLoader.getSprite(conditionName, null, true);
                    newCondition.gameObject.transform.SetParent(buffBar);
                }

            }
        }

        /***************************************************************
                            SPRITE HANDLERS
        ***************************************************************
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
            if (!turnController.isClientPlayerTurn) return;
            //Check ability selected
            int abilityIndex = AbilityButton.selectedAbilityIndex;
            if (abilityIndex == -1) return;
            //Check if targeting type applies to manual target selection
            Ability abilitySelected = Game.clientSidePlayer.playerClass.abilities[abilityIndex];
            if (abilitySelected.targetingType == TargetingType.AUTO) return;


            turnController.targets.Clear();

            //Party Member hovered over
            if (!combatant.combatSprite.isMonster)
            {
                if (abilitySelected.targetingType != TargetingType.ENEMY)
                {
                    combatant.combatSprite.sprite.color = Color.green;
                    turnController.hasValidTarget = true;
                    turnController.targets.Add(combatant);
                }

            }//Enemy sprite hovered over
            else
            {
                if (abilitySelected.targetingType != TargetingType.ALLIED)
                {
                    combatant.combatSprite.sprite.color = Color.red;
                    turnController.hasValidTarget = true;
                    turnController.targets.Add(combatant);
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
            if (!turnController.isClientPlayerTurn) return;
            //Check ability selected
            int abilityIndex = AbilityButton.selectedAbilityIndex;
            if (abilityIndex == -1) return;
            //Check if targeting type applies to manual target selection
            if (TargetingType.AUTO  == turnController.clientAdventurer.abilities[abilityIndex].targetingType)
                return;

            Ability abilitySelected = turnController.clientAdventurer.abilities[abilityIndex];
            turnController.targets.Clear();
            turnController.hasValidTarget = false;

            //Party Member hovered over
            if (!combatant.combatSprite.isMonster)
            {
                if (TargetingType.ENEMY != abilitySelected.targetingType)
                {
                    combatant.combatSprite.sprite.color = Color.white;
                }
            }//Enemy sprite hovered over
            else
            {
                if (TargetingType.ALLIED != abilitySelected.targetingType)
                {
                    combatant.combatSprite.sprite.color = Color.white;
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
            if (!turnController.isClientPlayerTurn) return;
            if (AbilityButton.selectedAbilityIndex == -1) return;

            Ability abilityUsed = turnController.clientAdventurer.abilities[AbilityButton.selectedAbilityIndex];
            //If A monster is hoeverd over with an ally target ability 
            if (isMonster && abilityUsed.targetingType == TargetingType.ALLIED) return;
            
            //If A PLAYER is hoeverd over with an enemy target ability
            if (!isMonster  && abilityUsed.targetingType == TargetingType.ENEMY) return;

            //Auto target abilities automatically allow for valid targets
            if (!turnController.hasValidTarget)
            {
                 if(abilityUsed.targetingType != TargetingType.AUTO)
                    return;
            }

            //Process Ability application to target/s
            foreach (var target in turnController.targets)
            {
                foreach (int abilityType in abilityUsed.typeIds)
                {
                    MetaTypes metaType = AbilityFactory.getMetaType(abilityType);
                    if (target.combatSprite.isMonster)
                    {
                        if (metaType == MetaTypes.DAMAGE)
                        {
                            StateManager.battleState.attackTarget(target, turnController.clientAdventurer, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                        }
                    }
                    else
                    {
                        if (metaType == MetaTypes.HEALING)
                        {
                            StateManager.battleState.healTarget(target, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                        }
                    }

                    if (metaType == MetaTypes.EFFECT)
                    {
                        StateManager.battleState.affectTarget(target, abilityUsed.statusEffect, abilityUsed.conditionStrength.potency, abilityUsed.conditionStrength.turnsApplied);
                        StateManager.battleState.applyAfterEffect(ref abilityUsed); //For client side caster of special case abilities
                    }
                }
            }

            //Update Cooldown / Targets 
            abilityUsed.setLastTurnUsed(TurnController.turnCount); //Flagging whether cooldown
            abilityUsed.cooldownTracker++;
            StateManager.battleState.setCooldownUI(AbilityButton.selectedAbilityIndex);
            onSpriteExit(in combatant);
            turnController.resetTargets();
            turnController.takeTurn();
        }

        void Update()
        {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool doesContain = sprite.bounds.Contains(position);
            if(doesContain)
            {
                onSpriteEnter(combatantRef);
            }
        }
    }

}  