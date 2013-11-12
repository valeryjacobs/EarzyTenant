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

namespace EarzyProvisioning
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        EarzyProvisioningCore _earzyProvisioningCore;
        public MainWindow()
        {
            InitializeComponent();

            _earzyProvisioningCore = new EarzyProvisioningCore();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
          
           await _earzyProvisioningCore.CreateEarzyForTenant(tenantBox.Text.Replace(" ", ""));
        }
    }
}
