using Microsoft.WindowsAzure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace EarzyProvisioning
{
    internal class CertificateAuthenticationHelper
    {
        internal static SubscriptionCloudCredentials GetCredentials(
            string subscrtionId,
            string base64EncodedCert)
        {
            return new CertificateCloudCredentials(subscrtionId,
                new X509Certificate2(Convert.FromBase64String(base64EncodedCert)));
        }
    }
}
