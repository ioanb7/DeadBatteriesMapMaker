using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() { X = 0; Y = 0; }
        public Point (int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Point ConvertPoint(System.Windows.Point point)
        {
            return new Point((int)point.X, (int)point.Y);
        }
    }
}
