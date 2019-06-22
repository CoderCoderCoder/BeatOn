import { Component, OnInit, Input } from '@angular/core';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { AppSettings } from '../appSettings';
@Component({
  selector: 'app-playlist-songs',
  templateUrl: './playlist-songs.component.html',
  styleUrls: ['./playlist-songs.component.scss']
})
export class PlaylistSongsComponent implements OnInit {
  constructor() { }
  
  @Input() playlist : BeatSaberPlaylist = { CoverArtFilename: "",
    PlaylistID : "",
    PlaylistName : "",
    SongList: []
    }

  ngOnInit() {

  }

  getBackground(item) {
    return AppSettings.API_ENDPOINT +'/host/beatsaber/songcover?songid=' + item.SongID ;
  }


}
