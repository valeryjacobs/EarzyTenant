$(document).ready(function () {
    function earzymodel(artist, title, album) {
        this.artist = ko.observable(artist);
        this.title = ko.observable(title);
        this.album = ko.observable(album);
        this.nosongs = ko.observable(0);
        this.mp3 = ko.observable();
        this.url = ko.observable();
    };

    var myvm = new earzymodel("-", "-", "-", 0);

    ko.applyBindings(myvm);

    var uriPrefix;
    var uriSuffix;
    var oTable;
    var uri = 'api/tracks';

    $(function () {
        $("#jquery_jplayer_1").jPlayer({
            ready: function () {
                //$(this).jPlayer("setMedia", {
                //    mp3: "https://jacobsmedia.blob.core.windows.net/uploads/01%20Anguish.mp3?sr=c&si=media&sig=J5%2B5IqsN2TcwlLgEK%2FMF%2Ft7VVAb1qwG2eDvLybrddHU%3D" // Defines the m4v url
                //});
            },
            supplied: "mp3",
            wmode: "window",
            smoothPlayBar: true,
            keyEnabled: true,
            error: function (event) {
                var time = $("#jquery_jplayer_1").data().jPlayer.status.currentTime;
                if (time > 0) {
                    $("#jquery_jplayer_1").jPlayer("play", time);
                }
                else {
                    function Breath() {
                        setTimeout(function () {
                            $("#jquery_jplayer_1").jPlayer("play", 0);
                        }, 5000);
                    }
                }
            }
        });

        $("#jquery_jplayer_1").bind($.jPlayer.event.ended + ".repeat", function () {
            var randomnumber = Math.floor(Math.random() * (myvm.nosongs() - +1));
            var row = oTable.fnGetData(randomnumber);

            myvm.artist(row.artist);
            myvm.title(row.title);
            myvm.album(row.album);
            myvm.mp3(row.mp3);
            var blobpath = myvm.mp3();
            if (blobpath.substring(blobpath.length - 4, blobpath.length) != ".mp3") {
                blobpath += ".mp3"
            }
            myvm.url(uriPrefix + blobpath + uriSuffix);

            $(this).jPlayer("setMedia", { mp3: myvm.url() });
            $(this).jPlayer("play");
        });

    });

    $("#stopbutton").click(function () {
        $("#jquery_jplayer_1").jPlayer("stop");
    });



    $.getJSON(uri)
        .done(function (data) {

            myvm.nosongs(data.length);

            $('#tablecontainer').html('<table cellpadding="0" cellspacing="0" border="0" class="display" id="example"></table>');
            oTable = $('#example').dataTable({
                "bPaginate": true,
                "bLengthChange": false,
                "bFilter": true,
                "bInfo": false,
                "bAutoWidth": false,
                "bScrollCollapse": false,
                "sScrollY": "220px",
                "bProcessing": true,
                "sAjaxSource": 'api/tracks/',
                "sAjaxDataProp": "",
                "aoColumns": [
                    {
                        "mDataProp": "artist"
                    },
                    {
                        "mDataProp": "title"
                    },
                      {
                          "mDataProp": "album"
                      },
                      {
                          "bVisible": false, "mDataProp": "mp3"
                      }
                ]
            });

            $.getJSON('api/tracks/geturitemplate')
              .done(function (data) {
                  uriPrefix = data.Prefix.substring(0, data.Prefix.length - 1);
                  uriSuffix = data.Suffix;
                  var accesSignature = data;


                  $('#nextbutton').live("click", function () {
                      var time = $("#jquery_jplayer_1").data().jPlayer.status.duration;
                      $("#jquery_jplayer_1").jPlayer("play", time - 0.1);
                  });

                  $('#nosongs').live("click", function () {
                      $.getJSON('api/tracks/GetFreshData')
                      .done(function (data) {
                          var x = data;
                      });
                  });

                  $('#example tbody td').live('click', function () {
                      var aPos = oTable.fnGetPosition(this);
                      var aData = oTable.fnGetData(aPos[3]);

                      //ko.applyBindings(new myvm(aData[aPos[0]].artist, aData[aPos[0]].title, aData[aPos[0]].album)); data.artist = aData[aPos[0]].artist;
                      myvm.artist(aData[aPos[0]].artist);
                      myvm.title(aData[aPos[0]].title);
                      myvm.album(aData[aPos[0]].album);
                      myvm.mp3(aData[aPos[0]].mp3);
                      var blobpath = aData[aPos[0]].mp3;
                      if (blobpath.substring(blobpath.length - 4, blobpath.length) != ".mp3") {
                          blobpath += ".mp3"
                      }
                      myvm.url(uriPrefix + blobpath + uriSuffix);

                      $("#jquery_jplayer_1").jPlayer("setMedia", { mp3: myvm.url() });
                      $("#jquery_jplayer_1").jPlayer("play");
                  });
              });

            var uriartitst = 'api/tracks/getartists';
            $.getJSON(uriartitst)
                    .done(function (data) {
                        var availableTags = data;

                        //$("#artistssearch").autocomplete({
                        //    source: availableTags
                        //});

                        //$("#auto").autocomplete({
                        //    source: function (request, response) {
                        //        var results = $.ui.autocomplete.filter(myarray, request.term);
                        //        response(results.slice(0, 10));
                        //    },
                        //    select: function (event, ui) {
                        //        alert('fdfd' + ui.item.value);
                        //        return false;
                        //    }
                        //});

                    });
        });
});