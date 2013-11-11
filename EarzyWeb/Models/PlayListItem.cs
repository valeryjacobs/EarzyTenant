using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EarzyWeb.Models
{
    public class PlayListItem
    {
        //  title:"Tempered Song",
        //  artist:"Miaow",
        //  mp3:"http://www.jplayer.org/audio/mp3/Miaow-01-Tempered-song.mp3",
        //  oga:"http://www.jplayer.org/audio/ogg/Miaow-01-Tempered-song.ogg",
        //  poster: "http://www.jplayer.org/audio/poster/Miaow_640x360.png"
        public string title { get; set; }
        public string artist { get; set; }
        public string album { get; set; }
        public string mp3 { get; set; }
    }
}