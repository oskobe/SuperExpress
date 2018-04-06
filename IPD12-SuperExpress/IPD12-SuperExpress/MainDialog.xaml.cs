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
using System.Device.Location;

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
        List<Pushpin> pushpinList = new List<Pushpin>();
        Pushpin shipMapPushpin;
        MapPolyline currentPolyline;
        static int earthRadius = 6367;
        double BestZoomLevel = 12;
        Microsoft.Maps.MapControl.WPF.Location BestCenter = new Microsoft.Maps.MapControl.WPF.Location(45.404761, -73.9448513);
        //double distance;
        private string BingMapsKey = "AuqsNVXfKfPx5B6juGoyi9rYuEZkIkYns-8GRbMbrx3BnhxpT5KsRNrRUgbyOpsm";
        private readonly OpenWeatherMapClient OpenWeatherMapTestClient = new OpenWeatherMapClient("23a61d3a72f546a7a1659131fb9499c0");

        public int currentCount = 0;

        System.Timers.Timer clickTimer;

        private void InitTimer()
        {
            int interval = 1;
            clickTimer = new System.Timers.Timer(interval);
            clickTimer.AutoReset = true;
            clickTimer.Enabled = true;
            clickTimer.Elapsed += new System.Timers.ElapsedEventHandler(TimerUp);
        }
        private void ResetTimer()
        {
            currentCount = 0;
        }

        private void TimerUp(object sender, System.Timers.ElapsedEventArgs e)
        {
            currentCount++;
        }

        private void DisplayBestView()
        {
            myMap.Center = BestCenter;
            myMap.ZoomLevel = BestZoomLevel;
            myMap.Focus();
        }
        public MainDialog()
        {
            InitializeComponent();
            InitTimer();
            DisplayBestView();
            AddPushpinToMap(new Coordinate(BestCenter.Latitude, BestCenter.Longitude), "H", 1);
            shipMap.Center = BestCenter;
            shipMap.ZoomLevel = BestZoomLevel;
            try
            {
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

        public static CurrentWeatherResponse MakeWeatherRequest(string requestUrl)
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
                    CurrentWeatherResponse objResponse = (CurrentWeatherResponse)jsonSerializer.ReadObject(response.GetResponseStream());
                    return objResponse;
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
        private double RadtoDeg(double x)
        {
            return x * 180 / Math.PI;
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
            /* .NET Framework 4 and higher support to calculate distance between two coordinates(just need to reference the System.Device.dll)
            var sCoord = new GeoCoordinate(a.Latitude, a.Longitude);
            var eCoord = new GeoCoordinate(b.Latitude, b.Longitude);
            double aa = sCoord.GetDistanceTo(eCoord);
            */
            return earthRadius * centralAngle;
        }
        private double CaculateMaxDistance()
        {
            int locNum = coordinateList.Count();
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
                    }
                }
                return distanceList2.Max();
            }
            return 0.0;
        }
        private void CleanPushpinAndPolylineOfMap()
        {
            myMap.Children.Remove(currentPolyline);
            foreach (Pushpin p in pushpinList)
            {
                myMap.Children.Remove(p);
            }
            currentPolyline = null;
            pushpinList.Clear();
        }
        private int getZoomLevel(double distance)
        {
            // reference:https://msdn.microsoft.com/en-us/library/aa940990.aspx

            if (distance > 9000)
            {
                return 1;
            }
            else if (distance > 4500)
            {
                return 2;
            }
            else if (distance > 2250)
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

        private Coordinate GetCentralGeoCoordinate(IList<Coordinate> geoCoordinates)
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
                //Given the values location in the list:Convert Lat and Lon from degrees to radians.
                var latitude = DegtoRad(geoCoordinate.Latitude);
                var longitude = DegtoRad(geoCoordinate.Longitude);
                //Convert lat/ lon to Cartesian coordinates.

                x += Math.Cos(latitude) * Math.Cos(longitude);
                y += Math.Cos(latitude) * Math.Sin(longitude);
                z += Math.Sin(latitude);
            }
            //Compute  average x, y and z coordinates.
            var total = geoCoordinates.Count;
            x = x / total;
            y = y / total;
            z = z / total;
            //Convert average x, y, z coordinate to latitude and longitude. 
            var centralLongitude = Math.Atan2(y, x);
            var centralSquareRoot = Math.Sqrt(x * x + y * y);
            var centralLatitude = Math.Atan2(z, centralSquareRoot);
            //Convert lat and lon to degrees.
            return new Coordinate(RadtoDeg(centralLatitude), RadtoDeg(centralLongitude));
        }
        private void TestShowShipmentRouteOnMap()
        {
            string postalCode = string.Empty;
            string countryCode = string.Empty;
            string cityName = string.Empty;
            coordinateList.Clear();
            //remove the distinct location
            List<TrackDetail> tempList = trackDetailList.Distinct(new TrackDetailComparer()).ToList();
            //remove the location who has no city name or countrycode
            var templist = from td in tempList where td.CountryCode != string.Empty && td.City != string.Empty select td;
            filteredtrackDetailList = templist.Reverse().ToList();
            foreach (var td in filteredtrackDetailList)
            {
                postalCode = td.PostalCode;
                countryCode = td.CountryCode;
                cityName = td.City;
                Coordinate tempCoordinate = GetCoordinate(postalCode, countryCode, cityName);
                if (tempCoordinate != null)
                {
                    coordinateList.Add(tempCoordinate);
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
            double maxDistance = CaculateMaxDistance();
            Coordinate center = GetCentralGeoCoordinate(coordinateList);

            if (center != null)
            {
                BestCenter = new Microsoft.Maps.MapControl.WPF.Location(center.Latitude, center.Longitude);
                myMap.Center = BestCenter;
            }
            BestZoomLevel = getZoomLevel(maxDistance);
            myMap.ZoomLevel = BestZoomLevel;
            myMap.Focus(); //allows '+' and '-' to zoom the map
        }
        private void ShowShipmentRouteOnMap()
        {
            string postalCode = string.Empty;
            string countryCode = string.Empty;
            string cityName = string.Empty;
            coordinateList.Clear();
            //remove the distinct location
            List<TrackDetail> tempList = trackDetailList.Distinct(new TrackDetailComparer()).ToList();
            //remove the location who has no city name or countrycode
            var templist = from td in tempList where td.CountryCode != string.Empty && td.City != string.Empty select td;
            filteredtrackDetailList = templist.Reverse().ToList();
            foreach (var td in filteredtrackDetailList)
            {
                postalCode = td.PostalCode;
                countryCode = td.CountryCode;
                cityName = td.City;
                Coordinate tempCoordinate = GetCoordinate(postalCode, countryCode, cityName);
                if (tempCoordinate != null)
                {
                    coordinateList.Add(tempCoordinate);
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
            double maxDistance = CaculateMaxDistance();
            Coordinate center = GetCentralGeoCoordinate(coordinateList);

            if (center != null)
            {
                BestCenter = new Microsoft.Maps.MapControl.WPF.Location(center.Latitude, center.Longitude);
                myMap.Center = BestCenter;
            }
            BestZoomLevel = getZoomLevel(maxDistance);
            myMap.ZoomLevel = BestZoomLevel;
            myMap.Focus(); //allows '+' and '-' to zoom the map
        }

        private string GetWeatherByDateURL(Coordinate cd, DateTime dt)
        {
            return string.Format("http://api.openweathermap.org/data/2.5/weather?lat={0}&lon={1}&APPID=23a61d3a72f546a7a1659131fb9499c0", cd.Latitude, cd.Longitude);
            //return string.Format("http://history.openweathermap.org/data/2.5/history/city?lat={0}&lon={1}&type=day&start={2}&end={3}&APPID=23a61d3a72f546a7a1659131fb9499c0", cd.Latitude, cd.Longitude, dt.Ticks/((long)10000*1000), dt.AddDays(1).Hour);
        }
        private void FillWeatherInfoLable(CurrentWeatherResponse result, TrackDetail td)
        {
            Color c;
            c = Colors.GreenYellow;
            if (result.Temperature.Value <= Globals.VERY_COLD || result.Wind.Speed.Value >= Globals.SPEECH_HURRICANE)
            {
                c = Colors.Red;//if there is a very terrible weather at current location,alert it with a red color.
            }
            c.A = 100;
            SolidColorBrush scb = new SolidColorBrush(c);
            string iconURL = "http://openweathermap.org/img/w/" + result.Weather.Icon + ".png";
            maindlg_imgDescription.Source = new BitmapImage(new Uri(iconURL));
            mainDlg_lbCountry.Background = scb;
            mainDlg_lbCountry.Content = result.City.Country;
            mainDlg_lbCity.Background = scb;
            mainDlg_lbCity.Content = result.City.Name;
            mainDlg_lbTemp.Background = scb;
            mainDlg_lbTemp.Content = Math.Round(result.Temperature.Value - 273.15) + "°C";
            mainDlg_lbWindy.Background = scb;
            mainDlg_lbWindy.Content = result.Wind.Speed.Value + "m/s";
            mainDlg_lbDescription.Background = scb;
            mainDlg_lbDescription.Content = result.Weather.Value;
            mainDlg_lbDate.Background = scb;
            mainDlg_lbDate.Content = td.Date;
        }
        private async Task FillWeatherInfoBoxAsync(Pushpin p)
        {
            string strIndex = p.Content.ToString();
            int index = int.Parse(strIndex);
            TrackDetail td = filteredtrackDetailList.ElementAt(index - 1);
            CurrentWeatherResponse result = await OpenWeatherMapTestClient.CurrentWeather.GetByCoordinates(new Coordinates { Latitude = p.Location.Latitude, Longitude = p.Location.Longitude });
            FillWeatherInfoLable(result, td);
        }
        private void Pushpin_MouseEnter(object sender, MouseEventArgs e)
        {
            FillWeatherInfoBoxAsync((Pushpin)sender);
        }
        private async Task FillCurrentWeatherInfoBoxAsync(Pushpin p, TrackDetail td)
        {
            CurrentWeatherResponse result = await OpenWeatherMapTestClient.CurrentWeather.GetByCoordinates(new Coordinates { Latitude = p.Location.Latitude, Longitude = p.Location.Longitude });
            FillWeatherInfoLable(result, td);
        }
        //Add a pushpin and a label with weather information to the map
        private async Task AddCurrentWeatherInfoToMap(Pushpin cd, TrackDetail td)
        {
            MapLayer labelLayer = new MapLayer();
            System.Windows.Controls.Label customLabel = new System.Windows.Controls.Label();
            CurrentWeatherResponse result = await OpenWeatherMapTestClient.CurrentWeather.GetByCoordinates(new Coordinates { Latitude = cd.Location.Latitude, Longitude = cd.Location.Longitude });
            Color c;
            customLabel.Content = string.Format("Country:{0},City:{1},{2}°C", result.City.Country, td.City, Math.Round(result.Temperature.Value - 273.15));
            c = Colors.GreenYellow;

            if (result.Temperature.Value <= Globals.VERY_COLD || result.Wind.Speed.Value >= Globals.SPEECH_HURRICANE)
            {
                c = Colors.Red;//if there is a very terrible weather at current location,alert it with a red color.
            }

            c.A = 100;
            SolidColorBrush scb = new SolidColorBrush(c);
            customLabel.Background = scb;
            // With map layers we can add WPF children to lat long (WPF Location obj) on the map.                
            labelLayer.AddChild(customLabel, new Microsoft.Maps.MapControl.WPF.Location(cd.Location.Latitude, cd.Location.Longitude));
            myMap.Children.Add(labelLayer);
            FillWeatherInfoLable(result, td);
        }
        private void AddPushpinToMap(Coordinate cd, string content, int flag)
        {
            Pushpin pushpin = new Pushpin();
            pushpin.MouseEnter += new MouseEventHandler(Pushpin_MouseEnter);
            pushpin.Content = content;
            pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
            if (flag == 1)
            {
                pushpinList.Add(pushpin);
                myMap.Children.Add(pushpin);
            }
            else
            {
                shipMap.Children.Remove(shipMapPushpin);
                shipMapPushpin = pushpin;
                shipMap.Children.Add(shipMapPushpin);
                if (flag == 3)
                {
                    shipMap.ZoomLevel = 15;
                    shipMap.Center = pushpin.Location;
                }
            }


        }
        private void AddPushpinAndWeatherInfoToMap()
        {
            int i = 1;
            int count = coordinateList.Count();
            Coordinate cd;
            for (int j = 0; j < count; j++)
            {
                cd = coordinateList.ElementAt(j);
                Pushpin pushpin = new Pushpin();
                pushpin.MouseEnter += new MouseEventHandler(Pushpin_MouseEnter);
                pushpin.Content = "" + i++;
                pushpin.Location = new Microsoft.Maps.MapControl.WPF.Location(Convert.ToDouble(cd.Latitude), Convert.ToDouble(cd.Longitude));
                if (j == count - 1)
                {
                    TrackDetail td = filteredtrackDetailList.ElementAt(j);
                    FillCurrentWeatherInfoBoxAsync(pushpin, td);
                    AddCurrentWeatherInfoToMap(pushpin, td);
                }
                pushpinList.Add(pushpin);
                myMap.Children.Add(pushpin);
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
            currentPolyline = polyline;
            myMap.Children.Add(polyline);
        }

        public Coordinate GetCoordinate(string postCode, string countryCode, string cityName)
        {
            string geocodeRequest = string.Empty;
            //Create REST Services geocode request using Locations API
            if (postCode != string.Empty)
            {
                geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + postCode + @"?key=" + BingMapsKey;
            }
            else
            {
                geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + countryCode + @"/" + cityName + @"?key=" + BingMapsKey;
            }
            //Make the request and get the response
            Response geocodeResponse = MakeRequest(geocodeRequest);
            BingMapsRESTToolkit.Location l = (BingMapsRESTToolkit.Location)geocodeResponse.ResourceSets[0].Resources[0];
            if (l != null)
            {
                return new Coordinate(l.Point.GetCoordinate().Latitude, l.Point.GetCoordinate().Longitude);
            }
            return null;
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
            CleanPushpinAndPolylineOfMap();
            ShowShipmentRouteOnMap();
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



        private void imgFocus_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DisplayBestView();
        }

        private void shipTab_btnSearch_Click(object sender, RoutedEventArgs e)
        {
            string postalCode = shipTab_tbPostalCode.Text;
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                MessageBox.Show("Please enter an PostalCode!", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                shipTab_tbPostalCode.Focus();
                return;
            }
            postalCode = postalCode.Replace(" ", string.Empty);
            string geocodeRequest;
            //Create REST Services geocode request using Locations API

            geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + postalCode + @"?key=" + BingMapsKey;
            Response geocodeResponse = MakeRequest(geocodeRequest);
            BingMapsRESTToolkit.Location l = (BingMapsRESTToolkit.Location)geocodeResponse.ResourceSets[0].Resources[0];
            Address shipToAddress = new Address();
            if (l != null)
            {
                shipToAddress = new Address();
                shipToAddress = l.Address;
                string resultPostalCode = shipToAddress.PostalCode;//正确的大写PostalCode
                string resultAddressLine = shipToAddress.AddressLine;//空值
                string resultAdminDistrict = shipToAddress.AdminDistrict;//省代码
                string resultAdminDistrict2 = shipToAddress.AdminDistrict2;//城市
                string resultCountryRegion = shipToAddress.CountryRegion;//国家全名
                string resultLocality = shipToAddress.Locality;//城市

                Coordinate cd = new Coordinate(l.Point.GetCoordinate().Latitude, l.Point.GetCoordinate().Longitude);
                AddPushpinToMap(cd, "T", 3);
            }
        }

        private void shipMap_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            clickTimer.Stop();
            if (currentCount < 10)
            {
                System.Windows.Point mousePosition = e.GetPosition(shipMap);
                Microsoft.Maps.MapControl.WPF.Location pinLocation = shipMap.ViewportPointToLocation(mousePosition);
                AddPushpinToMap(new Coordinate(pinLocation.Latitude, pinLocation.Longitude), "T", 2);
                string geocodeRequest = @"http://dev.virtualearth.net/REST/v1/Locations/" + pinLocation.Latitude + "," + pinLocation.Longitude + @"?key=" + BingMapsKey;
                Response geocodeResponse = MakeRequest(geocodeRequest);
                BingMapsRESTToolkit.Location l = (BingMapsRESTToolkit.Location)geocodeResponse.ResourceSets[0].Resources[0];
                Address shipToAddress;
                if (l != null)
                {
                    shipToAddress = new Address();
                    shipToAddress = l.Address;
                    string resultPostalCode = shipToAddress.PostalCode;//正确的大写PostalCode
                    string resultAddressLine = shipToAddress.AddressLine;//空值
                    string resultAdminDistrict = shipToAddress.AdminDistrict;//省代码
                    string resultAdminDistrict2 = shipToAddress.AdminDistrict2;//城市
                    string resultCountryRegion = shipToAddress.CountryRegion;//国家全名
                    string resultLocality = shipToAddress.Locality;//城市
                }
            }
            ResetTimer();
        }

        private void shipMap_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            clickTimer.Start();
        }

        private void myMap_MouseMove(object sender, MouseEventArgs e)
        {
            System.Windows.Point viewportPoint = e.GetPosition(myMap);
            Microsoft.Maps.MapControl.WPF.Location location;
            if (myMap.TryViewportPointToLocation(viewportPoint, out location))
            {
                Coords.Text = String.Format("Coordinate: {0:f6},{1:f6}", location.Longitude, location.Latitude);
            }
        }
    }
}
