using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class Coordinate
    {
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public Coordinate(string latitude, string longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }
    }
}
