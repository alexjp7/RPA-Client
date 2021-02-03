
namespace Assets.Scripts.Util
{
    using System;
    using Assets.Scripts.Common;
    using Assets.Scripts.Entities.Components;
    using Assets.Scripts.Entities.Monsters.MonsterTypes;
    using Assets.Scripts.Entities.Players;
    using Assets.Scripts.UI.Combat;
    using log4net;

    class RiggingFactory
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(RiggingFactory));

        internal static CharacterRig getRig(Combatant character, UnityEngine.Transform spriteTransform)
        {
            //All Adventurer types will have a humaniod rigging type
            if (character is Adventurer)
            {
                return spriteTransform.GetComponent<HumaniodRig>();
            }
            else
            {
                return spriteTransform.GetComponent<SimpleRig>();
            }
         /*   else
            {
                RiggingNotFound notFound = new RiggingNotFound($"Rigging class has not been registered for type [{character.GetType().Name}]");
                log.Error(notFound.Message, notFound);
                throw notFound;
            }*/





        }
    }
}
