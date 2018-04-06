using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class Database
    {
        MySqlConnection conn;

        public Database()
        {
            string dbConnectString = "server=den1.mysql1.gear.host;database=superexpress;user id=superexpress;password=Lf4g-Wc!wjBg";
            conn = new MySqlConnection(dbConnectString);
            conn.Open();
        }


        public void Addcountry(Country country)
        {
            using (MySqlCommand insertCommand = new MySqlCommand("INSERT INTO countries (code, name) VALUES (@code, @name)", conn))
            {
                insertCommand.Parameters.AddWithValue("@code", country.Code);
                insertCommand.Parameters.AddWithValue("@name", country.Name);
                insertCommand.ExecuteNonQuery();
            }

        }

        public void AddProvince(Province province)
        {
            using (MySqlCommand insertCommand = new MySqlCommand("INSERT INTO provinces (countrycode, code, name) VALUES (@countryCode, @code, @name)", conn))
            {
                insertCommand.Parameters.AddWithValue("@countryCode", province.CountryCode);
                insertCommand.Parameters.AddWithValue("@code", province.ProvinceStateCode);
                insertCommand.Parameters.AddWithValue("@name", province.ProvinceStateName);
                insertCommand.ExecuteNonQuery();
            }
        }

        public List<Country> GetAllCountry()
        {
            List<Country> list = new List<Country>();
            MySqlCommand command = new MySqlCommand("SELECT * FROM countries ORDER BY name", conn);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Country country = new Country() { Code = (string)reader["code"], Name = (string)reader["name"] };
                    list.Add(country);
                }
                return list;
            }
        }

        public List<Province> GetAllProvice()
        {
            List<Province> list = new List<Province>();
            MySqlCommand command = new MySqlCommand("SELECT * FROM provinces ORDER BY name", conn);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Province province = new Province() { CountryCode = (string)reader["countryCode"], ProvinceStateCode = (string)reader["code"], ProvinceStateName = (string)reader["name"] };
                    list.Add(province);
                }
                return list;
            }
        }

        public List<Province> GetAllProviceByCountryCode(string code)
        {
            List<Province> list = new List<Province>();
            MySqlCommand command = new MySqlCommand("SELECT * FROM provinces WHERE countryCode = @code ORDER BY name", conn);
            command.Parameters.AddWithValue("@code", code);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Province province = new Province() { CountryCode = (string)reader["countryCode"], ProvinceStateCode = (string)reader["code"], ProvinceStateName = (string)reader["name"] };
                    list.Add(province);
                }
                return list;
            }
        }

        public string AddOrder(ShipmentRequest sr)
        {
            using (MySqlCommand insertCommand = new MySqlCommand("INSERT INTO orders (serviceType, guaranteedService, estimatedDeliveryDate, weight, weightUnit, length, width, height, dimensionsUnit, amount, currency, countryFrom, provinceFrom, cityFrom, address1From, address2From, postalCodeFrom, countryTo, provinceTo, cityTo, address1To, address2To, postalCodeTo) VALUES (@serviceType, @guaranteedService, @estimatedDeliveryDate, @weight, @weightUnit, @length, @width, @height, @dimensionsUnit, @amount, @currency, @countryFrom, @provinceFrom, @cityFrom, @address1From, @address2From, @postalCodeFrom, @countryTo, @provinceTo, @cityTo, @address1To, @address2To, @postalCodeTo); SELECT LAST_INSERT_ID()", conn))

            {
                insertCommand.Parameters.AddWithValue("@serviceType", sr.ServiceType);
                insertCommand.Parameters.AddWithValue("@guaranteedService", sr.GuaranteedService);
                insertCommand.Parameters.AddWithValue("@estimatedDeliveryDate", sr.EstimatedDeliveryDate);
                insertCommand.Parameters.AddWithValue("@weight", sr.Weight);
                insertCommand.Parameters.AddWithValue("@weightUnit", sr.WeightUnit);
                insertCommand.Parameters.AddWithValue("@length", sr.Length);
                insertCommand.Parameters.AddWithValue("@width", sr.Width);
                insertCommand.Parameters.AddWithValue("@height", sr.Height);
                insertCommand.Parameters.AddWithValue("@dimensionsUnit", sr.DimensionsUnit);
                insertCommand.Parameters.AddWithValue("@amount", sr.Amount);
                insertCommand.Parameters.AddWithValue("@currency", sr.Currency);
                insertCommand.Parameters.AddWithValue("@countryFrom", sr.CountryFrom);
                insertCommand.Parameters.AddWithValue("@provinceFrom", sr.ProvinceFrom);
                insertCommand.Parameters.AddWithValue("@cityFrom", sr.CityFrom);
                insertCommand.Parameters.AddWithValue("@address1From", sr.Address1From);
                insertCommand.Parameters.AddWithValue("@address2From", sr.Address2From);
                insertCommand.Parameters.AddWithValue("@postalCodeFrom", sr.PostalCodeFrom);
                insertCommand.Parameters.AddWithValue("@countryTo", sr.CountryTo);
                insertCommand.Parameters.AddWithValue("@provinceTo", sr.ProvinceTo);
                insertCommand.Parameters.AddWithValue("@cityTo", sr.CityTo);
                insertCommand.Parameters.AddWithValue("@address1To", sr.Address1To);
                insertCommand.Parameters.AddWithValue("@address2To", sr.Address2To);
                insertCommand.Parameters.AddWithValue("@postalCodeTo", sr.PostalCodeTo);

                var generatedId = insertCommand.ExecuteScalar();
                return generatedId.ToString();
                //Console.WriteLine("ID is:  {0}", id);
            }
        }

        public int AddUser(User user)
        {
            using (MySqlCommand insertCommand = new MySqlCommand("INSERT INTO users (userId, password, name, phone, email, countryCode, provinceCode, cityName, address, postalCode) VALUES (@userId, @password, @name, @phone, @email, @countryCode, @provinceCode, @cityName, @address, @postalCode); SELECT LAST_INSERT_ID()", conn))

            {
                insertCommand.Parameters.AddWithValue("@userId", user.UserId);
                insertCommand.Parameters.AddWithValue("@password", user.Password);
                insertCommand.Parameters.AddWithValue("@name", user.Name);
                insertCommand.Parameters.AddWithValue("@phone", user.Phone);
                insertCommand.Parameters.AddWithValue("@email", user.Email);
                insertCommand.Parameters.AddWithValue("@countryCode", user.CountryCode);
                insertCommand.Parameters.AddWithValue("@provinceCode", user.ProvinceCode);
                insertCommand.Parameters.AddWithValue("@cityName", user.CityName);
                insertCommand.Parameters.AddWithValue("@address", user.Address);
                insertCommand.Parameters.AddWithValue("@postalCode", user.PostalCode);

                var generatedId = insertCommand.ExecuteScalar();
                return int.Parse(generatedId.ToString());
            } 
        }
    }
}
