using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperExpress
{
    class TrackDetail
    {
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
        public string CountryCode { get; set; }
        public string PostalCode { get; set; }
        public DateTime OccurredAt { get; set; }
        public string Activity { get; set; }

        public string Location
        {
            get
            {
                return City + ", " + StateProvinceCode + ", " + CountryCode + " " + PostalCode ;
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
