using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EarzyV1
{
    public class Track: TableEntity
    {
        public string id { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "artist")]
        public string Artist { get; set; }

        [JsonProperty(PropertyName = "album")]
        public string Album { get; set; }

        [JsonProperty(PropertyName = "year")]
        public int Year { get; set; }

        [JsonProperty(PropertyName = "blob")]
        public string Blob { get; set; }

        [JsonProperty(PropertyName = "score")]
        public int Score { get; set; }

        [JsonProperty(PropertyName = "comment")]
        public string Comment { get; set; }

        [JsonProperty(PropertyName = "lastplay")]
        public DateTime LastPlay { get; set; }

        [JsonProperty(PropertyName = "originalfilename")]
        public string OriginalFileName { get; set; }

        [JsonProperty(PropertyName = "uploaded")]
        public DateTime Uploaded { get; set; }

        [JsonProperty(PropertyName = "synced")]
        public bool Synced { get; set; }

        [JsonProperty(PropertyName = "genre")]
        public string Genre { get; set; }

        [JsonProperty(PropertyName = "size")]
        public Int64 Size { get; set; }

    }
}
