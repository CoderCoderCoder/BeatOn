import { Component, OnInit, Input } from '@angular/core';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { AppSettings } from '../appSettings';
import { BeatSaberSong } from '../models/BeatSaberSong';
@Component({
  selector: 'app-playlist-songs',
  templateUrl: './playlist-songs.component.html',
  styleUrls: ['./playlist-songs.component.scss']
})
export class PlaylistSongsComponent implements OnInit {
  constructor() { }
  private _playlist : BeatSaberPlaylist = <BeatSaberPlaylist>{};
  //@Input() playlist : BeatSaberPlaylist = <BeatSaberPlaylist>{};
  songlist : BeatSaberSong[];
  
  get playlist() : BeatSaberPlaylist {
    return this._playlist;
  }

  @Input()
  set playlist(playlist: BeatSaberPlaylist)
  {
    this._playlist = playlist;
    if (this._playlist == null || this._playlist.SongList == null) {
      this.songlist = null;
    } else {
      this.songlist = this._playlist.SongList.slice();
    }
  }
  ngOnInit() {

  }
  drop(e : CdkDragDrop<string>) 
  {
    if (e.container != e.previousContainer) {
      debugger;
      console.log("got a drop in songsd! " + JSON.stringify(e.previousContainer.element.nativeElement.attributes));
    }
    
  }
  getBackground(item) {
    return AppSettings.API_ENDPOINT +'/host/beatsaber/song/cover?songid=' + item.SongID ;
  }


}
