using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Client;
using ShipEngine.ApiClient.Model;
using static ShipEngine.ApiClient.Model.Weight;
using System.Net;
using Microsoft.Maps.MapControl.WPF;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using BingMapsRESTToolkit;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using OpenWeatherMap;

namespace IPD12_SuperExpress
{

    /// <summary>
    /// Interaction logic for MainDialog.xaml
    /// </summary>
    /// 


    public partial class MainDialog : Window
    {
        List<TrackDetail> trackDetailList = new List<TrackDetail>();
        List<TrackDetail> filteredtrackDetailList;
        List<Country> countryList = new List<Country>();
        List<Province> provinceList = new List<Province>();
        List<Coordinate> coordinateList = new List<Coordinate>();
        static int earthRadius = 6367;
        //double distance;
        private string BingMapsKey = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";
        private readonly OpenWeatherMapClient OpenWeatherMapTestClient = new OpenWeatherMapClient("23a61d3a72f546a7a1659131fb9499c0");
        public MainDialog()
        {
            InitializeComponent();
            myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(45.404761, -73.9448513);
            try
            {                
                //InitializeComponent();
                InitializeDataFromDatabase();
                InitializeShippingCostCalculator();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Error opening database connection: " + ex.Message);
                Environment.Exit(1);

            }
        }
        public class Cartesian
        {
            public double X { set; get; }
            public double Y { set; get; }
            public double Z { set; get; }

