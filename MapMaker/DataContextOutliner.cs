using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class DataContextOutliner
    {
        public List<MapImageResource> mapImageResources
        {
            get
            {
                return MM.Instance.mapImageResources;
            }
        }
    }
}
