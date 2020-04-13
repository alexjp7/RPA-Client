/*---------------------------------------------------------------
                        COMBAT-SPRITE
 ---------------------------------------------------------------*/
/***************************************************************
* Combat Sprite contains all the UI components relevant
  for displaying player and monster sprites, hp and nameplates.

 * This script is attached to the CombaSprite prefab.
**************************************************************/
using Assets.Scripts.Entities.Components;
using Assets.Scripts.Util;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class CombatSprite : MonoBehaviour
    {
        public Combatable combatantRef;
        public SpriteRenderer sprite { get; private set;}
        public GameObject buffBar { get; private set;}
        public Text displayName { get; private set; }
        public Text maxHealthValue { get; private set; }
        public Text currentHealthValue { get; private set; }
        public Image healthBar { get; set; }
        public bool isMonster { get; private set; }
        
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
            buffBar = gameObject.transform.Find("buff_bar").GetComponent<GameObject>();
        }


        /***************************************************************
        * Sets the values of each GameObject that makes up the CombatSprite.
        
        @param - combatant: The monster or player that is to be displayed
        **************************************************************/
        private void setData(in Combatable combatant)
        {
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

            mouseEnterEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onSpriteEnter(combatantRef));
            mouseExitEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onSpriteExit(combatantRef));
            mouseClickeEvent.callback.AddListener(evt => ViewController.INSTANCE.battleState.onSpriteClicked(combatantRef));

            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseEnterEvent);
            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseExitEvent);
            gameObject.GetComponent<EventTrigger>().triggers.Add(mouseClickeEvent);
        }

        void Update()
        {
            Vector2 position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            bool doesContain = sprite.bounds.Contains(position);
            if(doesContain)
            {
                ViewController.INSTANCE.battleState.onSpriteEnter(combatantRef);
            }

        }
    }

}  