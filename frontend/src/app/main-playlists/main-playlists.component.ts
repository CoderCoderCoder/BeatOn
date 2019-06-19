import { Component, OnInit } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { QuestomConfig } from '../models/QuestomConfig';
@Component({
  selector: 'app-main-playlists',
  templateUrl: './main-playlists.component.html',
  styleUrls: ['./main-playlists.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainPlaylistsComponent implements OnInit {

  constructor(private beatOnApi : BeatOnApiService) { }
  config : QuestomConfig = {Playlists:   [{
      CoverArtFilename : "something.png",
      PlaylistID : "CustomSongs",
  
      PlaylistName : "Custom Songs",
      SongList: []
    },
    {
      CoverArtFilename : "something.png",
      PlaylistID : "CustomSongs",
  
      PlaylistName : "Custom reallo long name Songs",
      SongList: []
    },
    {
      CoverArtFilename : "something.png",
      PlaylistID : "CustomSongs",
  
      PlaylistName : "Custom Songs",
      SongList: []
    },
    {
      CoverArtFilename : "something.png",
      PlaylistID : "CustomSongs",
  
      PlaylistName : "Custom Songs",
      SongList: []
    },
    {
      CoverArtFilename : "something.png",
      PlaylistID : "CustomSongs",
  
      PlaylistName : "Custom Songs",
      SongList: []
    }], LeftColor: null, RightColor: null};
  
  refreshConfig() : void {
    
    this.beatOnApi.getConfig()
      .subscribe(
        (data: any) => { 
          console.log("got stuff: "+JSON.stringify(data));
          this.config = data;
        },
        (err: any) => {
          //this.testHtml = JSON.stringify(err);
          console.log("ERROR" + err);
        },
    );

  }

  ngOnInit() {
    this.refreshConfig();
  }

}
