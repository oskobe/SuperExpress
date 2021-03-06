﻿using System;
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
            using (MySqlCommand insertCommand = new MySqlCommand("INSERT INTO orders (serviceType, guaranteedService, estimatedDeliveryDate, weight, weightUnit, length, width, height, dimensionsUnit, amount, currency, senderName, countryFrom, provinceFrom, cityFrom, addressFrom, postalCodeFrom, recipientName, countryTo, provinceTo, cityTo, addressTo, postalCodeTo) VALUES (@serviceType, @guaranteedService, @estimatedDeliveryDate, @weight, @weightUnit, @length, @width, @height, @dimensionsUnit, @amount, @currency, @senderName, @countryFrom, @provinceFrom, @cityFrom, @addressFrom, @postalCodeFrom, @recipientName, @countryTo, @provinceTo, @cityTo, @addressTo, @postalCodeTo); SELECT LAST_INSERT_ID()", conn))

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
                insertCommand.Parameters.AddWithValue("@senderName", sr.SenderName);
                insertCommand.Parameters.AddWithValue("@countryFrom", sr.CountryFrom);
                insertCommand.Parameters.AddWithValue("@provinceFrom", sr.ProvinceFrom);
                insertCommand.Parameters.AddWithValue("@cityFrom", sr.CityFrom);
                insertCommand.Parameters.AddWithValue("@addressFrom", sr.AddressFrom);
                insertCommand.Parameters.AddWithValue("@postalCodeFrom", sr.PostalCodeFrom);
                insertCommand.Parameters.AddWithValue("@recipientName", sr.RecipientName);
                insertCommand.Parameters.AddWithValue("@countryTo", sr.CountryTo);
                insertCommand.Parameters.AddWithValue("@provinceTo", sr.ProvinceTo);
                insertCommand.Parameters.AddWithValue("@cityTo", sr.CityTo);
                insertCommand.Parameters.AddWithValue("@addressTo", sr.AddressTo);
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

        public User GetUserByUserId(string userId)
        {
            User user = null; 
            MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE userId = @userId LIMIT 1", conn);
            command.Parameters.AddWithValue("@userId", userId);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    user = new User((int)reader["id"], (string)reader["userId"], (string)reader["password"], (string)reader["name"], (long)reader["phone"], (string)reader["email"], (string)reader["countryCode"], (string)reader["provinceCode"], (string)reader["cityName"], (string)reader["address"], (string)reader["postalCode"]);
                }
                return user;
            }
        }

        public User GetUserByEmail(string email)
        {
            User user = null;
            MySqlCommand command = new MySqlCommand("SELECT * FROM users WHERE email = @email LIMIT 1", conn);
            command.Parameters.AddWithValue("@email", email);

            using (MySqlDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    user = new User((int)reader["id"], (string)reader["userId"], (string)reader["password"], (string)reader["name"], (long)reader["phone"], (string)reader["email"], (string)reader["countryCode"], (string)reader["provinceCode"], (string)reader["cityName"], (string)reader["address"], (string)reader["postalCode"]);
                }
                return user;
            }
        }
    }
}
