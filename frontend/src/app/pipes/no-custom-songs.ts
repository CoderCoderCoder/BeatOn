import { Component, ViewEncapsulation, ViewChild, ElementRef, PipeTransform, Pipe, OnInit, Injectable } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';

@Injectable({
    providedIn: 'root',
})
@Pipe({ name: 'nocustomsongs' })
export class NoCustomSongsPipe implements PipeTransform {
    constructor(private sanitizer: DomSanitizer) {}
    transform(val: BeatSaberPlaylist[]) {
        const customIndex = val.map(p => p.PlaylistID).indexOf('CustomSongs');
        if (customIndex > -1) {
            let cpy = val.slice(0);
            cpy.splice(customIndex, 1);
            return cpy;
        } else {
            return val;
        }
    }
}
