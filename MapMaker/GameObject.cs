using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{
    public class GameObject
    {
        public int Id { get; set; }
        public MapImage mapImage { get; set; }


        //not needed because the variable names are self-explanatory - hopefully!
        public string Serialize(string x)
        {
            throw new NotImplementedException();
            if (x == "Type")
            {
                return "Type of object";
            }

            return String.Empty;
        }
    }
}
