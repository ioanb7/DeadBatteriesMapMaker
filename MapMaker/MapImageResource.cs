using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MapMaker
{
    public class MapImageResource
    {
        public string Id { get; set; }
        public string Location { get; set; }
        public Point Dimensions { get; set; }
        public bool IsPartOfTheBackground { get; set; }
    }
}
