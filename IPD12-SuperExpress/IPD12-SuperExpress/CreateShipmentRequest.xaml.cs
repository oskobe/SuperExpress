﻿using System;
using System.Collections.Generic;
using System.IO;
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
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using MySql.Data;
using MySql.Data.MySqlClient;
using ShipEngine.ApiClient.Api;
using ShipEngine.ApiClient.Client;
using ShipEngine.ApiClient.Model;

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for CreateShipmentRequest.xaml
    /// </summary>
    public partial class CreateShipmentRequest : Window
    {
        List<Country> countryList = new List<Country>();
        List<Province> provinceList = new List<Province>();
        CostCalculator costCalculator;
        SuperExpressRate rate;
        ShipmentRequest shipmentRequest;

        public CreateShipmentRequest(CostCalculator cal, SuperExpressRate rate)
        {
            try
            {
                InitializeComponent();
                InitializeDataFromDatabase();
                costCalculator = cal;
                this.rate = rate;
                InitializeShipmentRequest();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("Error opening database connection: " + ex.Message);
                Environment.Exit(1);

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

        private void InitializeShipmentRequest()
        {
            //List<String> countryNameList = (from country in countryList orderby country.Name select country.Name).ToList();
            lblServiceType.Content = rate.ServiceType;
            lblGuaranteedService.Content = rate.Guaranteed;
            lblEstimatedDate.Content = rate.EstimatedDeliveryDateTimeStr;
            lblWeight.Content = costCalculator.WeightStr;
            lblDimensions.Content = costCalculator.DimensionsStr;
            lblAmount.Content = rate.AmountStr + Globals.CURRENCY_CAD;

            cbCountryFrom.ItemsSource = countryList;//countryNameList;
            cbCountryFrom.Text = costCalculator.CountryFrom.Name;
            cbProvinceStateFrom.Text = costCalculator.ProvinceFrom.ProvinceStateName;
            tbCityFrom.Text = costCalculator.CityFrom;
            tbPostalCodeFrom.Text = costCalculator.PostalCodeFrom;

            cbCountryTo.ItemsSource = countryList;
            cbCountryTo.Text = costCalculator.CountryTo.Name;
            cbProvinceStateTo.Text = costCalculator.ProvinceTo.ProvinceStateName;
            tbCityTo.Text = costCalculator.CityTo;
            tbPostalCodeTo.Text = costCalculator.PostalCodeTo;


            /* for test start*/
            tbAddressFrom1.Text = "4100 Rue Goyer";
            tbAddressTo1.Text = "3-9992 Av Fontenac";
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

        private void btCreate_Click(object sender, RoutedEventArgs e)
        {
            if (tbAddressFrom1.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please enter the origin address1.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (tbAddressTo1.Text.Trim().Equals(string.Empty))
            {
                MessageBox.Show("Please enter the destination address1.", "Input error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            shipmentRequest = new ShipmentRequest();
            shipmentRequest.ServiceType = rate.ServiceType;
            shipmentRequest.GuaranteedService = rate.GuaranteedService;
            shipmentRequest.EstimatedDeliveryDate = rate.EstimatedDeliveryDateTime;
            shipmentRequest.Weight = costCalculator.Weight.Value??0;
            shipmentRequest.WeightUnit = costCalculator.Weight.Unit.ToString();
            shipmentRequest.Length = costCalculator.Dimensions.Length??0;
            shipmentRequest.Width = costCalculator.Dimensions.Width ?? 0;
            shipmentRequest.Height = costCalculator.Dimensions.Height ?? 0;
            shipmentRequest.DimensionsUnit = costCalculator.Dimensions.Unit.ToString();
            shipmentRequest.Amount = rate.Amount;
            shipmentRequest.Currency = Globals.CURRENCY_CAD.Trim();
            shipmentRequest.CountryFrom = cbCountryFrom.Text;
            shipmentRequest.ProvinceFrom = cbProvinceStateFrom.Text;
            shipmentRequest.CityFrom = tbCityFrom.Text;
            shipmentRequest.Address1From = tbAddressFrom1.Text;
            shipmentRequest.Address2From = tbAddressFrom2.Text;
            shipmentRequest.PostalCodeFrom = tbPostalCodeFrom.Text;
            shipmentRequest.CountryTo = cbCountryTo.Text;
            shipmentRequest.ProvinceTo = cbProvinceStateTo.Text;
            shipmentRequest.CityTo = tbCityTo.Text;
            shipmentRequest.Address1To = tbAddressTo1.Text;
            shipmentRequest.Address2To = tbAddressTo2.Text;
            shipmentRequest.PostalCodeTo = tbPostalCodeTo.Text;

            try
            {
                string newGeneratedId = Globals.db.AddOrder(shipmentRequest);
                MessageBoxResult result = MessageBox.Show("Shipment request #" + newGeneratedId + " was created, \nDo you want to save PDF format INVOICE?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
                if (result == MessageBoxResult.Yes)
                {
                    PrintPdfInvoice(newGeneratedId);
                }

            } catch (MySqlException ex)
            {
                Console.WriteLine(ex.StackTrace);
                MessageBox.Show("Error writing shipment request into database: \n" + ex.Message, "Database writing error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintPdfInvoice(string invoiceNo)
        {
            Stream myStream;
            SaveFileDialog file = new SaveFileDialog();
            file.Filter = "pdf files (*.pdf)|*.pdf|All files (*.*)|*.*"; ;
            file.FilterIndex = 1;
            file.FileName = Globals.SHIPPING_INVOICE + invoiceNo;
            file.RestoreDirectory = true;
            var result = file.ShowDialog();

            if (result.HasValue && result.Value)
            {
                string localFilePath = file.FileName.ToString();
                string fileNameExt = localFilePath.Substring(localFilePath.LastIndexOf("\\") + 1);
                myStream = file.OpenFile();

                Document document = new Document(PageSize.LETTER);
                PdfWriter writer = PdfWriter.GetInstance(document, myStream);
                document.Open();

                Font bold_big = new Font(Font.FontFamily.TIMES_ROMAN, 12, Font.BOLD | Font.UNDERLINE);
                Font bold = new Font(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
                Font normal = new Font(Font.FontFamily.TIMES_ROMAN, 10);

                iTextSharp.text.Paragraph title = new iTextSharp.text.Paragraph("SHIPPING INVOICE", bold_big);
                title.Alignment = iTextSharp.text.Rectangle.ALIGN_CENTER;
                document.Add(title);

                PdfPTable table = new PdfPTable(6);

                PdfPCell cell = new PdfPCell(new Phrase(" \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                cell.HorizontalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Invoice No.", bold));
                cell.Border = 0;
                cell.Colspan = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(invoiceNo, normal));
                cell.Border = 0;
                cell.Colspan = 5;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Sender:", bold));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Recipient:", bold));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);
                // FIXED ME
                cell = new PdfPCell(new Phrase("David Lee", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);
                // FIXED ME
                cell = new PdfPCell(new Phrase("Brain Jobs", normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.Address1From + " " + shipmentRequest.Address2From, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.Address1To + " " + shipmentRequest.Address2To, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.CityFrom + " " + shipmentRequest.ProvinceFrom + ", " + shipmentRequest.CountryFrom, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.CityTo + " " + shipmentRequest.ProvinceTo + ", " + shipmentRequest.CountryTo, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.PostalCodeFrom, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(shipmentRequest.PostalCodeTo, normal));
                cell.Border = 0;
                cell.Colspan = 3;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Service Type", bold));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Guaranteed Service", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Delivery Days", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Estimated Delivery Date", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Amount", bold));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.ServiceType, normal));
                cell.Colspan = 2;
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.Guaranteed, normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.DeliverDaysStr, normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.EstimatedDeliveryDateTimeStr, normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.AmountStr + Globals.CURRENCY_CAD, normal));
                cell.HorizontalAlignment = 1;
                cell.VerticalAlignment = 1;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(" \n \n \n \n "));
                cell.Border = 0;
                cell.Colspan = 6;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("SUB TOTAL", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase(rate.AmountStr + Globals.CURRENCY_CAD, normal));
                cell.Border = 0;
                //cell.Colspan = 1;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("SERVICE TAX (15%)", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                double tax = rate.Amount * 0.15;
                cell = new PdfPCell(new Phrase(string.Format("{0:0.00}{1}", tax, Globals.CURRENCY_CAD), normal));
                cell.Border = 0;
                //cell.Colspan = 1;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("TOTAL", bold));
                cell.Border = 0;
                cell.Colspan = 5;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                double total = rate.Amount + tax;
                cell = new PdfPCell(new Phrase(string.Format("{0:0.00}{1}", total, Globals.CURRENCY_CAD), normal));
                cell.Border = 0;
                //cell.Colspan = 1;
                cell.HorizontalAlignment = 2;
                table.AddCell(cell);

                document.Add(table);

                document.Close();
                myStream.Close();


                //gridBill.item
                Grid myGrid = new Grid();

            }
        }
    }
}
