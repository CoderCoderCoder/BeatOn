import { Component, OnInit, Input, Output, } from '@angular/core';
import { NguCarouselConfig } from '@ngu/carousel';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-playlist-slider',
  templateUrl: './playlist-slider.component.html',
  styleUrls: ['./playlist-slider.component.scss']
})
export class PlaylistSliderComponent implements OnInit {
  @Output() public selected: BeatSaberPlaylist;
  @Input() public playlists: BeatSaberPlaylist[] = [];
  
  //public carouselTileItems: Array<any> = [0, 1, 2, 3, 4, 5];
 
  onTileClick(item) {
    this.selected = item;
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
      height: 400
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


