using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EarzyWeb.Infrastructure
{
    internal static class Constants
    {
        /// <summary>
        /// Configuration section key containing connection string.
        /// </summary>
        internal const string ConfigurationSectionKey = "StorageConnectionString";

        /// <summary>
        /// Container where to upload files
        /// </summary>
        internal const string ContainerName = "uploadhub";

        /// <summary>
        /// Number of bytes in a Kb.
        /// </summary>
        internal const int BytesPerKb = 1024;

        /// <summary>
        /// Name of session element where attributes of file to be uploaded are saved.
        /// </summary>
        internal const string FileAttributesSession = "FileClientAttributes";

        internal const string AccountId = "32886274-10fa-4356-824a-f29adbeac4e4";
    }
}