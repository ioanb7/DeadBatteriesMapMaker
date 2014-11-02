using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class MapRectangle
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }

        public bool Intersects(MapRectangle mapRectangle)
        {
            Rectangle rect1 = new Rectangle(X, Y, Width, Height);
            Rectangle rect2 = new Rectangle(mapRectangle.X, mapRectangle.Y, mapRectangle.Width, mapRectangle.Height);

            return rect1.IntersectsWith(rect2);
        }
    }
}
