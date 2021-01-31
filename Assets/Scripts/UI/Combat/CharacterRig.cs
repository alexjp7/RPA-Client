using Assets.Scripts.Util;
using UnityEngine;

namespace Assets.Scripts.UI.Combat
{
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
    /// Body components for a combatant's skeleton. 
    /// Contains the individual body parts that is rigged up into a character model.
    /// </summary>
    public class CharacterRig : MonoBehaviour
    {
        private static readonly string PARTS_SUFFIX = "parts";

        private static readonly string BASE_RIG_PATH = "textures/sprite_textures/";
        private static readonly string PLAYER_PATH = BASE_RIG_PATH + "player_textures/";
        private static readonly string MONSTER_PATH = BASE_RIG_PATH + "monster_textures/";

        // Body Parts
        [SerializeField] public SpriteRenderer body;
        [SerializeField] public SpriteRenderer head;
        [SerializeField] public SpriteRenderer leftArm;
        [SerializeField] public SpriteRenderer leftLeg;
        [SerializeField] public SpriteRenderer rightLeg;
        [SerializeField] public SpriteRenderer rightArm;

        // Parent object used to animate within local space.
        [SerializeField] public Transform parent;

        public static CharacterRig create(string combatantClassName, bool isPlayer)
        {
            Transform spriteTransform = Instantiate(GameAssets.INSTANCE.characterRigPrefab, Vector2.zero, Quaternion.identity);
            CharacterRig rig = spriteTransform.GetComponent<CharacterRig>();
            rig.setData(combatantClassName, isPlayer);
            return rig;
        }

        /// <summary>
        /// Loads the rigged character model with each of its body parts. Rigged character models 
        /// </summary>
        /// <remarks>
        /// The sub-textures which represent each body component within the texture map are in the form [CLASS_NAME].[BODY_PART].
        /// </remarks>
        /// <param name="combatantClassName"></param>
        /// <param name="isPlayer"></param>
        public void setData(string combatantClassName, bool isPlayer)
        {
            string spriteMapPath = null;

            if (isPlayer)
            {
                spriteMapPath = $"{PLAYER_PATH}{combatantClassName + PARTS_SUFFIX}";
            }
            else
            {
                spriteMapPath = $"{MONSTER_PATH}{combatantClassName + PARTS_SUFFIX}";
            }

            AssetLoader.batchLoadAssets(spriteMapPath);

            head.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.HEAD}");
            body.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.BODY}");
            leftArm.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.LEFT_ARM}");
            rightArm.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.RIGHT_ARM}");
            leftLeg.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.LEFT_LEG}");
            rightLeg.sprite = AssetLoader.getSprite($"{combatantClassName}.{BodyPart.RIGHT_LEG}");
        }

        /// <summary>
        /// Initializes unity game components
        /// </summary>
        void Awake()
        {
            parent = gameObject.transform.Find("parent");
            body = parent.transform.Find(BodyPart.BODY).GetComponent<SpriteRenderer>();
            head = body.transform.Find(BodyPart.HEAD).GetComponent<SpriteRenderer>();

            leftArm = body.transform.Find(BodyPart.LEFT_ARM).GetComponent<SpriteRenderer>();
            rightArm = body.transform.Find(BodyPart.RIGHT_ARM).GetComponent<SpriteRenderer>();

            leftLeg = parent.transform.Find(BodyPart.LEFT_LEG).GetComponent<SpriteRenderer>();
            rightLeg = parent.transform.Find(BodyPart.RIGHT_LEG).GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Sets color of all body parts for the purpose of showing colored hit-indicators.
        /// </summary>
        /// <param name="color"></param>
        public void setColor(Color color)
        {
            foreach(SpriteRenderer component in gameObject.transform)
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
            return body.bounds.Contains(position) || head.bounds.Contains(position);
        }
    }
}
