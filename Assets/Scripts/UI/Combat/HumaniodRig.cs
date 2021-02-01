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
    /// Contains the individual body parts that is rigged up into a humaniod character model.
    /// </summary>
    public class HumaniodRig : CharacterRig
    {
        /// <summary>
        /// Creates object hierachy  
        /// </summary>
        protected override void Awake()
        {
            SpriteRenderer body = parent.transform.Find(BodyPart.BODY).GetComponent<SpriteRenderer>();
            SpriteRenderer head = body.transform.Find(BodyPart.HEAD).GetComponent<SpriteRenderer>();
            SpriteRenderer leftArm = body.transform.Find(BodyPart.LEFT_ARM).GetComponent<SpriteRenderer>();
            SpriteRenderer rightArm = body.transform.Find(BodyPart.RIGHT_ARM).GetComponent<SpriteRenderer>();
            SpriteRenderer leftLeg = parent.transform.Find(BodyPart.LEFT_LEG).GetComponent<SpriteRenderer>();
            SpriteRenderer rightLeg = parent.transform.Find(BodyPart.RIGHT_LEG).GetComponent<SpriteRenderer>();

            parts.Add(BodyPart.BODY, body);
            parts.Add(BodyPart.HEAD, head);
            parts.Add(BodyPart.LEFT_ARM, leftArm);
            parts.Add(BodyPart.RIGHT_ARM, rightArm);
            parts.Add(BodyPart.LEFT_LEG, leftLeg);
            parts.Add(BodyPart.RIGHT_LEG, rightLeg);
        }
    }
}
