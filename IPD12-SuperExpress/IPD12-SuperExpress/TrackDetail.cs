using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class TrackDetail
    {
        public string StateProvinceCode { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public DateTime OccurredAt { get; set; }

        public string _city;
        public string _activity;

        static CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        static TextInfo textInfo = cultureInfo.TextInfo;

        public string City {
            get {
                return _city;
            }
            set {
                _city = textInfo.ToTitleCase(textInfo.ToLower(value));
            }
        }

        public string Activity
        {
            get
            {
                return _activity;
            }
            set
            {
                
                _activity = textInfo.ToTitleCase(textInfo.ToLower(value));
            }
        }

        public string Location
        {
            get
            {
                List<string> locList = new List<string>();
                if (!City.Equals(""))
                {
                    locList.Add(City);
                }

                if (!StateProvinceCode.Equals(""))
                {
                    locList.Add(StateProvinceCode);
                }

                if (!CountryCode.Equals(""))
                {
                    locList.Add(CountryCode);
                }

                if (!PostalCode.Equals(""))
                {
                    locList.Add(PostalCode);
                }

                return string.Join(",", locList);
            }
        }

        public string Date
        {
            get
            {
                return OccurredAt.ToString("MM/dd/yyyy");
            }
        }

        public string LocalTime
        {
            get
            {
                return OccurredAt.ToString("HH:mm");
            }
        }
    }
}
