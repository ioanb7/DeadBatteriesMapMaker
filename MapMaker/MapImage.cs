using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MapMaker
{
    public class MapImage : MapImageResource
    {
        public int MapId { get; set; }
        public Point pos { get; set; }
        public Point MapDimensions { get; set; }
        public int Zindex { get; set; }

        public static int ZindexTotal = 1;
        public bool isStretched;
    }
}
