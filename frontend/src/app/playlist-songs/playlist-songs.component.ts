import { Component, OnInit, Input } from '@angular/core';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { AppSettings } from '../appSettings';
import { BeatSaberSong } from '../models/BeatSaberSong';
import { sortAscendingPriority } from '@angular/flex-layout';
import { HostMessageService } from '../services/host-message.service';
import { ClientDeleteSong } from '../models/ClientDeleteSong.cs';
import { MatDialog } from '@angular/material';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { ClientSortPlaylist } from '../models/ClientSortPlaylist';
import { PlaylistSortMode } from '../models/PlaylistSortMode';
@Component({
  selector: 'app-playlist-songs',
  templateUrl: './playlist-songs.component.html',
  styleUrls: ['./playlist-songs.component.scss']
})
export class PlaylistSongsComponent implements OnInit {
  constructor(private msgSvc : HostMessageService, private dialog : MatDialog) { }
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

    }
    
  }

  getBackground(item) {
    return AppSettings.API_ENDPOINT +'/host/beatsaber/song/cover?songid=' + item.SongID ;
  }
  clickSortName(reverse : boolean) {
    var sort = new ClientSortPlaylist();
    sort.PlaylistID = this._playlist.PlaylistID;
    sort.SortMode = PlaylistSortMode.Name;
    sort.Reverse = reverse;
    this.msgSvc.sendClientMessage(sort);
  }

  clickSortDifficulty(reverse : boolean) {
    var sort = new ClientSortPlaylist();
    sort.PlaylistID = this._playlist.PlaylistID;
    sort.SortMode = PlaylistSortMode.MaxDifficulty;
    sort.Reverse = reverse;
    this.msgSvc.sendClientMessage(sort);
  }
  clickDeleteSong(song) {
    var dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '450px',
      height: '180px',
      disableClose: true,
      data: {title: "Delete "+ song.SongName+"?", subTitle: "Are you sure you want to delete this song?", button1Text: "Yes"}
    });
    dialogRef.afterClosed().subscribe(res => {
      if (res == 1) {
        var msg = new ClientDeleteSong();
        msg.SongID = song.SongID;
        this.msgSvc.sendClientMessage(msg);
      }
    });    
  }
}