            public Cartesian(double x, double y, double z)
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        public class TrackDetailComparer : IEqualityComparer<TrackDetail>
        {
            //remove the the same location      
            public bool Equals(TrackDetail x, TrackDetail y)
            {
                if ((x.CountryCode.CompareTo(y.CountryCode) == 0) && (x.City.CompareTo(y.City) == 0))
                    return true;
                else
                    return false;
            }

            public int GetHashCode(TrackDetail obj)
            {
                return 0;
            }
        }
        public static Response MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        throw new Exception(String.Format(
                        "Server error (HTTP {0}: {1}).",
                        response.StatusCode,
                        response.StatusDescription));
                    DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(Response));
                    object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
                    /*
                    JArray obj = (JArray)objResponse;
                    obj["resourceSets"];
                    */
                    Response jsonResponse = objResponse as Response;
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }
        private double DegtoRad(double x)
        {
            return x * Math.PI / 180;
        }
        private Cartesian convertSphericalToCartesian(Coordinate latlong)
        {
            var lat = DegtoRad(latlong.Latitude);
            var lon = DegtoRad(latlong.Longitude);
            var x = earthRadius * Math.Cos(lat) * Math.Cos(lon);
            var y = earthRadius * Math.Cos(lat) * Math.Sin(lon);
            var z = earthRadius * Math.Sin(lat);
            return new Cartesian(x, y, z);
        }
        private double CaculateArcLength(Coordinate a, Coordinate b)
        {
            var point1 = convertSphericalToCartesian(a);
            var point2 = convertSphericalToCartesian(b);
            var cordLength = Math.Sqrt(Math.Pow(point1.X - point2.X, 2) + Math.Pow(point1.Y - point2.Y, 2) + Math.Pow(point1.Z - point2.Z, 2));
            var centralAngle = 2 * Math.Asin(cordLength / 2 / earthRadius);
            return earthRadius * centralAngle;
        }
        private double CaculateMaxDistance()
        {
            int locNum = coordinateList.Count();
            //List<double> distanceList = new List<double>();
            List<double> distanceList2 = new List<double>();
            //Get formatted addresses: Option 1
            //Get all locations in the response and then extract the formatted address for each location            

            if (locNum > 1)
            {
                for (int i = 0; i < locNum; i++)
                {
                    for (int j = i + 1; j < locNum; j++)
                    {
                        distanceList2.Add(CaculateArcLength(coordinateList.ElementAt(i), coordinateList.ElementAt(j)));
                        //JArray
                        /*
                        string url = GetDistanceOfTwoEndHTTPRequest(coordinateList.ElementAt(i), coordinateList.ElementAt(j));
                        Response response = MakeRequest(url);
                        if (response != null)
                        {
                            distanceList.Add(((DistanceMatrix)(response.ResourceSets[0].Resources[0])).Results[0].TravelDistance);
                        }   
                        */
                    }
                }                
                return distanceList2.Max();
            }
            return 0.0;
        }
        private string GetDistanceOfTwoEndHTTPRequest(Coordinate a, Coordinate b)
        {
            return string.Format(@"https://dev.virtualearth.net/REST/v1/Routes/DistanceMatrix?origins={0},{1}&destinations={2},{3}&distanceUnit=kilometer&travelMode=driving&key={4}", a.Latitude, a.Longitude, b.Latitude, b.Longitude, BingMapsKey);
        }
        private Coordinate getCenterPoint()
        {
            int count = coordinateList.Count();
            Coordinate c = null;
            if (count > 1)
            {
                c = coordinateList.ElementAt(count / 2);
            }
            else if (count == 1)
            {
                c = coordinateList.ElementAt(0);
            }
            return c;
        }
        private int getZoomLevel(double distance)
        {
            // reference:https://msdn.microsoft.com/en-us/library/aa940990.aspx
            
            if (distance >9000)
            {
                return 1;
            }
            else if (distance>4500)
            {
                return 2;
            }
            else if (distance>2250)
            {
                return 3;
            }
            else if (distance > 1125)
            {
                return 4;
            }
            else if (distance > 650)
            {
                return 5;
            }
            else if (distance > 325)
            {
                return 6;
            }
            else if (distance > 162.5)
            {
                return 7;
            }
            else if (distance > 81)
            {
                return 8;
            }
            else if (distance > 40)
            {
                return 9;
            }
            else if (distance > 20)
            {
                return 10;
            }
            else if (distance > 10)
            {
                return 9;
            }
            else if (distance > 5)
            {
                return 10;
            }
            else if (distance > 2.5)
            {
                return 11;
            }
            else if (distance > 1.25)
            {
                return 12;
            }
            return 13;
        }
        private static Coordinate GetCentralGeoCoordinate(IList<Coordinate> geoCoordinates)
        {
            if (geoCoordinates.Count == 1)
            {
                return geoCoordinates.Single();
            }
            double x = 0;
            double y = 0;
            double z = 0;
            foreach (var geoCoordinate in geoCoordinates)
            {
                var latitude = geoCoordinate.Latitude * Math.PI / 180;
                var longitude = geoCoordinate.Longitude * Math.PI / 180;

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }
            var total = geoCoordinates.Count;
            x = x / total;
            y = y / total;
            z = z / total;
            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);
            return new Coordinate(centralLatitude * 180 / Math.PI, centralLongitude * 180 / Math.PI);
        }

        private void btnTracking_Click(object sender, RoutedEventArgs e)
        {
            string postalCode = string.Empty;
            string countryCode = string.Empty;
            string cityName = string.Empty;
            //remove the distinct location
            List<TrackDetail> tempList = trackDetailList.Distinct(new TrackDetailComparer()).ToList();
            //remove the location whose postalCode is null
            var filteredTrackList = from td in tempList where td.CountryCode != string.Empty && td.City != string.Empty select td;
            filteredtrackDetailList = filteredTrackList.Reverse().ToList();
            foreach (var td in filteredtrackDetailList)
            {
                postalCode = td.PostalCode;
                countryCode = td.CountryCode;
                cityName = td.City;
                XmlDocument searchResponse = Geocode(postalCode, countryCode, cityName);
                if (searchResponse != null)
                {
                    coordinateList.Add(ConvertLocationToCoordinate(searchResponse));
                }                
                else
                {
                    filteredtrackDetailList.Remove(td);
                }                
            }
            int count = coordinateList.Count();
            if (count > 1)
            {
                AddPolyline();
                AddPushpinAndWeatherInfoToMap();
            }
            else if (count == 1)
            {
                AddPushpinAndWeatherInfoToMap();
            }
            //CaculateMaxDistance();
            //It is for getting the distance between sender and receiver. That is for decide which Zoomlevel the maps will display
            //string url = "https://dev.virtualearth.net/REST/v1/Routes/DistanceMatrix?origins=47.6149,-122.1936&destinations=47.4747,-122.2057&distanceUnit=mile&travelMode=driving&key=" + BingMapsKey;

            double maxDistance = CaculateMaxDistance();            
            Coordinate center = GetCentralGeoCoordinate(coordinateList);
            if (center != null)
            {
                myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(center.Latitude, center.Longitude);
            }
            myMap.ZoomLevel = getZoomLevel(maxDistance);
            //myMapLabel.Visibility = Visibility.Visible;
            myMap.Focus(); //allows '+' and '-' to zoom the map
        }
        
        //Add a pushpin and a label with weather information to the map
        private async Task AddPushpinAndWeatherInfoToMap()
        {
            int i = 1;
            int count = coordinateList.Count();
            for(int j = 0; j < count; j++)
            {
                Coordinate cd = coordinateList.ElementAt(j);
                Pushpin pushpin = new Pushpin();
                pushpin.Content = "" + i++;

                pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
                myMap.Children.Add(pushpin);

                TrackDetail td = filteredtrackDetailList.ElementAt(j);
                System.Windows.Controls.Label customLabel = new System.Windows.Controls.Label();
                CurrentWeatherResponse result = await OpenWeatherMapTestClient.CurrentWeather.GetByCoordinates(new Coordinates { Latitude = cd.Latitude, Longitude = cd.Longitude});
                customLabel.Content = string.Format("City:{0},Tempture:{1} C,Windy:{2}", td.City,result.Temperature.Value-273.15,result.Wind.Speed.Value);
                Color c = Colors.Blue;
                c.A = 100; //the point here is that you're setting the A component to 0 (alpha, 0 meaning completely transparent)
                customLabel.Background = new SolidColorBrush(c);
                // With map layers we can add WPF children to lat long (WPF Location obj) on the map.
                MapLayer labelLayer = new MapLayer();
                labelLayer.AddChild(customLabel, pushpin.Location);
                myMap.Children.Add(labelLayer);
            }            
        }
        private void AddPolyline()
        {
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.GreenYellow);
            polyline.StrokeThickness = 5;
            polyline.Opacity = 0.7;
            LocationCollection locationCollection = new LocationCollection();
            foreach (Coordinate cd in coordinateList)
            {
                locationCollection.Add(new Microsoft.Maps.MapControl.WPF.Location(cd.Latitude, cd.Longitude));
            }
            polyline.Locations = locationCollection;
            myMap.Children.Add(polyline);
        }
        // Geocode an address and return a latitude and longitude
        public XmlDocument Geocode(string postCode, string countryCode, string cityName)
        {
            string geocodeRequest = string.Empty;
            //Create REST Services geocode request using Locations API
            if (postCode != string.Empty)
            {
                geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + postCode + @"?o=xml&key=" + BingMapsKey;
            }
            else
            {
                geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + countryCode + @"/" + cityName + @"?o=xml&key=" + BingMapsKey;
            }


            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            return (geocodeResponse);
        }

        // Submit a REST Services or Spatial Data Services request and return the response
        private XmlDocument GetXmlResponse(string requestUrl)
        {
            System.Diagnostics.Trace.WriteLine("Request URL (XML): " + requestUrl);
            HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format("Server error (HTTP {0}: {1}).",
                    response.StatusCode,
                    response.StatusDescription));
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return xmlDoc;
            }
        }

        //Search for POI near a point
        private Coordinate ConvertLocationToCoordinate(XmlDocument xmlDoc)
        {
            //Get location information from geocode response 
            //Create namespace manager
            string latitude = string.Empty;
            string longitude = string.Empty;
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsmgr.AddNamespace("rest", "http://schemas.microsoft.com/search/local/ws/rest/v1");

            //Get all geocode locations in the response 
            XmlNodeList locationElements = xmlDoc.SelectNodes("//rest:Location", nsmgr);
            if (locationElements.Count == 0)
            {
                MessageBox.Show("Sorry! There is no tracking information");
            }
            else
            {
                //Get the geocode location points that are used for display (UsageType=Display)
                XmlNodeList displayGeocodePoints =
                        locationElements[0].SelectNodes(".//rest:GeocodePoint/rest:UsageType[.='Display']/parent::node()", nsmgr);
                latitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Latitude", nsmgr).InnerText;
                longitude = displayGeocodePoints[0].SelectSingleNode(".//rest:Longitude", nsmgr).InnerText;
            }
            return new Coordinate(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
        }

        private void btTrack_Click(object sender, RoutedEventArgs e)
        {
            var apiTrackInstance = new TrackingApi();
            var trackingNumber = tbTrackNumber.Text;

            try
            {
                TrackingInformation result = apiTrackInstance.TrackingTrack(Globals.APIKEY_SHIPENGINE, Globals.CARRIER_CODE_UPS, trackingNumber);

                List<TrackEvent> list = result.Events;
                trackDetailList.Clear();
                foreach (TrackEvent t in list)
                {
                    trackDetailList.Add(new TrackDetail() { City = t.CityLocality, StateProvinceCode = t.StateProvince, CountryCode = t.CountryCode, PostalCode = t.PostalCode, OccurredAt = (DateTime)t.OccurredAt, Activity = t.Description });
                }

                lvTrackDetails.ItemsSource = trackDetailList;

            }
            catch (Exception ex)
            {
                lblStatus.Content += "Exception when calling TrackingApi.TrackingTrack: " + ex.Message;
            }

        }

        private void btEstimate_Click(object sender, RoutedEventArgs e)
        {
            // Save input and result in costCalculator OBJ
            CostCalculator costCalculator = new CostCalculator();

            // Read and check input data
            costCalculator.CountryFrom = (Country)cbCountryFrom.SelectedItem;
            costCalculator.ProvinceFrom = (Province)cbProvinceStateFrom.SelectedItem;
            if (tbCityFrom.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please enter your origin city.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbCityFrom.Focus();
                return;
            }
            costCalculator.CityFrom = tbCityFrom.Text;
            if (tbPostalCodeFrom.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please enter your origin postal code.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbPostalCodeFrom.Focus();
                return;
            }
            costCalculator.PostalCodeFrom = tbPostalCodeFrom.Text;
            if (cbCountryTo.SelectedIndex == -1)
            {
                MessageBox.Show("Please choose your destination country.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            costCalculator.CountryTo = (Country)cbCountryTo.SelectedItem;
            costCalculator.ProvinceTo = (Province)cbProvinceStateTo.SelectedItem;
            if (tbCityTo.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please enter your destination city.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbCityTo.Focus();
                return;
            }
            costCalculator.CityTo = tbCityTo.Text;
            costCalculator.PostalCodeTo = tbPostalCodeTo.Text;

            double doubleWeight = 0;
            if (!double.TryParse(tbWeight.Text, out doubleWeight))
            {
                MessageBox.Show("Please enter double weight.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbWeight.Focus();
                return;
            }
            Weight.UnitEnum weightUnit = (Weight.UnitEnum)Enum.Parse(typeof(Weight.UnitEnum), cbWeithtUnit.SelectedItem.ToString(), false);
            Weight weight = new Weight(doubleWeight, weightUnit);
            costCalculator.Weight = weight;

            double doubleLength = 0, doubleWidth = 0, doubleHeight = 0;
            if (!double.TryParse(tbLength.Text, out doubleLength))
            {
                MessageBox.Show("Please enter double length.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbLength.Focus();
                return;
            }
            if (!double.TryParse(tbWidth.Text, out doubleWidth))
            {
                MessageBox.Show("Please enter double width.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbWidth.Focus();
                return;
            }
            if (!double.TryParse(tbHeight.Text, out doubleHeight))
            {
                MessageBox.Show("Please enter double height.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbHeight.Focus();
                return;
            }
            Dimensions.UnitEnum dimensionUnit = (Dimensions.UnitEnum)Enum.Parse(typeof(Dimensions.UnitEnum), cbDimensionUnit.SelectedItem.ToString(), false);
            Dimensions dimensions = new Dimensions(dimensionUnit, doubleLength, doubleWidth, doubleHeight);
            costCalculator.Dimensions = dimensions;

            // Calculate the rate
            var apiRatesInstance = new RatesApi();
            var estimateRequest = new RateEstimateRequest(Globals.CARRIER_ID_UPS, costCalculator.CountryFrom.Code, costCalculator.PostalCodeFrom, costCalculator.CountryTo.Code, costCalculator.PostalCodeTo, costCalculator.CityTo, costCalculator.ProvinceTo.ProvinceStateCode, costCalculator.Weight, costCalculator.Dimensions);
            try
            {
                costCalculator.Result = apiRatesInstance.RatesEstimate(estimateRequest, Globals.APIKEY_SHIPENGINE);

                ShippingCostCalculatorResult resultDialog = new ShippingCostCalculatorResult(costCalculator);
                if (resultDialog.ShowDialog() == true)
                {

                }
                /*
                foreach (Rate r in costCalculator.Result)
                {
                    lblStatus.Content = r.ToJson();
                    MessageBox.Show(r.ToJson());
                }
                */
            }
            catch (Exception ex)
            {
                lblStatus.Content = "Exception when calling RatesApi.RatesEstimate: " + ex.Message;
                MessageBox.Show(ex.Message);
            }

        }

        private void InitializeDataFromDatabase()
        {
            try
            {
                countryList = Globals.db.GetAllCountry();
                //provinceList = Globals.db.GetAllProvice();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Error fetching country or provice data from database: " + ex.Message);
                Environment.Exit(1);
            }
        }

        private void InitializeShippingCostCalculator()
        {
            //List<String> countryNameList = (from country in countryList orderby country.Name select country.Name).ToList();
            cbCountryFrom.ItemsSource = countryList;//countryNameList;
            cbCountryFrom.Text = "Canada";
            cbCountryTo.ItemsSource = countryList;

            cbWeithtUnit.ItemsSource = Enum.GetNames(typeof(Weight.UnitEnum));
            Weight.UnitEnum defaultWeightUnit = Weight.UnitEnum.Pound;
            cbWeithtUnit.SelectedIndex = cbWeithtUnit.Items.IndexOf(defaultWeightUnit.ToString());

            cbDimensionUnit.ItemsSource = Enum.GetNames(typeof(Dimensions.UnitEnum));
            Dimensions.UnitEnum defaultDimensionUnit = Dimensions.UnitEnum.Inch;
            cbDimensionUnit.SelectedIndex = cbDimensionUnit.Items.IndexOf(defaultDimensionUnit.ToString());

            /* for test start*/
            cbProvinceStateFrom.Text = "Quebec";
            tbCityFrom.Text = "laval";
            tbPostalCodeFrom.Text = "h7t2t4";

            cbCountryTo.Text = "Canada";
            cbProvinceStateTo.Text = "Quebec";
            tbCityTo.Text = "montreal";
            tbPostalCodeTo.Text = "h3t1e6";

            tbWeight.Text = "0.65";
            tbLength.Text = "2.33";
            tbWidth.Text = "3.52";
            tbHeight.Text = "1.0";
            /* for test end */

        }

        private void cbCountryFrom_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string countryCode = ((Country)cbCountryFrom.SelectedItem).Code;
            List<Province> provinceInSelectedCountryList = Globals.db.GetAllProviceByCountryCode(countryCode);
            cbProvinceStateFrom.ItemsSource = provinceInSelectedCountryList;
            cbProvinceStateFrom.SelectedIndex = 0;
        }

        private void cbCountryTo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string countryCode = ((Country)cbCountryTo.SelectedItem).Code;
            List<Province> provinceInSelectedCountryList = Globals.db.GetAllProviceByCountryCode(countryCode);
            cbProvinceStateTo.ItemsSource = provinceInSelectedCountryList;
            cbProvinceStateTo.SelectedIndex = 0;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}
