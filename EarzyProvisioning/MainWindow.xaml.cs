using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        string _tenantName;
        string _accountId;
        string _siteName;
        public MainWindow()
        {
            InitializeComponent();

            _earzyProvisioningCore = new EarzyProvisioningCore();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            _tenantName = tenantBox.Text.Replace(" ", "");
            _accountId = Guid.NewGuid().ToString();
            _siteName = "EarzyFor" + _tenantName;
            await _earzyProvisioningCore.CreateEarzyForTenantSTEPA(_tenantName, _siteName, _accountId);

            Process.Start(@"C:\Windows\System32\WindowsPowerShell\v1.0\powershell_ise.exe", string.Format(@"C:\\TopSecret\\TenantScript{0}.ps1", _siteName));
        }

        private async void Button_ClickB(object sender, RoutedEventArgs e)
        {
            if (_tenantName != null && _siteName != null && _siteName != null)
                await _earzyProvisioningCore.CreateEarzyForTenantSTEPB(_tenantName, _siteName, _accountId);
        }
    }
}
