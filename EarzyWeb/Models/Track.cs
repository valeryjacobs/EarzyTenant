using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EarzyWeb.Models
{
    public class Track : TableEntity
    {
        public Track() { }

        public string Id { get; set; }
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public string Blob { get; set; }
        public string Score { get; set; }
        public string Comment { get; set; }
        public DateTime LastPlay { get; set; }
        public string OriginalFileName { get; set; }
        public DateTime Uploaded { get; set; }
        public bool Synced { get; set; }
        public string Genre { get; set; }
        public int Size { get; set; }
    }
}