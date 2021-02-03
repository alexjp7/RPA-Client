using Assets.Scripts.Util;
using log4net;
using UnityEngine;

namespace Assets.Scripts.UI.Combat
{
    /// <summary>
    /// Body components for a combatant's skeleton. 
    /// Contains the individual body parts that is rigged up into a humaniod character model.
    /// </summary>
    public class HumaniodRig : CharacterRig
    {

        /// <summary>
        /// Creates object hierachy  
        /// </summary>
        private void Awake()
        {
            base.Awake();

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
