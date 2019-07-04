import { Component, OnInit, Injectable, Inject } from '@angular/core';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { Observable } from 'rxjs';
import { ReplaceSource } from 'webpack-sources';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AppSettings } from '../appSettings';
import { timingSafeEqual } from 'crypto';
@Component({
  selector: 'app-add-edit-playlist-dialog',
  templateUrl: './add-edit-playlist-dialog.component.html',
  styleUrls: ['./add-edit-playlist-dialog.component.scss']
})
@Injectable({
  providedIn: 'root'
})
export class AddEditPlaylistDialogComponent implements OnInit {

  constructor( public dialogRef: MatDialogRef<AddEditPlaylistDialogComponent>, @Inject(MAT_DIALOG_DATA) public data) { 
    var fixedUri = encodeURIComponent(data.playlist.PlaylistID);
    fixedUri = fixedUri.replace('(', '%28').replace(')', '%29');
    this.currentCover = AppSettings.API_ENDPOINT +'/host/beatsaber/playlist/cover?playlistid=' + fixedUri;
  }
  getCover() {
    return 'url('+this.currentCover+')';
  }

  currentCover;
  makeAutoID(input : string) {
    if (input == null || !input.replace(' ', '').length)
    {
      return "Playlist ID";
    }
    var ret= input.replace(/[^a-z0-9]/gmi, "").replace(/\s+/g, "").replace(' ','');
    return "Playlist ID ("+ret+")";
  }

  loadCover(files) {

    if (files.length == 0) {
       console.log("No file selected!");
       return
    }
      let file: File = files[0];
      var reader = new FileReader();
    reader.onload = (ev) => {
        this.currentCover = reader.result;
        this.data.playlist.CoverImageBytes = this.currentCover.substring(this.currentCover.indexOf(";base64,")+8);
    };
    reader.readAsDataURL(file);
  }

  clickSave() {
    if (this.data.playlist.PlaylistName.replace(' ','').length < 1)
    {
      //todo: show error why no save
      return;
    }
    if (!this.data.playlist.PlaylistID || this.data.playlist.PlaylistID.replace(' ','').length < 1)
    {
      if (this.data.isNew) {
          this.data.playlist.PlaylistID = this.data.playlist.PlaylistName.replace(/[^a-z0-9]/gmi, "").replace(/\s+/g, "").replace(' ','');
          this.data.playlist.SongList = [];
      } else {
        //how?
        return;
      }
    }
    this.dialogRef.close(this.data);
  }

  clickCancel() {
    this.data = null;
    this.dialogRef.close();
  }

  clickDelete() {
    
    this.dialogRef.close({ playlist: this.data.playlist, deletePlaylist: true});
  }

  ngOnInit() {
  }

}
