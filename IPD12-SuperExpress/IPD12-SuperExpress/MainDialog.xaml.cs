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
    public partial class MainDialog : Window
    {
        List<TrackDetail> trackDetailList = new List<TrackDetail>();
        List<Coordinate> coordinateList = new List<Coordinate>();
        double distance;
        private string BingMapsKey = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";

        public MainDialog()
        {
            InitializeComponent();
        }

        public class TrackDetailComparer : IEqualityComparer<TrackDetail>
        {
            //remove the the same location      
            public bool Equals(TrackDetail x, TrackDetail y)
            {
                if (x.PostalCode.CompareTo(y.PostalCode) == 0)
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
                    Response jsonResponse
                    = objResponse as Response;
                    return jsonResponse;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return null;
            }

        }
        private void TestRequest()
        {
            string url = "https://dev.virtualearth.net/REST/v1/Routes/DistanceMatrix?origins=47.6044,-122.3345;47.6731,-122.1185;47.6149,-122.1936&destinations=45.5347,-122.6231;47.4747,-122.2057&travelMode=driving&key=" + BingMapsKey;
            Response locationsResponse = MakeRequest(url);
            ProcessResponse(locationsResponse);
        }
        public void ProcessResponse(Response locationsResponse)
        {

            int locNum = locationsResponse.ResourceSets[0].Resources.Length;

            //Get formatted addresses: Option 1
            //Get all locations in the response and then extract the formatted address for each location
            Console.WriteLine("Show all formatted addresses");
            for (int i = 0; i < locNum; i++)
            {
                DistanceMatrix matrix = (DistanceMatrix)locationsResponse.ResourceSets[0].Resources[i];
                Console.WriteLine(matrix.Results[0].TravelDistance);
            }
            Console.WriteLine();
        }
        private async void CaculateMaxDistance()
        {
            int count = coordinateList.Count();
            if (true)
            {
                //var startPoint = coordinateList.ElementAt(0);
                //var endPoint = coordinateList.ElementAt(count - 1);
                Coordinate a = new Coordinate(88.590868, -122.336729);
                Coordinate b = new Coordinate(47.4747, -122.2057);
                
                
                var request = new DistanceMatrixRequest()
                {
                    Origins = new List<SimpleWaypoint>()
                {
                    new SimpleWaypoint(47.6044, -122.3345),
                    new SimpleWaypoint(47.6731, -122.1185),
                    new SimpleWaypoint(47.6149, -122.1936)
                },
                    Destinations = new List<SimpleWaypoint>()
                {
                    new SimpleWaypoint(45.5347, -122.6231),
                    new SimpleWaypoint(47.4747, -122.2057)
                },
                    BingMapsKey = BingMapsKey,
                    TimeUnits = TimeUnitType.Minute,
                    DistanceUnits = DistanceUnitType.Kilometers,
                    TravelMode = TravelModeType.Driving
                };
                try
                {
                    Response response = await request.Execute();
                    //(DistanceMatrix)(response.ResourceSets[0].Resources[0]);
                    //response.ResourceSets[0].Resources[0].

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                distance = 5;
            }            
        }
        
        private void btnTracking_Click(object sender, RoutedEventArgs e)
        {
            string postalCode = string.Empty;
            string countryCode = string.Empty;
            //remove the distinct location
            List<TrackDetail> tempList = trackDetailList.Distinct(new TrackDetailComparer()).ToList();
            //remove the location whose postalCode is null
            var filteredTrackList = from td in tempList where td.PostalCode != string.Empty select td;
            foreach (var td in filteredTrackList)
            {
                postalCode = td.PostalCode;
                countryCode = td.CountryCode;
                XmlDocument searchResponse = Geocode(postalCode, countryCode);
                coordinateList.Add(ConvertLocationToCoordinate(searchResponse));
            }
            int count = filteredTrackList.Count();
            if (count >1)
            {
                AddPolyline();
                AddPushpinToMap();
            }
            else if (count == 1)
            {
                AddPushpinToMap();
            }
            //CaculateMaxDistance();
            TestRequest();

            myMap.Visibility = Visibility.Visible;
            //myMap.Center = new Location(Convert.ToDouble(latitude), Convert.ToDouble(longitude));
            myMap.ZoomLevel = 12;
            //myMapLabel.Visibility = Visibility.Visible;
            myMap.Focus(); //allows '+' and '-' to zoom the map
        }
        //Add a pushpin with a label to the map
        private void AddPushpinToMap()
        {
            int i = 1;
            foreach (Coordinate cd in coordinateList)
            {
                Pushpin pushpin = new Pushpin();
                pushpin.Content = "" + i++;
                pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
                myMap.Children.Add(pushpin);
            }
        }
        private void AddPolyline()
        {
            MapPolyline polyline = new MapPolyline();
            polyline.Stroke = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Colors.Blue);
            polyline.StrokeThickness = 5;
            polyline.Opacity = 0.7;
            LocationCollection locationCollection = new LocationCollection();
            foreach (Coordinate cd in coordinateList)
            {
                locationCollection.Add(new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude)));
            }
            polyline.Locations = locationCollection;
            myMap.Children.Add(polyline);
        }
        // Geocode an address and return a latitude and longitude
        public XmlDocument Geocode(string postCode, string countryCode)
        {
            //Create REST Services geocode request using Locations API
            string geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + postCode + "?o=xml&key=" + BingMapsKey;

            //Make the request and get the response
            XmlDocument geocodeResponse = GetXmlResponse(geocodeRequest);

            return (geocodeResponse);
        }

        /// <summary>
        /// This method has a lot of logic that is specific to the sample. To process a request you can easily just call the Execute method on the request.
        /// </summary>
        /// <param name="request"></param>
        private async void ProcessRequest(BaseRestRequest request)
        {
            try
            {
                var response = await request.Execute();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
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

            Weight weight = new Weight(1.5, UnitEnum.Pound);
            var apiRatesInstance = new RatesApi();
            var estimateRequest = new RateEstimateRequest("se-241902", "CA", "H3T1E8", "CA", "H7T2T2", "Laval", "QCf", weight);
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
    }
}
