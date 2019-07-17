import { Component, Input, OnInit } from '@angular/core';

@Component({
    selector: 'app-sync-saber-max-songs',
    templateUrl: './sync-saber-max-songs.component.html',
    styleUrls: ['./sync-saber-max-songs.component.scss'],
})
export class SyncSaberMaxSongsComponent implements OnInit {
    @Input() maxSongs = 10;
    constructor() {}

    ngOnInit() {}

    down() {
        this.maxSongs = this.maxSongs < 1 ? 0 : this.maxSongs - 10;
    }

    up() {
        this.maxSongs = this.maxSongs > 199 ? 200 : this.maxSongs + 10;
    }
}
