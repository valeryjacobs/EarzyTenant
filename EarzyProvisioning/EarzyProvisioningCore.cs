using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Models;
using Microsoft.WindowsAzure.Management.Storage;
using Microsoft.WindowsAzure.Management.Storage.Models;
using Microsoft.WindowsAzure.Management.WebSites;
using Microsoft.WindowsAzure.Management.WebSites.Models;
using Octokit;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace EarzyProvisioning
{
    //Manage Earzy environments
    class EarzyProvisioningCore : IDisposable
    {
        //Use these management client
        private StorageManagementClient _storageManagementClient;
        private ComputeManagementClient _computeManagementClient;
        private WebSiteManagementClient _webSitesManagementClient;

        const string _webspaceName = "westeuropewebspace";

        #region Construct

        public EarzyProvisioningCore()
        {
            //Get credentials from subscription ID & Certificate
            var credential = CertificateAuthenticationHelper.GetCredentials(
                ConfigurationManager.AppSettings["SubscriptionId"],
                ConfigurationManager.AppSettings["Certificate"]);

            //Initialize clients
            _storageManagementClient = CloudContext.Clients.CreateStorageManagementClient(credential);
            _computeManagementClient = CloudContext.Clients.CreateComputeManagementClient(credential);
            _webSitesManagementClient = CloudContext.Clients.CreateWebSiteManagementClient(credential);
        }

        #endregion

        #region Methods

        //Create an instance of the Earzy App for a tenant.
        internal async Task CreateEarzyForTenantSTEPA(string tenantName,string siteName, string accountId)
        {
            await CreateStorageAccount(LocationNames.WestEurope, siteName.ToLower());

            //Create web site
            //var response = await _webSitesManagementClient.WebSites.CreateAsync(webspaceName,
            //    new WebSiteCreateParameters
            //    {
            //        ComputeMode = WebSiteComputeMode.Shared,
            //        Name = name,
            //        SiteMode = WebSiteMode.Limited,
            //        HostNames = { name + ".azurewebsites.net" },
            //        WebSpaceName = webspaceName,
            //    });

            PsAdapter psa = new PsAdapter();
            psa.CreateEarzyTenantWebsite(siteName);

            //var r2 = await _webSitesManagementClient.WebSites.CreateRepositoryAsync(webspaceName, name);
        }

        internal async Task CreateEarzyForTenantSTEPB(string tenantName, string siteName,string accountId)
        {
            var storageConnectionString = await GetStorageAccountConnectionString(siteName.ToLower());

            Dictionary<string, string> appSettings = new Dictionary<string, string>();
            appSettings.Add("AccountId", accountId);
            appSettings.Add("StorageConnectionString", storageConnectionString);
            appSettings.Add("TenantName", tenantName);

            await _webSitesManagementClient.WebSites.UpdateConfigurationAsync(_webspaceName, siteName, new WebSiteUpdateConfigurationParameters
            {
                AppSettings = appSettings

            });
        }

        internal async Task CreateStorageAccount(string region, string storageAccountName)
        {
            await _storageManagementClient.StorageAccounts.CreateAsync(
                new StorageAccountCreateParameters
                {
                    Location = region,
                    ServiceName = storageAccountName
                });
        }

        //Construct a connectionstring to access a specified storage account
        private async Task<string> GetStorageAccountConnectionString(string storageAccountName)
        {
            var keys = await _storageManagementClient.StorageAccounts.GetKeysAsync(storageAccountName);

            string connectionString = string.Format(
                CultureInfo.InvariantCulture,
                "DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                storageAccountName, keys.SecondaryKey);

            return connectionString;
        }
        #endregion

        #region Clean up

        public void Dispose()
        {
            if (_storageManagementClient != null)
                _storageManagementClient.Dispose();
            if (_computeManagementClient != null)
                _computeManagementClient.Dispose();
            if (_webSitesManagementClient != null)
                _webSitesManagementClient.Dispose();
        }

        #endregion
    }
}
