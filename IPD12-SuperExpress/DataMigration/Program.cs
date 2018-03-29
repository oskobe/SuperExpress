using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IPD12_SuperExpress;
using System.Data.SqlClient;

namespace DataMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            Database db = null;

            try
            {
                db = new Database();
            }
            catch (SqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Error opening database connection: " + ex.Message);
                Environment.Exit(1);

            }
            /*
            string[] lines = File.ReadAllLines("../../data/country_list.csv");
            
            foreach (string line in lines)
            {
                string[] countryList = line.Split(',');
                string code = countryList[0];
                string name = countryList[1];
                Country country = new Country() { Code = code, Name = name };
                try
                {
                    db.Addcountry(country);
                    Console.WriteLine("Country: " + name + " added.");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("Error Adding country to database: " + ex.Message);
                }
            }
            */

            string[] lines = File.ReadAllLines("../../data/province_list.csv");

            foreach (string line in lines)
            {
                string[] provinceList = line.Split(',');
                string countryCode = provinceList[0];
                string code = provinceList[1];
                string name = provinceList[2];
                Province province = new Province() {CountryCode=countryCode, ProvinceStateCode = code, ProvinceStateName = name };
                try
                {
                    db.AddProvince(province);
                    Console.WriteLine("Provice: " + name + " added.");
                }
                catch (SqlException ex)
                {
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("Error Adding country to database: " + ex.Message);
                }
            }

            Console.ReadLine();
        }
    }
}
