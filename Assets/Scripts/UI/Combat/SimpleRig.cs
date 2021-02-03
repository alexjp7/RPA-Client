using Assets.Scripts.UI.Combat;
using UnityEngine;

namespace Assets.Scripts.Util
{
    /// <summary>
    /// For characters that have a single sprite texture for body rig
    /// </summary>
    public class SimpleRig : CharacterRig
    {
        /// <summary>
        /// Creates object hierachy  
        /// </summary>
        private void Awake()
        {
            base.Awake();
            parts.Add(BodyPart.BODY, parent.transform.Find(BodyPart.BODY).GetComponent<SpriteRenderer>());
        }
    }
}