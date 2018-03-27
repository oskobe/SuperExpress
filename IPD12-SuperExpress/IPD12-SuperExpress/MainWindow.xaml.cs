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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
     

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            Register dlg = new Register();
            dlg.ShowDialog();
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            MainDialog dlg = new MainDialog();
            dlg.ShowDialog();
        }
    }
}
