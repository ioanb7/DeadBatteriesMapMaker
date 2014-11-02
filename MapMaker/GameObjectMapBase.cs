using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class GameObjectMapBase
    {
        public string Id { get; set; } // choose between a list of already extistent maps.

        public Point dimensions { get; set; }
    }
}
