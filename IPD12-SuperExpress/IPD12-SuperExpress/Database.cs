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



        /*
        public List<Person> GetAllPeople()
        {
            List<Person> list = new List<Person>();
            SqlCommand command = new SqlCommand("SELECT * FROM people", conn);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Person person = new Person((int)reader[0], (string)reader[1], (int)reader[2], (double)reader[3]);
                    list.Add(person);
                }
                return list;
            }

        }

        public void GetAllPeople(List<Person> list)
        {
            List<Person> list2 = new List<Person>();
            list = list2;
            SqlCommand command = new SqlCommand("SELECT * FROM people", conn);

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Person person = new Person((int)reader[0], (string)reader[1], (int)reader[2], (double)reader[3]);
                    list2.Add(person);
                }
                //return list;
            }

        }

        public void AddPerson(Person person)
        {
            using (SqlCommand insertCommand = new SqlCommand("INSERT INTO people (name, age, height) VALUES (@name, @age, @height)", conn))
            {
                insertCommand.Parameters.AddWithValue(@"name", person.Name);
                insertCommand.Parameters.AddWithValue(@"age", person.Age);
                insertCommand.Parameters.AddWithValue(@"height", person.Height);
                insertCommand.ExecuteNonQuery();
            }

        }

        public void UpdatePerson(Person person)
        {
            using (SqlCommand insertCommand = new SqlCommand("UPDATE people SET name = @name, age = @age, height = @height WHERE id = @id", conn))
            {
                insertCommand.Parameters.AddWithValue(@"name", person.Name);
                insertCommand.Parameters.AddWithValue(@"age", person.Age);
                insertCommand.Parameters.AddWithValue(@"height", person.Height);
                insertCommand.Parameters.AddWithValue(@"id", person.Id);
                insertCommand.ExecuteNonQuery();
            }

        }

        public void DeletePerson(int id)
        {
            using (SqlCommand insertCommand = new SqlCommand("DELETE FROM people WHERE id = @id", conn))
            {
                insertCommand.Parameters.AddWithValue(@"id", id);
                insertCommand.ExecuteNonQuery();
            }

        }
        */
        public void AddUser(User user)
        {
            string query = "INSERT INTO users (name, phone,email,postalCode,countryCode,provinceCode,cityName,streetName,apartment,Password) values(@name, @phone,@email,@postalCode,@countryCode,@provinceCode,@cityName,@streetName,@apartment,@Password)";
            try
            {
                using (MySqlCommand insertCommand = new MySqlCommand(query, Globals.db.conn))
                {
                    insertCommand.Parameters.AddWithValue("@name", user.Name);
                    insertCommand.Parameters.AddWithValue("@phone", user.Phone);
                    insertCommand.Parameters.AddWithValue("@email", user.Email);
                    insertCommand.Parameters.AddWithValue("@postalCode", user.PostalCode);
                    insertCommand.Parameters.AddWithValue("@countryCode", user.CountryCode);
                    insertCommand.Parameters.AddWithValue("@provinceCode", user.ProvinceCode);
                    insertCommand.Parameters.AddWithValue("@cityName", user.CityName);
                    insertCommand.Parameters.AddWithValue("@streetName", user.StreetName);
                    insertCommand.Parameters.AddWithValue("@apartment", user.Apartment);
                    insertCommand.Parameters.AddWithValue("@Password", user.Password);
                    insertCommand.ExecuteNonQuery();
                }
            }
            catch (MySqlException ex)
            {
                throw ex;
            }
        }
        public string SelectPasswordByEmail(string email)
        {
            using (MySqlCommand command = new MySqlCommand("SELECT password FROM users WHERE email = @email", conn))
            {
                command.Parameters.AddWithValue("@email", email);
                try
                {
                    using (MySqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader["password"].ToString();
                        }
                    }
                }
                catch (MySqlException ex)
                {
                    throw ex;
                }
                return string.Empty;
            }
        }
    }
}
