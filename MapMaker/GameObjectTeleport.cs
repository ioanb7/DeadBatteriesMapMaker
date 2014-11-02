using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    //public class GameObjectTeleport
    //{
    //    public Point pos { get; set; }
    //    //public Map nextMap { get; set; } // can be null if the game finishes here.
    //    //^ just a pointer


    //}
    public class GameObjectTeleportBase : GameObject
    {
        public Point pos { get; set; }
        public GameObjectMapBase nextMap { get; set; } // can be null if the game finishes here.

    }
}
