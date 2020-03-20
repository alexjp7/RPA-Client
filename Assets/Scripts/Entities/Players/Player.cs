using Assets.Scripts.Player_Classes;
using Assets.Scripts.RPA_Entity_Components;

namespace Assets.Scripts.RPA_Player
{
    public class Player
    {
        public int id;
        public string name;
        public int adventuringClass;
        public bool ready;
        public bool isClientPlayer { set; get;} //flag for marking the client side player
        
        public Renderable assetData { get; set; }
        public AdventuringClass playerClass { get; set; }

        public Player()
        {
            this.name = "";
            this.adventuringClass = -1;
            this.id = -1;
            this.ready = false;
            assetData = new Renderable();
        }  
        public Player(int id, string name, int adventuringClass, bool ready)
        {
            this.id = id;
            this.name = name;
            this.adventuringClass = adventuringClass;
            this.ready = ready;
            assetData = new Renderable();
        } 

        public void applyClass()
        {
            switch ((PlayerClasses)this.adventuringClass)
            {
                case PlayerClasses.WARRIOR:
                    this.playerClass = new Warrior(this.name);
                    break;

                case PlayerClasses.WIZARD:
                    this.playerClass = new Wizard(this.name);
                    break;

                case PlayerClasses.ROGUE:
                    this.playerClass = new Rogue(this.name);
                    break;

                case PlayerClasses.CLERIC:
                    this.playerClass = new Cleric(this.name);
                    break;
            }
        }

        public string toString()
        {
            return "id: " + this.id + " name: " + this.name + " class: " + this.adventuringClass + " ready:" + ready;
        }
    }
  
}
