using Microsoft.Maps.MapControl.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for Tracking.xaml
    /// </summary>
    /// 
    public class TrackDetailComparer : IEqualityComparer<TrackDetail>
    {
        #region IEqualityComparer<TrackDetail>
        
        public bool Equals(TrackDetail x, TrackDetail y)
        {
            if (x.PostalCode.CompareTo(y.PostalCode)==0)
                return true;
            else
                return false;
        }

        public int GetHashCode(TrackDetail obj)
        {
            return 0;
        }
        #endregion
    }
    //remove the the same location  
    
    public partial class Tracking : Window
    {
        List<TrackDetail> trackDetailList = new List<TrackDetail>();
        List<Coordinate> coordinateList = new List<Coordinate>();
        string BingMapsKey = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";
        public Tracking()
        {
            InitializeComponent();
        }

        private void btnTracking_Click(object sender, RoutedEventArgs e)
        {
            string postalCode=string.Empty;
            string countryCode = string.Empty;
            List<TrackDetail> tempList = trackDetailList.Distinct(new TrackDetailComparer()).ToList();
            var filteredTrackList = from td in tempList where td.PostalCode != string.Empty select td;
            foreach(var td in filteredTrackList)
            {
                postalCode = td.PostalCode;
                countryCode = td.CountryCode;
                XmlDocument searchResponse = Geocode(postalCode, countryCode);
                coordinateList.Add(ConvertLocationToCoordinate(searchResponse));
            }
            AddPushpinToMap();
            AddPolyline();
            myMap.Visibility = Visibility.Visible;
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
                pushpin.Content = "" +i++;
                pushpin.Location = new Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
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
                locationCollection.Add(new Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude)));
            }
            polyline.Locations = locationCollection;
            myMap.Children.Add(polyline);
        }
        // Geocode an address and return a latitude and longitude
        public XmlDocument Geocode(string postCode,string countryCode)
        {
            //Create REST Services geocode request using Locations API
            string geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/"+ postCode + "?o=xml&key=" + BingMapsKey;

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
            string latitude=string.Empty; 
            string longitude=string.Empty;
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
            return new Coordinate(latitude, longitude);
        }


        //Add label element to application
        private void AddLabel(Panel parent, string labelString)
        {
            /*
            Label dname = new Label();
            dname.Content = labelString;
            dname.Style = (Style)FindResource("AddressStyle");
            parent.Children.Add(dname);
            //*/
        }

        

        //Show the POI address information and insert pushpins on the map
        private void DisplayResults(XmlDocument nearbyPOI)
        {
            
            XmlNamespaceManager nsmgr = new XmlNamespaceManager(nearbyPOI.NameTable);
            nsmgr.AddNamespace("d", "http://schemas.microsoft.com/ado/2007/08/dataservices");
            nsmgr.AddNamespace("m", "http://schemas.microsoft.com/ado/2007/08/dataservices/metadata");
            nsmgr.AddNamespace("a", "http://www.w3.org/2005/Atom");

            //Get the the entityID for each POI entity in the response
            XmlNodeList displayNameList = nearbyPOI.SelectNodes("//d:DisplayName", nsmgr);

            //Provide entity information and put a pushpin on the map.
            if (displayNameList.Count == 0)
            {
                //ErrorMessage.Content = "No results were found for this location.";
                //ErrorMessage.Visibility = Visibility.Visible;
            }
            else
            {
                XmlNodeList addressLineList = nearbyPOI.SelectNodes("//d:AddressLine", nsmgr);
                XmlNodeList localityList = nearbyPOI.SelectNodes("//d:Locality", nsmgr);
                XmlNodeList adminDistrictList = nearbyPOI.SelectNodes("//d:AdminDistrict", nsmgr);
                XmlNodeList postalCodeList = nearbyPOI.SelectNodes("//d:PostalCode", nsmgr);
                XmlNodeList latitudeList = nearbyPOI.SelectNodes("//d:Latitude", nsmgr);
                XmlNodeList longitudeList = nearbyPOI.SelectNodes("//d:Longitude", nsmgr);
                for (int i = 0; i < displayNameList.Count; i++)
                {
                    /*
                    AddLabel(AddressList, "[" + Convert.ToString(i + 1) + "] " + displayNameList[i].InnerText);
                    AddLabel(AddressList, addressLineList[i].InnerText);
                    AddLabel(AddressList, localityList[i].InnerText + ", " + adminDistrictList[i].InnerText);
                    AddLabel(AddressList, postalCodeList[i].InnerText);
                    AddLabel(AddressList, "");
                    */
                    //AddPushpinToMap(Convert.ToDouble(latitudeList[i].InnerText), Convert.ToDouble(longitudeList[i].InnerText), Convert.ToString(i + 1));
                }
                //SearchResults.Visibility = Visibility.Visible;
                myMap.Visibility = Visibility.Visible;
                //myMapLabel.Visibility = Visibility.Visible;
                myMap.Focus(); //allows '+' and '-' to zoom the map
            }
            
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

    }
}
