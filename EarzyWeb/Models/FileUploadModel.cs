﻿using System;
using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace EarzyWeb.Models
{
   

    /// <summary>
    /// Model denoting file object to be uploaded to blob.
    /// </summary>
    public class FileUploadModel
    {
        /// <summary>
        /// Gets or sets the block count.
        /// </summary>
        /// <value>The block count.</value>
        public long BlockCount { get; set; }

        /// <summary>
        /// Gets or sets the size of the file.
        /// </summary>
        /// <value>The size of the file.</value>
        public long FileSize { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the CloudBlockBlob object associated with this blob.
        /// </summary>
        /// <value>The CloudBlockBlob object associated with this blob.</value>
        public CloudBlockBlob BlockBlob { get; set; }

        /// <summary>
        /// Gets or sets the operation start time.
        /// </summary>
        /// <value>The start time.</value>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Gets or sets the upload status message.
        /// </summary>
        /// <value>The upload status message.</value>
        public string UploadStatusMessage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether upload of this instance is complete.
        /// </summary>
        /// <value>
        /// True if upload of this instance is complete; otherwise, false.
        /// </value>
        public bool IsUploadCompleted { get; set; }
    }
}