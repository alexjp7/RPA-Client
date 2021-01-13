namespace Assets.Scripts.Entities.Items
{
    abstract class Item
    {
        public int id { get; private set; }
        public string name { get; private set; }
        public int coinValue { get; private set; }
    }
}
