using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class Country
    {
        public string Code { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }

    }


}
