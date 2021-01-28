namespace Assets.Scripts.UI.Combat
{
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;

    using Assets.Scripts.Entities.Combat;
    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.GameStates;
    using Assets.Scripts.RPA_Game;
    using Assets.Scripts.RPA_Messages;
    using Assets.Scripts.UI.Common;
    using Assets.Scripts.Util;

    /// <summary>
    /// Combat Sprite contains all the UI components relevant for displaying player and monster sprites, hp and nameplates.
    /// This script is attached to the CombaSprite.
    /// </summary>
    public class CombatSprite : MonoBehaviour
    {
        public int popupCount { get; set; } // Have independant tracking of text-popups to make it more readable
        public static bool hasValidTarget;
        public Combatant combatantRef;
        public SpriteRenderer sprite { get; private set; }
        public Text displayName { get; private set; }
        public Text maxHealthValue { get; private set; }
        public Text currentHealthValue { get; private set; }
        public Image healthBar { get; set; }
        public bool isMonster { get; private set; }

        public BuffBar buffBar { get; private set; }

        private static CombatController combatController => StateManager.battleState.combatController;

        /// <summary>
        /// Instantiates and returns the a CombatSpriute instance.
        /// </summary>
        /// <param name="combatant">The monster or player that is to be displayed</param>
        /// <returns>A combat sprite reflecting that of the passed in combatant.</returns>
        public static CombatSprite create(in Combatant combatant)
        {
            Transform spriteTransform = Instantiate(GameAssets.INSTANCE.combatSpritePrefab, Vector2.zero, Quaternion.identity);
            CombatSprite cbSprite = spriteTransform.GetComponent<CombatSprite>();
            cbSprite.setData(combatant);
            return cbSprite;
        }

        /// <summary>
        /// Initialize Components
        /// </summary>
        void Awake()
        {
            displayName = gameObject.transform.Find("text_player_name").GetComponent<Text>();
            sprite = gameObject.transform.Find("sprite_player").GetComponent<SpriteRenderer>();
            healthBar = gameObject.transform.Find("image_hp_bar").GetComponent<Image>();
            maxHealthValue = gameObject.transform.Find("text_max_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
            currentHealthValue = gameObject.transform.Find("text_current_health").GetComponent<Text>();
            buffBar = gameObject.transform.Find("buff_bar").GetComponent<BuffBar>();
        }


        /// <summary>
        /// Sets the values of each GameObject that makes up the CombatSprite.
        /// </summary>
        /// <param name="combatant">The monster or player that is to be displayed</param>
        private void setData(in Combatant combatant)
        {
            popupCount = 0;
            combatantRef = combatant;
            displayName.text = combatant.name;
            sprite.sprite = combatant.assetData.sprite;
            currentHealthValue.text = ((int)combatant.healthProperties.currentHealth).ToString();
            maxHealthValue.text = "/" + (int)combatant.healthProperties.maxHealth;
            setEventHandlers();

            if (combatant.type == CombatantType.PLAYER) isMonster = false;
            else if (combatant.type == CombatantType.MONSTER) isMonster = true;
        }

        /// <summary>
        /// Creates the event handlers for hovering and click events for the player party and monster party sprites.
        /// </summary>
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

        /// <summary>
        /// Updates the buffbar based on the current state of a combatants condition list.
        /// </summary>
        public void updateConditions()
        {
            buffBar.updateConditions(combatantRef.conditions);
        }

        /// <summary>
        /// Event handler for defered targeting types (single-target abiltiies).
        /// Includes the logic for ensuring the ability type's targeting type has the desired effect on displaying/highlighting the target selection.
        /// </summary>
        /// <param name="combatant">The combatant that is being hovered over</param>
        public void onSpriteEnter(in Combatant combatant)
        {
            if (!combatController.isClientPlayerTurn) return;
            //Check ability selected
            int abilityIndex = AbilityButton.selectedAbilityIndex;
            if (abilityIndex == -1) return;
            //Check if targeting type applies to manual target selection
            Ability abilitySelected = Game.clientSidePlayer.playerClass.abilities[abilityIndex];
            if (abilitySelected.targetingType == TargetingType.AUTO) return;


            combatController.targets.Clear();

            //Party Member hovered over
            if (!combatant.combatSprite.isMonster)
            {
                if (abilitySelected.targetingType != TargetingType.ENEMY)
                {
                    combatant.combatSprite.sprite.color = Color.green;
                    combatController.hasValidTarget = true;
                    combatController.targets.Add(combatant);
                }

            }//Enemy sprite hovered over
            else
            {
                if (abilitySelected.targetingType != TargetingType.ALLIED)
                {
                    combatant.combatSprite.sprite.color = Color.red;
                    combatController.hasValidTarget = true;
                    combatController.targets.Add(combatant);
                }

            }
        }

        /// <summary>
        /// Provides the opposite functionality to onSpriteEnter(),while having selected
        /// a defered targeting type the mouse exit event will set the sprite tint back to the sprite's  original color.
        /// </summary>
        /// <param name="combatant">The combatant that is being moved away from</param>
        public void onSpriteExit(in Combatant combatant)
        {
            if (!combatController.isClientPlayerTurn) return;
            //Check ability selected
            int abilityIndex = AbilityButton.selectedAbilityIndex;
            if (abilityIndex == -1) return;
            //Check if targeting type applies to manual target selection
            if (TargetingType.AUTO == combatController.clientAdventurer.abilities[abilityIndex].targetingType)
                return;

            Ability abilitySelected = combatController.clientAdventurer.abilities[abilityIndex];
            combatController.targets.Clear();
            combatController.hasValidTarget = false;

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

        /// <summary>
        /// Event handler for when a sprite is clicked after selecting a valid target with valid ability. Applies the abilities effects to the target. 
        /// </summary>
        /// <param name="combatant">The combatant that has been selected through mouse click</param>
        public void onSpriteClicked(in Combatant combatant)
        {
            if (!combatController.isClientPlayerTurn)
            {
                return;
            }
            if (AbilityButton.selectedAbilityIndex == -1)
            {
                return;
            }

            Ability abilityUsed = combatController.clientAdventurer.abilities[AbilityButton.selectedAbilityIndex];
            //If A monster is hoeverd over with an ally target ability 
            if (isMonster && abilityUsed.targetingType == TargetingType.ALLIED)
            {
                return;
            }

            //If A PLAYER is hoeverd over with an enemy target ability
            if (!isMonster && abilityUsed.targetingType == TargetingType.ENEMY)
            {
                return;
            }

            //Auto target abilities automatically allow for valid targets
            if (!combatController.hasValidTarget)
            {
                if (abilityUsed.targetingType != TargetingType.AUTO)
                    return;
            }

            //Set to allow server/client distribution of what ability was used
            combatController.lastAbilityUsed = abilityUsed;

            //Process Ability application to target/s
            foreach (var target in combatController.targets)
            {
                foreach (int abilityType in abilityUsed.typeIds)
                {
                    MetaType metaType = AbilityUtils.getMetaType(abilityType);
                    if (target.combatSprite.isMonster)
                    {
                        if (metaType == MetaType.DAMAGE)
                        {
                            combatController.attackTarget(target, combatController.clientAdventurer, abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                        }
                    }
                    else
                    {
                        if (metaType == MetaType.HEALING)
                        {
                            target.heal(abilityUsed.abilityStrength.min, abilityUsed.abilityStrength.max);
                        }
                    }

                    if (metaType == MetaType.EFFECT)
                    {
                        target.applyEffect(abilityUsed.statusEffect, abilityUsed.conditionStrength.potency, abilityUsed.conditionStrength.turnsApplied);
                        combatController.applyAfterEffect(ref abilityUsed); //For client side caster of special case abilities
                    }
                }
            }

            //Update Cooldown / Targets 
            abilityUsed.setLastTurnUsed(combatController.turnCount); //Flagging whether cooldown
            abilityUsed.cooldownTracker++;
            StateManager.battleState.setCooldownUI(AbilityButton.selectedAbilityIndex);
            onSpriteExit(in combatant);

            if (!Game.isSinglePlayer)
            {
                //Send action performed to server, along with turn progression message
                Task.Run(() =>
                {
                    Message.send(new BattleMessage(BattleInstruction.TURN_ACTION));
                    Message.send(new BattleMessage(BattleInstruction.TURN_PROGRESSED));
                    combatController.resetTargets();
                    combatController.takeTurn();
                });

            }
            else
            {
                combatController.resetTargets();
                combatController.takeTurn();
            }
            //Perform turn progression client side

        }

        void Update()
        {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool doesContain = sprite.bounds.Contains(position);
            if (doesContain)
            {
                onSpriteEnter(combatantRef);
            }
        }
    }

}