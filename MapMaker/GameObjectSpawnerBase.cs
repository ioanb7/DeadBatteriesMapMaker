using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class GameObjectSpawnerBase : GameObject
    {
        public int monsters {get; set;}
        public GameObject trigger { get; set; }
    }
}
