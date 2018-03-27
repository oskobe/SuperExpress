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

namespace IPD12_SuperExpress
{
    /// <summary>
    /// Interaction logic for MainDialog.xaml
    /// </summary>
    public partial class MainDialog : Window
    {
        public MainDialog()
        {
            InitializeComponent();
        }

        private void btnTracking_Click(object sender, RoutedEventArgs e)
        {
            Tracking dlg = new Tracking();
            dlg.ShowDialog();
        }
    }
}
