using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class Globals
    {
        public const string APIKEY_SHIPENGINE = "7YPRCFZhpdEfNSXWHCG5TyZwu6dGx9MwdK+r1R0FrUU";
        public const string CARRIER_CODE_UPS = "ups";
        public const string CARRIER_ID_UPS = "se-241902";
        public const int PERIMETER_OF_EARTH = 40075;
        public const double SPEECH_HURRICANE = 28.5;
        public const double VERY_COLD = -25;
        public static Database db=new Database();
        public static  string emailExpression = @"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$";
        public static User currentUser;
        public const string YES = "Yes";
        public const string NO = "No";
        public const string CURRENCY_CAD = " CAD";
        public const string SHIPPING_INVOICE = "Shipping_Invoice_";
    }
}
