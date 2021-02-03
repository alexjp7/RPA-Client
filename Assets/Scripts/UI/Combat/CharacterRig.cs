using System.Linq;

namespace Assets.Scripts.UI.Combat
{
    using UnityEngine;
    using System.Collections.Generic;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Components;
    using System;
    using log4net;
    using Assets.Scripts.Entities.Players;

    /// <summary>
    /// string suffixes of each body part, used for sprite lookup.
    /// </summary>
    public readonly struct BodyPart
    {
        public const string BODY = "body";
        public const string HEAD = "head";
        public const string LEFT_ARM = "left_arm";
        public const string RIGHT_ARM = "right_arm";
        public const string LEFT_LEG = "left_leg";
        public const string RIGHT_LEG = "right_leg";
    }

    /// <summary>
    /// Provides the base functionality of multi-component sprites (i.e. those with body parts).
    /// </summary>
    public abstract class CharacterRig : MonoBehaviour
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CharacterRig));

        protected static readonly string PARTS_SUFFIX = ".parts";

        protected static readonly string BASE_RIG_PATH = "textures/sprite_textures/";
        protected static readonly string PLAYER_PATH = BASE_RIG_PATH + "player_textures/";
        protected static readonly string MONSTER_PATH = BASE_RIG_PATH + "monster_textures/";

        /// <summary>
        /// Key-value map between a body part name and its SpriteRenderer.
        /// </summary>
        public Dictionary<string, SpriteRenderer> parts { get; protected set; }


        /// <summary>
        /// Parent object used to animate within local space.
        /// </summary>
        [SerializeField] public Transform parent;

        /// <summary>
        /// Factory-esque method to create the appropraite rigging instance to provide the correct structure
        /// for a character model.
        /// </summary>
        /// <param name="combatantClassName"></param>
        /// <param name="isPlayer"></param>
        /// <returns></returns>
        public static CharacterRig create(Combatant character) 
        {
            if(character.characterRigRef is null)
            {
                ArgumentException ex = new ArgumentException($"Rigging prefab not defined for {character.GetType().Name}.");
                log.Error(ex.Message,ex);
                throw ex;
            }

            Transform spriteTransform = Instantiate(character.characterRigRef, Vector2.zero, Quaternion.identity);
            CharacterRig rig = RiggingFactory.getRig(character, spriteTransform);

            rig.setData(character.GetType().Name, character is Adventurer);

            return rig;
        }

        public CharacterRig()
        {
            parts = new Dictionary<string, SpriteRenderer>();
        }

        protected void Awake()
        {
            parent = gameObject.transform.Find("parent");
        }

        /// <summary>
        /// Sets the sprite for each body part for a Character.
        /// </summary>
        /// <remarks>
        /// The sub-textures which represent each body component within the texture map are in the form [CLASS_NAME].[BODY_PART].
        /// </remarks>
        protected virtual void setData(string className, bool isPlayer)
        {
            if (this is SimpleRig)
            {
                parts[BodyPart.BODY].sprite = AssetLoader.getSprite(className);
            }
            else
            {
                string spriteMapPath = isPlayer ? $"{PLAYER_PATH}{className + PARTS_SUFFIX}" : $"{MONSTER_PATH}{className + PARTS_SUFFIX}";
                AssetLoader.batchLoadAssets(spriteMapPath); //Must occur before loading individual sprites

                // In some cases the prefab will have the sprite components already set 
                // if the prefab does not have the sprites set, we will programmatically  set them 
                foreach (var part in parts.Where(part => part.Value.sprite is null))
                {
                    part.Value.sprite = AssetLoader.getSprite($"{className}.{part.Key}");
                }
            }
        }

        /// <summary>
        /// Sets color of all body parts for the purpose of showing colored hit-indicators.
        /// </summary>
        /// <param name="color"></param>
        public void setColor(Color color)
        {
            foreach (SpriteRenderer component in parts.Values)
            {
                component.color = color;
            }
        }

        /// <summary>
        /// Checks if the incoming x/y coordiantes are within the bounds of the character rig.
        /// </summary>
        /// <param name="position">x/y position to check against e.g. Mouse Position.</param>
        /// <returns></returns>
        public bool contains(Vector2 position)
        {
            foreach (SpriteRenderer component in parts.Values)
            {
                if (component.bounds.Contains(position))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
