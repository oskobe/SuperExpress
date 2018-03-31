using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for Register.xaml
    /// </summary>
    public partial class Register : Window
    {
        string emailExpression = @"^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$";
        public Register()
        {
            InitializeComponent();
        }        

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string name = regDlg_tbName.Text;
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Please enter your name");
                regDlg_tbName.Text = "";
                regDlg_tbName.Focus();
                return;
            }
            
            string email = regDlg_tbEmail.Text;
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter an email");
                regDlg_tbEmail.Text = "";
                regDlg_tbEmail.Focus();
                return;
            }
            MatchCollection mc = Regex.Matches(email, emailExpression);
            if (mc.Count == 0)
            {
                MessageBox.Show("Please enter an valid email(example@gmail.com)");
                regDlg_tbEmail.Text = "";
                regDlg_tbEmail.Focus();
                return;
            }
            long phone;
            if (!long.TryParse(regDlg_tbPhone.Text, out phone))
            {
                MessageBox.Show("Please enter a digit number(11 digit)");
                regDlg_tbPhone.Text = "";
                regDlg_tbPhone.Focus();
                return;
            }
            string postalCode = regDlg_tbPostalCode.Text;
            if (string.IsNullOrWhiteSpace(postalCode))
            {
                MessageBox.Show("Please enter a postalCode");
                regDlg_tbPostalCode.Text = "";
                regDlg_tbPostalCode.Focus();
                return;
            }
            string countryCode = regDlg_cbxCountry.SelectedValue.ToString();
            string provinceCode = regDlg_cbxProvince.SelectedValue.ToString();
            string cityName =((ComboBoxItem)regDlg_cbxCity.SelectedItem).Content.ToString();
            string streetName = regDlg_tbStreet.Text;
            if (string.IsNullOrWhiteSpace(streetName))
            {
                MessageBox.Show("Please enter a street name");
                regDlg_tbStreet.Text = "";
                regDlg_tbStreet.Focus();
                return;
            }

            string apartment = regDlg_tbApt.Text;
            if (string.IsNullOrWhiteSpace(apartment))
            {
                MessageBox.Show("Please enter a street name");
                regDlg_tbApt.Text = "";
                regDlg_tbApt.Focus();
                return;
            }
            string Password1 = regDlg_pbPassword.Password.ToString();
            string Password2 = regDlg_pbConfirm.Password.ToString();
            if (string.IsNullOrWhiteSpace(Password1))
            {
                MessageBox.Show("Please enter a password");
                regDlg_pbPassword.Password = "";
                regDlg_pbConfirm.Password = "";
                regDlg_pbPassword.Focus();
                return;
            }
            if (Password1.CompareTo(Password2) != 0)
            {
                MessageBox.Show("Two password are not matched!");
                regDlg_pbPassword.Password = "";
                regDlg_pbConfirm.Password = "";
                regDlg_pbPassword.Focus();
                return;
            }
            try
            {
                //Globals.db.AddUser(new User(1, name, phone, email, postalCode, countryCode, provinceCode, cityName, streetName, apartment, Password1));
                Globals.db.AddUser(new User(1, name, phone, email, postalCode, "Ca", "QC", cityName, streetName, apartment, Password1));
                MessageBox.Show("Add user Successfully!");
                this.Close();
            }
            catch(MySqlException ex)
            {
                MessageBox.Show("Add user failure!");
            }
            
        }
    }
}
