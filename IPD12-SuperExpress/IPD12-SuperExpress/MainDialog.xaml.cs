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

namespace IPD12_SuperExpress
{

    /// <summary>
    /// Interaction logic for MainDialog.xaml
    /// </summary>
    /// 


    public partial class MainDialog : Window
    {
        List<TrackDetail> trackDetailList = new List<TrackDetail>();
<<<<<<< HEAD
        List<TrackDetail> filteredtrackDetailList;
=======
        List<Country> countryList = new List<Country>();
        List<Province> provinceList = new List<Province>();
>>>>>>> 42bfd8020343de7297008d7a248b64ec474a71e7
        List<Coordinate> coordinateList = new List<Coordinate>();
        static int earthRadius = 6367;
        //double distance;
        private string BingMapsKey = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";

        public MainDialog()
        {
<<<<<<< HEAD
            InitializeComponent();
            myMap.Center = new Microsoft.Maps.MapControl.WPF.Location(45.404761, -73.9448513);           
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
=======
            try
            {
                Globals.db = new Database();
                InitializeComponent();
                InitializeDataFromDatabase();
                InitializeShippingCostCalculator();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Error opening database connection: " + ex.Message);
                Environment.Exit(1);

            }

>>>>>>> 42bfd8020343de7297008d7a248b64ec474a71e7
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
                        /*string url = GetDistanceOfTwoEndHTTPRequest(coordinateList.ElementAt(i), coordinateList.ElementAt(j));
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
            
            if (distance >8555)
            {
                return 1;
            }
            else if (distance>5268)
            {
                return 2;
            }
            else if (distance>2493)
            {
                return 3;
            }
            else if (distance > 1248)
            {
                return 4;
            }
            else if (distance > 620)
            {
                return 5;
            }
            else if (distance > 317)
            {
                return 6;
            }
            else if (distance > 156)
            {
                return 7;
            }
            else if (distance > 77)
            {
                return 8;
            }
            else if (distance > 37)
            {
                return 9;
            }
            else if (distance > 19)
            {
                return 10;
            }
            else if (distance > 9)
            {
                return 9;
            }
            else if (distance > 5)
            {
                return 10;
            }
            else if (distance > 2.4)
            {
                return 11;
            }
            else if (distance > 1.1)
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
            }
            int count = coordinateList.Count();
            if (count > 1)
            {
                AddPolyline();
                AddPushpinAndTextToMap();
            }
            else if (count == 1)
            {
                AddPushpinAndTextToMap();
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
        //Add a pushpin with a label to the map
        private void AddPushpinAndTextToMap()
        {
            int i = 1;
            foreach (Coordinate cd in coordinateList)
            {
                Pushpin pushpin = new Pushpin();
                pushpin.Content = "" + i++;

                pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
                myMap.Children.Add(pushpin);

                System.Windows.Controls.Label customLabel = new System.Windows.Controls.Label();
                customLabel.Content = "Country:Canada;City:Montreal";
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
            // Input check
            if (tbCityFrom.Text.Trim().Equals(""))
            {
                MessageBox.Show("Please enter your origin city.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbCityFrom.Focus();
                return;
            }

            if (tbPostalCodeFrom.Text.Trim().Equals(""))
            {
                MessageBox.Show("Please enter your origin postal code.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbPostalCodeFrom.Focus();
                return;
            }

            if (cbCountryTo.SelectedIndex == -1)
            {
                MessageBox.Show("Please choose your destination country.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (tbCityTo.Text.Trim().Equals(""))
            {
                MessageBox.Show("Please enter your destination city.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                tbCityTo.Focus();
                return;
            }


            Weight weight = new Weight(1.5, UnitEnum.Pound);
            var apiRatesInstance = new RatesApi();
            var estimateRequest = new RateEstimateRequest("se-241902", "CA", "H3T1E8", "CN", "299500", "Chuzhou", "AH", weight);
            try
            {
                List<Rate> result = apiRatesInstance.RatesEstimate(estimateRequest, Globals.APIKEY_SHIPENGINE);
                foreach (Rate r in result)
                {
                    lblStatus.Content = r.ToJson();
                    MessageBox.Show(r.ToJson());
                }
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
    }
}
