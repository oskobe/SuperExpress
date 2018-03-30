using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class User
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public long Phone { get; set; }
        public string Email { get; set; }
        public string PostalCode { get; set; }
        public string CountryCode { get; set; }
        public string ProvinceCode { get; set; }
        public string CityName { get; set; }
        public string StreetName { get; set; }
        public string Apartment { get; set; }
    }
}
