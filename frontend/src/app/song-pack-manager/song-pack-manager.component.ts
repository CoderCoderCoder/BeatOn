import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
// import { BsaberService } from '../bsaber.service';
import { DragulaService } from 'ng2-dragula';
import {Subscription} from "rxjs/internal/Subscription";
// import { Subscription } from 'rxjs/Subscription';
// import { AppService } from '../app.service';
declare let autoScroll;
export interface SongItem {
    id: string;
    name: string;
    path: string;
    cover: string;
    created: number;
    selected?: boolean;
    _songAuthorName?: string;
    _levelAuthorName?: string;
    _beatsPerMinute?: string;
    _difficultyBeatmapSets?: string[];
}
@Component({
    selector: 'app-song-pack-manager',
    templateUrl: './song-pack-manager.component.html',
    styleUrls: ['./song-pack-manager.component.scss'],
})
export class SongPackManagerComponent implements OnInit {
    @Input('packs') packs;
    @Input('songs') songs;
    @Output('addPack') addPack = new EventEmitter();
    @Output('editPack') editPack = new EventEmitter();
    @Output('saveJson') saveJson = new EventEmitter();
    @Output('removeSong') removeSong = new EventEmitter();
    @Output('orderSongs') orderSongs = new EventEmitter();
    @Output('openSong') openSong = new EventEmitter();
    @ViewChild('song_container', { static: false }) song_container;
    @ViewChild('pack_container', { static: false }) pack_container;
    @ViewChild('mirror_holder', { static: false }) mirror_holder;
    checkboxChecked: boolean;
    BAG = 'SONGS';
    test: string;
    subs = new Subscription();
    public constructor(
        private dragulaService: DragulaService,
       // public bsaberService: BsaberService,
        //private appService: AppService
    ) {
        this.dragulaService.createGroup(this.BAG, {
            copy: (el, source) => {
                return source === this.song_container.nativeElement;
            },
            accepts: (el, target) => {
                return (
                    target !== this.song_container.nativeElement &&
                    ((el.parentElement === this.pack_container.nativeElement && target === this.pack_container.nativeElement) ||
                        (el.parentElement !== this.pack_container.nativeElement && target !== this.pack_container.nativeElement))
                );
            },
            moves: (el, source, handle, sibling) => {
                return !~el.className.indexOf('add-to-drag'); // elements are always draggable by default
            },
            copyItem: (item: any) => ({ ...item }),
            //mirrorContainer: this.mirror_holder.nativeElement,
        });
        // this.subs.add(dragulaService.drag(this.BAG)
        //   .subscribe(({ el }) => {
        //     //this.removeClass(el, 'ex-moved');
        //   })
        // );
        this.subs.add(
            dragulaService.drop(this.BAG).subscribe(({ el }) => {
                //this.addClass(el, 'ex-moved');
                console.log('dropped');
                this.saveJson.emit();
            })
        );
        // this.subs.add(dragulaService.over(this.BAG)
        //   .subscribe(({ el, container }) => {
        //     //console.log('over', container);
        //     //this.addClass(container, 'ex-over');
        //   })
        // );
        // this.subs.add(dragulaService.out(this.BAG)
        //   .subscribe(({ el, container }) => {
        //    // console.log('out', container);
        //     //this.removeClass(container, 'ex-over');
        //   })
        // );
    }
    ngOnDestroy() {
        this.dragulaService.destroy('SONGS');
        this.subs.unsubscribe();
    }
    ngOnInit() {}
    ngAfterViewInit() {
        let drake = this.dragulaService.find('SONGS').drake;
        let scroll = autoScroll([window, this.pack_container.nativeElement], {
            margin: 150,
            autoScroll: function() {
                return this.down && drake.dragging;
            },
        });
    }
    hasSelected() {
        return this.songs.filter(s => s.selected).length;
    }
    addSelected(pack) {
        pack.levelIDs = pack.levelIDs.concat(this.songs.filter(s => s.selected));
        this.songs.forEach(s => (s.selected = false));
        this.checkboxChecked = false;
    }
    uniquePack(pack) {
        let keys = {};
        pack.isOpen = false;
        pack.levelIDs = pack.levelIDs.filter(a => {
            let key = '_' + a.id + '|' + a.name;
            if (!keys[key]) {
                keys[key] = true;
                return true;
            }
        });
        setTimeout(() => {
            pack.isOpen = true;
            this.saveJson.emit();
        });
    }
    sortPack(pack, isRecent?) {
        if (isRecent) {
            // pack.SongList = pack.SongList
            //     .sort((a, b) => {
            //         return a.created < b.created ? -1 : a.created > b.created ? 1 : 0;
            //     })
            //     .reverse();
        } else {
            pack.SongList = pack.SongList.sort((a, b) => {
                let textA = a.SongName.toUpperCase();
                let textB = b.SongName.toUpperCase();
                return textA < textB ? -1 : textA > textB ? 1 : 0;
            });
        }
        this.saveJson.emit();
    }
    removeSongFromPack(song, pack) {
        pack.SongList = pack.SongList.filter(s => s !== song);
        this.saveJson.emit();
    }
    selectAll() {
        this.checkboxChecked = true;
        this.songs.forEach(s => {
            s.selected = true;
        });
    }
}
