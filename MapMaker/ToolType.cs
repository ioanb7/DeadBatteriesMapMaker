using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public enum ToolType
    {
        MapImage,
        MapImageRect, // show a rect when drawing.
        MapImageRectRevise,
        Collision,
        ImageProperties,
        None
    }
}
