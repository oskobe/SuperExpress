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
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Client;
using ShipEngine.ApiClient.Model;

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for ShippingCostCalculatorResult.xaml
    /// </summary>
    public partial class ShippingCostCalculatorResult : Window
    {
        List<SuperExpressRate> rates = new List<SuperExpressRate>();

        public ShippingCostCalculatorResult(CostCalculator calculator)
        {
            InitializeComponent();
            InitializeCostCalculatorResult(calculator);
        }

        private void InitializeCostCalculatorResult(CostCalculator cal)
        {
            
            List<Rate> result = cal.Result;
            // Prepare data for listview
            foreach(Rate r in result)
            {
                SuperExpressRate rate = new SuperExpressRate();
                rate.IsSelected = false;
                rate.ServiceType = r.ServiceType;
                rate.GuaranteedService = r.GuaranteedService??false;
                rate.DeliveryDays = r.DeliveryDays??0;
                rate.EstimatedDeliveryDateTime = r.EstimatedDeliveryDate??DateTime.Now;
                rate.Amount = r.ShippingAmount.Amount??0;
                rates.Add(rate);
            }

            // Set data to the calculator result window
            lblCityProvinceFrom.Content = string.Format("{0} {1}", cal.CityFrom, cal.ProvinceFrom);
            lblCountryFrom.Content = cal.CountryFrom;
            lblPostalCodeFrom.Content = cal.PostalCodeFrom;

            lblCityProvinceTo.Content = string.Format("{0} {1}", cal.CityTo, cal.ProvinceTo);
            lblCountryTo.Content = cal.CountryTo;
            lblPostalCodeTo.Content = cal.PostalCodeTo;

            lblWeight.Content = string.Format("{0:0.00} {1}", cal.Weight.Value, cal.Weight.Unit);
            lblDimensions.Content = string.Format("{0:0.0} * {1:0.0} * {2:0.0} {3}", cal.Dimensions.Length, cal.Dimensions.Width, cal.Dimensions.Height, cal.Dimensions.Unit);

            // Sort by the amount
            rates = (from r in rates orderby r.Amount select r).ToList();
            // Set listview data source
            lvShippingCostCalculatorResult.ItemsSource = rates;
        }

        private void lvShippingCostCalculatorResult_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check on radio button when any item selected
            var rate = rates.Find(r => r.IsSelected == true);
            if (rate != null)
            {
                rate.IsSelected = false;
            }
            rates[lvShippingCostCalculatorResult.SelectedIndex].IsSelected = true;
            lvShippingCostCalculatorResult.Items.Refresh();
        }

        private void rbOption_Checked(object sender, RoutedEventArgs e)
        {
            // Select one line when the corresponding radio button checked
            RadioButton radioButton = sender as RadioButton;
            lvShippingCostCalculatorResult.SelectedItem = radioButton.DataContext;
        }
    }
}
