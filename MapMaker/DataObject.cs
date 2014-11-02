using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class DataObject
    {
        public string metadata;
        public MapImage BackgroundImage;
        public List<MapImageResource> mapImageResources;
        public bool isABackgroundImage;
        public List<MapImage> mapObjects;
        public bool areCollisionRectanglesAvailable;
        public CollisionHolder collisionHolder;
        public List<GameObject> gameObjects;
        public Map map;
    }

    public class CollisionHolder : MapRectangle
    {
        public List<MapRectangle> Children;
        public CollisionHolder()
        {
            Children = new List<MapRectangle>();
        }
    }
}
