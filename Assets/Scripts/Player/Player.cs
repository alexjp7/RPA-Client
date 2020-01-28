

namespace Assets.Scripts.RPA_Player
{
    [System.Serializable]
    public class Player
    {
        public int id;
        public string name;
        public int adventuringClass;
        public bool ready;

        //Default construction
        public Player()
        {
            this.name = "";
            this.adventuringClass = -1;
            this.id = -1;
            this.ready = false;
        }  
        //Called on json -> player construction in connection message
        public Player(int id, string name, int adventuringClass, bool ready)
        {
            this.id = id;
            this.name = name;
            this.adventuringClass = adventuringClass;
            this.ready = ready;
        } 

        public string toString()
        {
            return "id: " + this.id + " name: " + this.name + " class: " + this.adventuringClass + " ready:" + ready;
        }
    }
  
}
