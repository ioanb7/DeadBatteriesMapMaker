using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapMaker
{

    public class DatabaseTable : List<DatabaseRow> { }

    public class DatabaseRow
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

}
