namespace Assets.Scripts.Entities.Containers
{
    using log4net;
    using System.Collections.Generic;
    using System.Linq;

    using Assets.Scripts.Entities.Components;

    /// <summary>
    /// Base class for combat party container types.
    /// </summary>
    public class CombatParty
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(CombatParty));

        /// <summary>
        /// The combatant whos turn it was most recently for an instance of a party container.
        /// </summary>
        public Combatant currentCombatant { get; private set; }


        /// <summary>
        /// Used for turn order cycling
        /// </summary>
        private Queue<Combatant> combatQueue;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="combatants">List of combatants which should be sorted into turn order.</param>
        public CombatParty(in List<Combatant> combatants)
        {
            this.combatQueue = new Queue<Combatant>(combatants);
        }

        public List<Combatant> asList()
        {
            return combatQueue.AsEnumerable().ToList(); 
        }

        /// <summary>
        /// Cycles through to the next combatant in turn order. 
        /// </summary>
        /// <returns>The combatant whos turn it is. Returns <c>null</c> if party is defeated.</returns>
        public Combatant cycleNext()
        {
            if (checkDefeatedAndRemoveDead())
            {
                return null;
            }

            do
            {
                currentCombatant = combatQueue.Dequeue();
            }
            while (!currentCombatant.isAlive());

            combatQueue.Enqueue(currentCombatant);

            return currentCombatant;
        }

        /// <summary>
        /// Checks the next combatant for a player/monster party
        /// </summary>
        /// <returns>The next combatant to have its turn.</returns>
        public Combatant peekNext()
        {
            return combatQueue.Peek();
        }

        private void removeDead()
        {
            combatQueue = new Queue<Combatant>(combatQueue.Where(combatant => combatant.isAlive()));
        }

        /// <summary>
        /// Checks if the party of combatants is defeated through evaluating if all members are dead. Removes dead combatants.
        /// </summary>
        /// <returns>
        /// <list type="bullet">
        /// <item>
        /// True - The party has no members alive
        /// </item>
        /// <item>
        /// False - The party has atleast 1 member alive
        /// </item>
        /// </list>
        /// </returns>
        public bool checkDefeatedAndRemoveDead()
        {
            removeDead();
            return !combatQueue.Any();
        }

        /// <summary>
        /// Resets the combat party for the beginning of a new encounter.
        /// </summary>
        public void reset(List<Combatant> combatants)
        {
            foreach (Combatant combatant in combatants)
            {
                combatant.reset();
            }

            combatQueue = new Queue<Combatant>(combatants);
        }
    }
}
