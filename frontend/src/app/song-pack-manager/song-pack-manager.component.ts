import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { DragulaService } from 'ng2-dragula';
import {Subscription} from "rxjs/internal/Subscription";
import {ClientSortPlaylist} from "../models/ClientSortPlaylist";
import {PlaylistSortMode} from "../models/PlaylistSortMode";
import {HostMessageService} from "../services/host-message.service";
import {AppSettings} from "../appSettings";
declare let autoScroll;
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
    selectAllToggle: boolean;
    reverseSortToggle: boolean;
    BAG = 'SONGS';
    test: string;
    subs = new Subscription();
    public constructor(
        private dragulaService: DragulaService,
        private msgSvc : HostMessageService
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
        this.subs.add(
            dragulaService.drop(this.BAG).subscribe(({ el }) => {
                //this.addClass(el, 'ex-moved');
                console.log('dropped');
                this.saveJson.emit();
            })
        );
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
        return this.songs.filter(s => s.Selected).length;
    }
    addSelected(pack) {
        pack.SongList = pack.SongList.concat(this.songs.filter(s => s.Selected));
        this.songs.forEach(s => (s.selected = false));
        this.checkboxChecked = false;
    }
    uniquePack(pack) {
        let keys = {};
        pack.isOpen = false;
        pack.SongList = pack.SongList.filter(a => {
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
    getBackground(SongID) {
      return AppSettings.API_ENDPOINT +'/host/beatsaber/song/cover?songid=' + SongID ;
    }
    sortAuthor(PlaylistID: string){
      this.sortPack(PlaylistID, PlaylistSortMode.LevelAuthor);
    }
    sortDifficulty(PlaylistID: string){
      this.sortPack(PlaylistID, PlaylistSortMode.MaxDifficulty);
      // setTimeout(()=>{
      //   console.log(this.packs);
      // },3000)
    }
    sortName(PlaylistID: string){
      this.sortPack(PlaylistID, PlaylistSortMode.Name);
    }
    sortPack(PlaylistID: string, mode: PlaylistSortMode) {
      this.reverseSortToggle = !this.reverseSortToggle;
      const sort = new ClientSortPlaylist();
      sort.PlaylistID = PlaylistID;
      sort.SortMode = mode;
      sort.Reverse = this.reverseSortToggle;
      this.msgSvc.sendClientMessage(sort);
        this.saveJson.emit();
    }
    removeSongFromPack(song, pack) {
        pack.SongList = pack.SongList.filter(s => s !== song);
        this.saveJson.emit();
    }
    selectAll() {
      this.selectAllToggle = !this.selectAllToggle;
        this.checkboxChecked = true;
        this.songs.forEach(s => {
            s.Selected = this.selectAllToggle;
        });
    }
}
