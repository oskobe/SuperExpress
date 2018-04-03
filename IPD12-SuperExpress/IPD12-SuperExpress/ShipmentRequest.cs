using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPD12_SuperExpress
{
    public class ShipmentRequest
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string ServiceType { get; set; }
        public bool GuaranteedService { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public double Weight { get; set; }
        public string WeightUnit { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string DimensionsUnit { get; set; }
        public double Amount { get; set; }
        public string Currency { get; set; }
        public string CountryFrom { get; set; }
        public string ProvinceFrom { get; set; }
        public string CityFrom { get; set; }
        public string Address1From { get; set; }
        public string Address2From { get; set; }
        public string PostalCodeFrom { get; set; }
        public string CountryTo { get; set; }
        public string ProvinceTo { get; set; }
        public string CityTo { get; set; }
        public string Address1To { get; set; }
        public string Address2To { get; set; }
        public string PostalCodeTo { get; set; }
    }
}
