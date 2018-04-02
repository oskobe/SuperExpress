using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Client;
using ShipEngine.ApiClient.Model;
using System.Globalization;
using System.Threading;

namespace IPD12_SuperExpress
{
    public class CostCalculator
    {
        public Country CountryFrom { get; set; }
        public Province ProvinceFrom { get; set; }
        public string _cityFrom { get; set; }
        public string _postalCodeFrom { get; set; }
        public Country CountryTo { get; set; }
        public Province ProvinceTo { get; set; }
        public string _cityTo { get; set; }
        public string _postalCodeTo { get; set; }
        public Weight Weight { get; set; }
        public Dimensions Dimensions { get; set; }
    
        public List<Rate> Result { get; set; }

        static CultureInfo cultureInfo = Thread.CurrentThread.CurrentCulture;
        static TextInfo textInfo = cultureInfo.TextInfo;

        public string CityFrom
        {
            get
            {
                return _cityFrom;
            }
            set
            {
                _cityFrom = textInfo.ToTitleCase(textInfo.ToLower(value));
            }
        }

        public string CityTo
        {
            get
            {
                return _cityTo;
            }
            set
            {
                _cityTo = textInfo.ToTitleCase(textInfo.ToLower(value));
            }
        }

        public string PostalCodeFrom
        {
            get
            {
                return _postalCodeFrom;
            }
            set
            {
                _postalCodeFrom = textInfo.ToUpper(value);
            }
        }

        public string PostalCodeTo
        {
            get
            {
                return _postalCodeTo;
            }
            set
            {
                _postalCodeTo = textInfo.ToUpper(value);
            }
        }
    }
}
