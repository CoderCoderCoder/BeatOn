import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { NguCarouselConfig } from '@ngu/carousel';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { Observable } from 'rxjs';
import {AppSettings} from '../appSettings';

@Component({
  selector: 'app-playlist-slider',
  templateUrl: './playlist-slider.component.html',
  styleUrls: ['./playlist-slider.component.scss']
})
export class PlaylistSliderComponent implements OnInit {
  @Output() public selectedPlaylist: EventEmitter<BeatSaberPlaylist> = new EventEmitter<BeatSaberPlaylist>();
  @Input() public playlists: BeatSaberPlaylist[] = [];
  
   selected : BeatSaberPlaylist;
  //public carouselTileItems: Array<any> = [0, 1, 2, 3, 4, 5];



  getBackground(item) {
    return 'url('+ AppSettings.API_ENDPOINT +'/host/beatsaber/playlistcover?playlistid=' + item.PlaylistID + ')';
  }

  onTileClick(item) {
    console.log("playlist selecteD");
    this.selected = item;
    this.selectedPlaylist.emit(item);
  }
  public carouselTile: NguCarouselConfig = {
    grid: { xs:4, sm:4, md: 4, lg: 4, all: 0 },
    slide: 3,
    speed: 250,
    point: {
      visible: false
    },
    load: 2,
    velocity: 0,
    touch: true,
    easing: 'cubic-bezier(0, 0, 0.2, 1)',
    vertical: {
      enabled: true,
      height: 390
    }
  };
  constructor() {}
 
  ngOnInit() {
      // this.playlists = [ {
      //   CoverArtFilename : "something.png",
      //   PlaylistID : "CustomSongs",
    
      //   PlaylistName : "Custom Songs",
      //   SongList: []
      // },
      // {
      //   CoverArtFilename : "something.png",
      //   PlaylistID : "CustomSongs",
    
      //   PlaylistName : "Custom Songs",
      //   SongList: []
      // },
      // {
      //   CoverArtFilename : "something.png",
      //   PlaylistID : "CustomSongs",
    
      //   PlaylistName : "Custom Songs",
      //   SongList: []
      // },
      // {
      //   CoverArtFilename : "something.png",
      //   PlaylistID : "CustomSongs",
    
      //   PlaylistName : "Custom Songs",
      //   SongList: []
      // },
      // {
      //   CoverArtFilename : "something.png",
      //   PlaylistID : "CustomSongs",
    
      //   PlaylistName : "Custom Songs",
      //   SongList: []
      // }];
  }
 
}


