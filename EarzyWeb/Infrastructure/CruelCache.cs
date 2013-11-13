using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace EarzyWeb.Infrastructure
{
    public sealed class CruelCache
    {
        private static readonly Lazy<CruelCache> lazy =
            new Lazy<CruelCache>(() => new CruelCache());

        public static CruelCache Instance { get { return lazy.Value; } }

        private CruelCache()
        {
            Store = new Dictionary<string, object>();
            LastUpdate = DateTime.Now.AddDays(-1);
            Init();
        }

        public Dictionary<string,object> Store { get; set; }

        public CloudStorageAccount Account { get; set; }
        public CloudTableClient TableClient { get; set; }

        public CloudBlobContainer Container { get; set; }

        public string SasToken { get; set; }

        public Uri BlobBaseUri { get; set; }

        public DateTime LastUpdate { get; set; }

        private void Init()
        {
            Trace.TraceInformation("Starting initialization.");
            Account = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[Constants.ConfigurationSectionKey]);
            TableClient = Account.CreateCloudTableClient();

            Container = Account.CreateCloudBlobClient().GetContainerReference(Constants.ContainerName);

            BlobContainerPermissions blobPermissions = new BlobContainerPermissions();
            blobPermissions.SharedAccessPolicies.Add(ConfigurationManager.AppSettings["AccountId"], new SharedAccessBlobPolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(10),
                Permissions = SharedAccessBlobPermissions.Write |
                   SharedAccessBlobPermissions.Read
            });
            blobPermissions.PublicAccess = BlobContainerPublicAccessType.Off;

            // Set the permission policy on the container.
            Container.SetPermissions(blobPermissions);

            SasToken = Container.GetSharedAccessSignature(new SharedAccessBlobPolicy(), ConfigurationManager.AppSettings["AccountId"]);

            BlobBaseUri = Account.BlobEndpoint;
            Trace.TraceInformation("Initialization finished.");
        }
    }
}