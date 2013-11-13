using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using EarzyWeb.Models;
using System.IO;
using System.Text;
using System.Globalization;
using EarzyWeb.Infrastructure;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Table;
using System.Diagnostics;

namespace EarzyWeb.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Prepares the meta data for file to be uploaded.
        /// </summary>
        /// <param name="blocksCount">Count of blocks to be uploaded.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileSize">Size of the file.</param>
        /// <returns>True in JSON format to the view.</returns>
        [HttpPost]
        public ActionResult PrepareMetadata(int blocksCount, string fileName, long fileSize)
        {
            Trace.TraceInformation("Preparing file upload.");
            var container = CloudStorageAccount.Parse(ConfigurationManager.AppSettings[Constants.ConfigurationSectionKey]).CreateCloudBlobClient().GetContainerReference(Constants.ContainerName);
            container.CreateIfNotExists();
            var fileToUpload = new FileUploadModel()
            {
                BlockCount = blocksCount,
                FileName = fileName,
                FileSize = fileSize,
                BlockBlob = container.GetBlockBlobReference(fileName),
                StartTime = DateTime.Now,
                IsUploadCompleted = false,
                UploadStatusMessage = string.Empty
            };
            Session.Add("FileAttributesSession", fileToUpload);
            return Json(true);
        }

        /// <summary>
        /// Uploads each block of file to be uploaded and puts block list in the end.
        /// </summary>
        /// <param name="id">The id of block to upload.</param>
        /// <returns>
        /// JSON message specifying status of operation.
        /// </returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult UploadBlock(int id)
        {
            var request = Request.Files["Slice"];
            byte[] chunk = new byte[request.ContentLength];
            request.InputStream.Read(chunk, 0, Convert.ToInt32(request.ContentLength));
            if (Session["FileAttributesSession"] != null)
            {
                var model = (FileUploadModel)Session["FileAttributesSession"];
                using (var chunkStream = new MemoryStream(chunk))
                {
                    var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", id)));
                    try
                    {
                        model.BlockBlob.PutBlock(blockId, chunkStream, null);
                    }
                    catch (StorageException e)
                    {
                        Session.Clear();
                        model.IsUploadCompleted = true;
                        model.UploadStatusMessage = string.Format(CultureInfo.CurrentCulture, "failed upload blabla", e.Message);
                        return Json(new { error = true, isLastBlock = false, message = model.UploadStatusMessage });
                    }
                }

                if (id == model.BlockCount)
                {
                    model.IsUploadCompleted = true;
                    bool errorInOperation = false;
                    try
                    {
                        var blockList = Enumerable.Range(1, (int)model.BlockCount).ToList<int>().ConvertAll(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));
                        model.BlockBlob.Properties.ContentType = "audio/mpeg3";
                        model.BlockBlob.PutBlockList(blockList);
                        var duration = DateTime.Now - model.StartTime;
                        float fileSizeInKb = model.FileSize / Constants.BytesPerKb;
                        string fileSizeMessage = fileSizeInKb > Constants.BytesPerKb ?
                            string.Concat((fileSizeInKb / Constants.BytesPerKb).ToString(CultureInfo.CurrentCulture), " MB") :
                            string.Concat(fileSizeInKb.ToString(CultureInfo.CurrentCulture), " KB");

                        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting(Constants.ConfigurationSectionKey));  
                        var tableClient = storageAccount.CreateCloudTableClient();
                        CloudTable table = tableClient.GetTableReference("tracks");
                        table.CreateIfNotExists();

                        
                        try
                        {
                            var fileId = Guid.NewGuid().ToString();
                            var id3 = new Id3Lib.ID3v1();

                            Stream stream  = new MemoryStream();
                            model.BlockBlob.DownloadToStream(stream);
                            //TODO:Use DownloadRangeToStream to save bandwidth.
                            using (var fileStream = stream)
                            {
                                var track = new Track
                                {
                                    PartitionKey = ConfigurationManager.AppSettings["AccountId"],
                                    RowKey = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString(),
                                    Id = fileId,
                                    OriginalFileName = model.FileName,
                                    Uploaded = DateTime.Now,
                                    Size = Convert.ToInt32(model.FileSize),
                                    LastPlay = DateTime.Now
                                };
                              
                                try
                                {
                                    id3.Deserialize(fileStream);                             
                                    track.Album = id3.Album;                                 
                                    track.Artist = id3.Artist;                             
                                    track.Comment = id3.Comment;                               
                                    track.Genre = id3.Genre.ToString();                            
                                    track.Title = id3.Song;
                                    int year;
                                    if (int.TryParse(id3.Year, out year))
                                        track.Year = year;
                                }
                                catch (Exception ex)
                                {
                                    track.Title = model.FileName;
                                }

                                if (track.Title == null || track.Title == "")
                                {
                                    track.Title = Path.GetFileNameWithoutExtension(track.OriginalFileName);
                                }

                                track.Blob = model.BlockBlob.Uri.AbsolutePath;
                                TableOperation insertOperation = TableOperation.Insert(track);
                                table.Execute(insertOperation);

                                Trace.TraceInformation(string.Format("File {0} uploaded.",track.OriginalFileName));
                            }
                        }
                        catch (Exception ex) {
                            Trace.TraceInformation("Error uploading. " + ex.Message);
                        }

                        model.UploadStatusMessage = string.Format(CultureInfo.CurrentCulture, "Upload completed", fileSizeMessage, duration.TotalSeconds);
                    }
                    catch (StorageException e)
                    {
                        model.UploadStatusMessage = string.Format(CultureInfo.CurrentCulture, "Upload failed", e.Message);
                        Trace.TraceInformation("Upload failed: " + e.Message);
                        errorInOperation = true;
                    }
                    finally
                    {
                        Session.Clear();
                    }

                    return Json(new { error = errorInOperation, isLastBlock = model.IsUploadCompleted, message = model.UploadStatusMessage });
                }
            }
            else
            {
                return Json(new { error = true, isLastBlock = false, message = string.Format(CultureInfo.CurrentCulture, "upload failed blabla", "session expired blabla ") });
            }

            return Json(new { error = false, isLastBlock = false, message = string.Empty });
        }
    }
}
