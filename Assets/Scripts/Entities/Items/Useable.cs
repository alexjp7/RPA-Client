namespace Assets.Scripts.Entities.Items
{
    interface Useable
    {
        /// <summary>
        /// Method to implement which signifies what logic to execute when an item is used via double click or context menu.
        /// </summary>
        void use();
    }
}
