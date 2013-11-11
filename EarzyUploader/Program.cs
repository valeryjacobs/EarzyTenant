using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;

namespace EarzyV1
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                DirScan ds = new DirScan();

                string[] files = ds.Browse(folderBrowserDialog.SelectedPath);
                Log(string.Format("Uploading {0} files", files.Length));

                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(CloudConfigurationManager.GetSetting("StorageConnectionString"));

                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudTableClient tableClient = storageAccount.CreateCloudTableClient();

                CloudBlobContainer container = blobClient.GetContainerReference("uploadhub");
                CloudTable table = tableClient.GetTableReference("tracks");
                container.CreateIfNotExists();
                table.CreateIfNotExists();

                /////////////Check Exist///////////////

                TableQuery<Track> query = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, CloudConfigurationManager.GetSetting("LibraryId")));

                var queryResult = table.ExecuteQuery(query);

                var existingItems = new List<Tuple<string, string, long>>();



                foreach (Track track in queryResult)
                {
                    existingItems.Add(new Tuple<string, string, long>(Path.GetFileName(track.OriginalFileName), track.Artist, track.Size));
                }

                ////////////////////////////////////////////////

                foreach (string filepath in files)
                {
                    try
                    {
                        var id = Guid.NewGuid().ToString();

                        var id3 = new Id3Lib.ID3v1();

                        using (var fileStream = System.IO.File.OpenRead(filepath))
                        {
                            var track = new Track
                            {
                                PartitionKey = CloudConfigurationManager.GetSetting("LibraryId"),
                                RowKey = (DateTime.MaxValue.Ticks - DateTime.Now.Ticks).ToString(),
                                id = id,
                                OriginalFileName = filepath,
                                Uploaded = DateTime.Now,
                                Size = fileStream.Length,
                                LastPlay = DateTime.Now
                            };


                            CloudBlockBlob blockBlob = null;

                            try
                            {
                                id3.Deserialize(fileStream);
                                track.Album = id3.Album.Trim();
                                track.Artist = id3.Artist.Trim();
                                track.Comment = id3.Comment.Trim();
                                track.Genre = id3.Genre.ToString().Trim();
                                track.Title = id3.Song.Trim();
                                int year;
                                if (int.TryParse(id3.Year, out year))
                                    track.Year = year;
                            }
                            catch
                            {
                            }
                            finally
                            {
                                if (track.Artist == null || track.Artist == "")
                                {
                                    if ((track.Artist == null || track.Artist == "") && new DirectoryInfo(track.OriginalFileName).Parent.Parent != null)
                                        track.Artist = new DirectoryInfo(track.OriginalFileName).Parent.Parent.Name;
                                }

                                if (track.Title == null || track.Title == "")
                                    track.Title = Path.GetFileNameWithoutExtension(track.OriginalFileName);

                                if ((track.Album == null || track.Album == "") && new DirectoryInfo(track.OriginalFileName).Parent != null)
                                    track.Album = new DirectoryInfo(track.OriginalFileName).Parent.Name;


                                if (!existingItems.Exists(x => x.Item1 == Path.GetFileName(track.OriginalFileName) &&
                                    x.Item2 == track.Artist &&
                                    x.Item3 == track.Size))
                                {
                                    if (blockBlob == null)
                                        blockBlob = container.GetBlockBlobReference(id + ".mp3");

                                    blockBlob.Properties.ContentType = "audio/mpeg3";

                                    if (track.Comment != null && track.Comment != "")
                                        blockBlob.Metadata.Add("Comment", track.Comment != null ? track.Comment.Replace("´", "'") : "");

                                    if (track.Genre != null && track.Genre != "")
                                        blockBlob.Metadata.Add("Genre", track.Genre != null ? track.Genre.ToString().Replace("´", "'") : "");

                                    if (track.Year != null)
                                        blockBlob.Metadata.Add("Year", track.Year != null ? track.Year.ToString().Replace("´", "'") : "");

                                    if (track.Album == "")
                                        track.Album = " ";

                                    if (track.Artist == "")
                                        track.Artist = " ";

                                    if (track.Title == "")
                                        track.Title = " ";


                                    blockBlob.Metadata.Add("FilePath", filepath.Replace("´", "'"));
                                    blockBlob.Metadata.Add("Uploaded", DateTime.Now.ToString());
                                    blockBlob.Metadata.Add("Album", track.Album.Replace("´", "'"));
                                    blockBlob.Metadata.Add("Artist", track.Artist.Replace("´", "'"));
                                    blockBlob.Metadata.Add("Song", track.Title.Replace("´", "'"));

                                    fileStream.Position = 0;

                                    blockBlob.UploadFromStream(fileStream);
                                    track.Blob = blockBlob.Uri.AbsolutePath;
                                    TableOperation insertOperation = TableOperation.Insert(track);
                                    table.Execute(insertOperation);

                                    Log(string.Format("A:{0} | Title:{1} | Album:{2} | Id:{3} | Path:{4}", track.Artist, track.Title, track.Album, track.id, track.OriginalFileName));
                                }
                                else
                                {
                                    Log(string.Format("SKIPPED: {0}", track.OriginalFileName));
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Log(string.Format("ERROR:{0} | Message: {1}", filepath, ex.Message));
                    }
                }
            }
            Log("Finished.");
        }

        private static void Log(string log)
        {
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"C:\Earzy\earzyupload.log", true))
            {

                Console.WriteLine(log.Substring(0, log.Length > 100 ? 100 : log.Length));
                file.WriteLine(log);
            }
        }
    }
}
