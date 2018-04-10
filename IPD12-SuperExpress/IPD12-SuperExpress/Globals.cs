using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class Globals
    {
        public const string APIKEY_SHIPENGINE = "7YPRCFZhpdEfNSXWHCG5TyZwu6dGx9MwdK+r1R0FrUU";
        public const string APIKEY_BINGMAPS = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";
        public const string APIKEY_OPENWEATHER = "23a61d3a72f546a7a1659131fb9499c0";
        public const string CARRIER_CODE_UPS = "ups";
        public const string CARRIER_ID_UPS = "se-241902";
        public const int PERIMETER_OF_EARTH = 40075;
        public const double SPEECH_HURRICANE = 28.5;
        public const double VERY_COLD = -15;
        public const double SPEECH_POWER = 20.8;
        public const double EXTRAMELY_COLD = -25;
        public const int SHIPMAP_CLICK_ELASPED_TIME = 10;
        public static Database db=new Database();
        public static string emailExpression = @"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$";
        public static User currentUser;
        public const string YES = "Yes";
        public const string NO = "No";
        public const string CURRENCY_CAD = " CAD";
        public const string SHIPPING_INVOICE = "Shipping_Invoice_";
        public const string COUNTRY_CANADA = "Canada";
        public const string DEFAULT_EMAIL = "example@superexpress.com";

        public static string GetMd5Hash(string input)
        {
            MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

            byte[] inputBytes = Encoding.Default.GetBytes(input);

            byte[] data = md5Hasher.ComputeHash(inputBytes);

            StringBuilder sBuilder = new StringBuilder();

            // Change to hexadecimal
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            return sBuilder.ToString();
        }

        public static bool VerifyMd5Hash(string input, string hash)
        {
            string hashOfInput = GetMd5Hash(input);

            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            if (comparer.Compare(hashOfInput, hash) == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static string GetErrorMessage(string input)
        {
            string newStr1, newStr2;
            int idx1 = input.IndexOf("message");
            if (idx1 >= 0){
                newStr1 = input.Substring(idx1 + 11);
                int idx2 = newStr1.IndexOf("\"");
                newStr2 = newStr1.Substring(0, idx2);
            }
            else
            {
                newStr2 = input;
            }
           
            return newStr2;
        }
    }
}
