using EarzyWeb.Infrastructure;
using EarzyWeb.Models;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.IO;
using System.Diagnostics;

namespace EarzyWeb.Controllers
{
    public class TracksController : ApiController
    {

        public IEnumerable<PlayListItem> Get()
        {
            if (CruelCache.Instance.Store.ContainsKey("Tracks") && CruelCache.Instance.Store["Tracks"] != null && (CruelCache.Instance.LastUpdate.AddHours(1) > DateTime.Now))
            {
                Trace.TraceInformation("Data retrieved from cache.");
                return (List<PlayListItem>)CruelCache.Instance.Store["Tracks"];

            }

           
            List<PlayListItem> result = new List<PlayListItem>();
            foreach (Track entity in QueryTable())
            {
                PlayListItem playlistItem = new PlayListItem
                {
                    artist = entity.Artist,
                    title = entity.Title,
                    album = entity.Album,
                    mp3 =  entity.Blob
                };
                result.Add(playlistItem);
            }

            Trace.TraceInformation("Data retrieved from storage.");

            CruelCache.Instance.Store["Tracks"] = result;
            CruelCache.Instance.LastUpdate = DateTime.Now;
            return result;
        }

        public IEnumerable<string> Get(string id)
        {
            return new List<string>();
        }

        public IEnumerable<string> GetArtists()
        { 
            if (CruelCache.Instance.Store.ContainsKey("Tracks") && CruelCache.Instance.Store["Tracks"] != null)
            {
                var tracks = (List<PlayListItem>)CruelCache.Instance.Store["Tracks"];

                var artists = tracks.Where(x => x.artist != null).Select(x => x.artist).Distinct().ToList();
               
                List<string> noDupes = new List<string>(new HashSet<string>(artists));
                artists.Clear();
                artists.AddRange(noDupes);
                return artists;
            }
            
            return new List<string>();
        }

        public string GetFreshData()
        {
            Trace.TraceInformation("Data refresh request. Cache expired.");
            CruelCache.Instance.LastUpdate = DateTime.Now.AddHours(-2);
            return ConfigurationManager.AppSettings["AccountId"];
        }


        public dynamic GetUriTemplate()
        {
           var uriTemplate = new {  Prefix = CruelCache.Instance.BlobBaseUri,  Suffix = CruelCache.Instance.SasToken };

           return uriTemplate;
        }

        private IEnumerable<Track> QueryTable()
        {
            CloudTable table = CruelCache.Instance.TableClient.GetTableReference("tracks");
            TableQuery<Track> query = new TableQuery<Track>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, ConfigurationManager.AppSettings["AccountId"]));

            var queryResult = table.ExecuteQuery(query);

            //foreach (Track track in queryResult)
            //{
            //if (track.Artist == null || track.Artist == "")
            //{
            //    track.Title = Path.GetFileNameWithoutExtension(track.OriginalFileName);
            //    if ((track.Album == null || track.Album == "") && new DirectoryInfo(track.OriginalFileName).Parent != null)
            //    {
            //        track.Album = new DirectoryInfo(track.OriginalFileName).Parent.Name;
            //    }

            //    if ((track.Artist == null || track.Artist == "") && new DirectoryInfo(track.OriginalFileName).Parent.Parent != null)
            //        track.Artist = new DirectoryInfo(track.OriginalFileName).Parent.Parent.Name;
            //}

            //        TableOperation updateOperation = TableOperation.Replace(track);
            //        table.Execute(updateOperation);
            //    }
            //}

            return table.ExecuteQuery(query);
        }
    }
}