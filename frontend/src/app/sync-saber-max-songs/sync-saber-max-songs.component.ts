import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FeedReader } from '../models/FeedReader';

@Component({
    selector: 'app-sync-saber-max-songs',
    templateUrl: './sync-saber-max-songs.component.html',
    styleUrls: ['./sync-saber-max-songs.component.scss'],
})
export class SyncSaberMaxSongsComponent implements OnInit {
    @Input() reader: FeedReader;
    @Output() update = new EventEmitter();
    constructor() {}

    ngOnInit() {}

    down() {
        this.reader.MaxSongs = this.reader.MaxSongs < 1 ? 0 : this.reader.MaxSongs - 10;
        this.update.emit();
    }

    up() {
        this.reader.MaxSongs = this.reader.MaxSongs > 199 ? 200 : this.reader.MaxSongs + 10;
        this.update.emit();
    }
}
