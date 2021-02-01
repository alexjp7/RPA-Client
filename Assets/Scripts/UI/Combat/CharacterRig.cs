namespace Assets.Scripts.UI.Combat
{
    using UnityEngine;
    using System.Collections.Generic;

    using Assets.Scripts.Util;
    using Assets.Scripts.Entities.Components;
    using System;

    /// <summary>
    /// Provides the base functionality of multi-component sprites (i.e. those with body parts).
    /// </summary>
    public abstract class CharacterRig : MonoBehaviour
    {
        protected static readonly string PARTS_SUFFIX = "parts";

        protected static readonly string BASE_RIG_PATH = "textures/sprite_textures/";
        protected static readonly string PLAYER_PATH = BASE_RIG_PATH + "player_textures/";
        protected static readonly string MONSTER_PATH = BASE_RIG_PATH + "monster_textures/";

        /// <summary>
        /// Key-value map between a body part name and its SpriteRenderer.
        /// </summary>
        public Dictionary<string, SpriteRenderer> parts { get; protected set; }

        /// <summary>
        /// Used to suffix asset loading, whereby each part will begin with the class name of the character being loaded.
        /// </summary>
        protected string characterClassName;

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
            bool isPlayer = character.type == CombatantType.PLAYER;
            CharacterRig rig = null;

            Transform spriteTransform = Instantiate(character.characterRigRef, Vector2.zero, Quaternion.identity);
            if (isPlayer)
            {
                 rig = spriteTransform.GetComponent<HumaniodRig>();
            }

            rig.setData(character.GetType().Name, isPlayer);
            return rig;
        }

        public CharacterRig()
        {
            parts = new Dictionary<string, SpriteRenderer>();
        }

        protected virtual void Awake()
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
            string spriteMapPath = null;
            characterClassName = className;

            if (isPlayer)
            {
                spriteMapPath = $"{PLAYER_PATH}{characterClassName + PARTS_SUFFIX}";
            }
            else
            {
                spriteMapPath = $"{MONSTER_PATH}{characterClassName + PARTS_SUFFIX}";
            }

            //Must occur before loading individual sprites
            AssetLoader.batchLoadAssets(spriteMapPath);

            foreach (var part in parts)
            {
                part.Value.sprite = AssetLoader.getSprite($"{characterClassName}.{part.Key}");
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
