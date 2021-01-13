/*---------------------------------------------------------------
                       TURN-CHEVRON
 ---------------------------------------------------------------*/
/***************************************************************
* The script attached to the turnchevron prefab,
  used to manipulate the turn indcator instance.
**************************************************************/
namespace Assets.Scripts.Combat
{
    using UnityEngine;
    using Assets.Scripts.Util;

    public class TurnChevron : MonoBehaviour
    {
        private SpriteRenderer chevronSprite;
        private Transform chevronTransform;

        private static TurnChevron currentChevron;
        private static TurnChevron nextChevron;


        private static TurnChevron create(in Transform combatant)
        {
            Transform chevronTransform = Instantiate(GameAssets.INSTANCE.turnChevronPrefab, Vector2.zero, Quaternion.identity);
            TurnChevron chevron = chevronTransform.GetComponent<TurnChevron>();
            return chevron;
        }

        public static void updateTurnChevrons(in Transform currentCombant, in Transform nextcombatant, bool isPlayerTurn)
        {
            if (currentChevron == null)
            {
                currentChevron = create(currentCombant);
            }

            if (nextChevron == null)
            {
                nextChevron = create(nextcombatant);
            }

            if (isPlayerTurn)
            {
                currentChevron.chevronSprite.color = Color.green;
                nextChevron.chevronSprite.color = Color.red;
            }
            else
            {
                currentChevron.chevronSprite.color = Color.red;
                nextChevron.chevronSprite.color = Color.green;
            }

            currentChevron.setPosition(currentCombant);
            nextChevron.setPosition(nextcombatant);

        }

        void Awake()
        {
            chevronSprite = gameObject.GetComponent<SpriteRenderer>();
            chevronTransform = gameObject.transform;
        }
        /***************************************************************
        * Updates the position of the chevron to reflect the next
         combatants turn.

         @param  - position: The new position that the chevron
         transform will be updated to.
        **************************************************************/
        public void setPosition(Transform position)
        {
            Vector3 localPoint = position.position;
            localPoint.y += 150;
            chevronTransform.SetParent(position.transform);
            chevronTransform.position = localPoint;
        }
    }
}
