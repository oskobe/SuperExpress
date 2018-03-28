using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    class TrackDetail
    {
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public DateTime OccurredAt { get; set; }

        public string _activity;
        public string Activity
        {
            get
            {
                return _activity.ToLowerInvariant();
            }
            set
            {
                _activity = value;
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
