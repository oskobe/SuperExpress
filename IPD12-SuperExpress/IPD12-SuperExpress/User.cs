using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class User
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Name { get; set; }
        public long Phone { get; set; }
        public string Email { get; set; }
        public string CountryCode { get; set; }
        public string ProvinceCode { get; set; }
        public string CityName { get; set; }
        public string Address { get; set; }
        public string PostalCode { get; set; }

        public User(int id, string userId, string password, string name, long phone, string email, string countryCode, string provinceCode, string cityName, string address, string postalCode)
        {
            Id = id;
            UserId = userId;
            Password = password;
            Name = name;
            Phone = phone;
            Email = email;
            PostalCode = postalCode;
            CountryCode = countryCode;
            ProvinceCode = provinceCode;
            CityName = cityName;
            Address = address;
            PostalCode = postalCode;
        }
    }
}
