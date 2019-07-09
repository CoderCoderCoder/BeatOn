import { Component, OnInit, Injectable, Inject } from '@angular/core';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { Observable } from 'rxjs';
import { ReplaceSource } from 'webpack-sources';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { AppSettings } from '../appSettings';
import { timingSafeEqual } from 'crypto';
import { ImagePickerDialogComponent } from '../image-picker-dialog/image-picker-dialog.component';
import { AppIntegrationService } from '../services/app-integration.service';
@Component({
    selector: 'app-add-edit-playlist-dialog',
    templateUrl: './add-edit-playlist-dialog.component.html',
    styleUrls: ['./add-edit-playlist-dialog.component.scss'],
})
@Injectable({
    providedIn: 'root',
})
export class AddEditPlaylistDialogComponent implements OnInit {
    constructor(
        private appInt: AppIntegrationService,
        public dialogRef: MatDialogRef<AddEditPlaylistDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data,
        private dialog: MatDialog
    ) {
        var fixedUri = encodeURIComponent(data.playlist.PlaylistID);
        fixedUri = fixedUri.replace('(', '%28').replace(')', '%29');
        this.currentCover = AppSettings.API_ENDPOINT + '/host/beatsaber/playlist/cover?playlistid=' + fixedUri;
    }
    getCover() {
        return 'url(' + this.currentCover + ')';
    }
    onQuest() {
        return this.appInt.isAppLoaded();
    }
    currentCover;
    makeAutoID(input: string) {
        if (input == null || !input.replace(' ', '').length) {
            return 'Playlist ID';
        }
        var ret = input
            .replace(/[^a-z0-9]/gim, '')
            .replace(/\s+/g, '')
            .replace(' ', '');
        return 'Playlist ID (' + ret + ')' + new Date().getTime();
    }

    loadCover(files) {
        if (files.length == 0) {
            console.log('No file selected!');
            return;
        }
        let file: File = files[0];
        var reader = new FileReader();
        reader.onload = ev => {
            this.currentCover = reader.result;
            this.data.playlist.CoverImageBytes = this.currentCover.substring(this.currentCover.indexOf(';base64,') + 8);
        };
        reader.readAsDataURL(file);
    }

    clickSave() {
        if (this.data.playlist.PlaylistName.replace(' ', '').length < 1) {
            //todo: show error why no save
            return;
        }
        if (!this.data.playlist.PlaylistID || this.data.playlist.PlaylistID.replace(' ', '').length < 1) {
            if (this.data.isNew) {
                this.data.playlist.PlaylistID =
                    this.data.playlist.PlaylistName.replace(/[^a-z0-9]/gim, '')
                        .replace(/\s+/g, '')
                        .replace(' ', '') + new Date().getTime();
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
        this.dialogRef.close({ playlist: this.data.playlist, deletePlaylist: true });
    }
    questClick() {
        const dr = this.dialog.open(ImagePickerDialogComponent, {
            width: '500px',
            height: '500px',
            disableClose: true,
        });
        dr.afterClosed().subscribe(res => {
            if (res) {
                this.currentCover = res;
                this.data.playlist.CoverImageBytes = this.currentCover.substring(this.currentCover.indexOf(';base64,') + 8);
            }
        });
    }

    ngOnInit() {}
}
