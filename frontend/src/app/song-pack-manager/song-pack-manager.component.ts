import {
    ChangeDetectorRef,
    Component,
    EventEmitter,
    Input,
    OnInit,
    Output,
    ViewChild,
    OnChanges,
    SimpleChanges,
    AfterViewChecked,
} from '@angular/core';
import { DragulaService } from 'ng2-dragula';
import { Subscription } from 'rxjs/internal/Subscription';
import { ClientSortPlaylist } from '../models/ClientSortPlaylist';
import { PlaylistSortMode } from '../models/PlaylistSortMode';
import { HostMessageService } from '../services/host-message.service';
import { AppSettings } from '../appSettings';
import { fromEvent } from 'rxjs/internal/observable/fromEvent';
import { merge } from 'rxjs/internal/observable/merge';
import { ClientMoveSongInPlaylist } from '../models/ClientMoveSongInPlaylist';
import { ClientMoveSongToPlaylist } from '../models/ClientMoveSongToPlaylist';
import { ClientMovePlaylist } from '../models/ClientMovePlaylist';
import { ClientDeleteSong } from '../models/ClientDeleteSong.cs';
import { ClientDeletePlaylist } from '../models/ClientDeletePlaylist';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { AddEditPlaylistDialogComponent } from '../add-edit-playlist-dialog/add-edit-playlist-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { ClientAddOrUpdatePlaylist } from '../models/ClientAddOrUpdatePlaylist';
import { ConfigService } from '../services/config.service';
import { Subject } from 'rxjs/internal/Subject';
import { ClientAutoCreatePlaylists } from '../models/ClientAutoCreatePlaylists';
import { trigger, transition, style, animate, state } from '@angular/animations';
declare let autoScroll;
@Component({
    selector: 'app-song-pack-manager',
    templateUrl: './song-pack-manager.component.html',
    styleUrls: ['./song-pack-manager.component.scss'],
    animations: [
        trigger('winkout', [
            transition(':leave', [style({ transform: 'scale(1)' }), animate('0.1s', style({ transform: 'scale(0)' }))]),
        ]),
    ],
})
export class SongPackManagerComponent implements OnInit, OnChanges, AfterViewChecked {
    @Input('packs') packs;
    @Input('songs') songs;
    @Output('addPack') addPack = new EventEmitter();
    @Output('editPack') editPack = new EventEmitter();
    @Output('saveJson') saveJson = new EventEmitter();
    @Output('orderSongs') orderSongs = new EventEmitter();
    @Output('openSong') openSong = new EventEmitter();
    @ViewChild('song_container', { static: false }) song_container;
    @ViewChild('pack_container', { static: false }) pack_container;
    @ViewChild('mirror_holder', { static: false }) mirror_holder;
    checkboxChecked: boolean;
    selectAllToggle: boolean;
    reverseSortToggle: boolean;
    defaultImage: string = 'assets/default-pack-cover.png';
    BAG = 'SONGS';
    test: string;
    subs = new Subscription();
    scrollObserver: any;
    lastPackScrollOffset: number;
    lastSongsScrollOffset: number;
    updateSearchResult: Subject<void>;
    updateHack: number = 0;
    public constructor(
        private dialog: MatDialog,
        private configSvc: ConfigService,
        private dragulaService: DragulaService,
        private msgSvc: HostMessageService,
        private changeRef: ChangeDetectorRef
    ) {
        this.updateSearchResult = new Subject();
        this.dragulaService.createGroup(this.BAG, {
            copy: (el, source) => {
                return false;
            },
            accepts: (el, target) => {
                return (
                    //target !== this.song_container.nativeElement &&
                    (el.parentElement === this.pack_container.nativeElement && target === this.pack_container.nativeElement) ||
                    (el.parentElement !== this.pack_container.nativeElement && target !== this.pack_container.nativeElement)
                );
            },
            moves: (el, source, handle, sibling) => {
                return !~el.className.indexOf('add-to-drag'); // elements are always draggable by default
            },
            copyItem: (item: any) => ({ ...item }),
        });
        this.subs.add(
            dragulaService.drop(this.BAG).subscribe(({ name, el, target, source, sibling }) => {
                let index;
                let playlistId = (target as HTMLElement).dataset.playlist_id;
                if (!sibling) {
                    if (target === this.pack_container.nativeElement) {
                        index = this.packs.length + 1; // add one for the custom songs removed
                    }
                } else {
                    index = Array.prototype.indexOf.call(sibling.parentNode.childNodes, sibling) - 2;
                }
                if (target === this.pack_container.nativeElement) {
                    const msg = new ClientMovePlaylist();
                    msg.PlaylistID = (el as HTMLElement).dataset.playlist_id;
                    msg.Index = index;
                    this.msgSvc.sendClientMessage(msg);
                } else {
                    let songId = (el as HTMLElement).dataset.song_id;
                    if (target === source) {
                        const msg = new ClientMoveSongInPlaylist();
                        msg.SongID = songId;
                        msg.Index = index;
                        this.msgSvc.sendClientMessage(msg);
                    } else {
                        if (playlistId == 'CustomSongs' && this.packs.findIndex(x => x.PlaylistID == 'CustomSongs') < 0) {
                            console.log('making pl');
                            const pl: BeatSaberPlaylist = <BeatSaberPlaylist>{ SongList: [] };
                            pl.PlaylistID = 'CustomSongs';
                            pl.PlaylistName = 'Custom Songs';
                            const plMsg = new ClientAddOrUpdatePlaylist();
                            plMsg.Playlist = pl;
                            this.msgSvc.sendClientMessage(plMsg);
                        }
                        const msg = new ClientMoveSongToPlaylist();
                        msg.ToPlaylistID = playlistId;
                        msg.SongID = songId;
                        msg.Index = index;
                        this.msgSvc.sendClientMessage(msg);
                    }
                }
            })
        );
    }
    ngOnDestroy() {
        this.dragulaService.destroy('SONGS');
        this.subs.unsubscribe();
    }
    ngOnInit() {}
    ngOnChanges(changes: SimpleChanges) {
        if (this.pack_container) {
            this.lastPackScrollOffset = this.pack_container.nativeElement.scrollTop;
        }
        if (this.song_container) {
            this.lastSongsScrollOffset = this.song_container.nativeElement.scrollTop;
        }
    }
    ngAfterViewChecked() {
        if (this.lastPackScrollOffset && this.pack_container) {
            this.pack_container.nativeElement.scrollTop = this.lastPackScrollOffset;
            this.lastPackScrollOffset = null;
        }
        if (this.lastSongsScrollOffset && this.song_container) {
            this.song_container.nativeElement.scrollTop = this.lastSongsScrollOffset;
            this.lastSongsScrollOffset = null;
        }
    }
    ngAfterViewInit() {
        let drake = this.dragulaService.find('SONGS').drake;
        let scroll = autoScroll([window, this.pack_container.nativeElement], {
            margin: 150,
            autoScroll: function() {
                return this.down && drake.dragging;
            },
        });
        this.scrollObserver = merge(
            fromEvent(window, 'scroll'),
            fromEvent(this.song_container.nativeElement, 'scroll'),
            fromEvent(this.pack_container.nativeElement, 'scroll'),
            this.updateSearchResult
        );
        this.changeRef.detectChanges();
        this.updateImages(2500);
    }
    hasSelected() {
        return this.songs.filter(s => s.Selected).length;
    }
    addSelected(pack) {
        const songsToMove = this.songs.filter(s => s.Selected);
        songsToMove.forEach(s => {
            const msg = new ClientMoveSongToPlaylist();
            msg.ToPlaylistID = pack.PlaylistID;
            msg.SongID = s.SongID;
            //msg.Index = 0;
            this.msgSvc.sendClientMessage(msg);
        });
        pack.SongList = pack.SongList.concat(songsToMove);
        this.songs.forEach(s => (s.selected = false));
        this.checkboxChecked = false;
    }
    getBackground(SongID) {
        return AppSettings.API_ENDPOINT + '/host/beatsaber/song/cover?songid=' + SongID;
    }
    updateImages(timeout: number) {
        setTimeout(() => this.updateSearchResult.next(), timeout);
    }
    getPackBackground(PlaylistID) {
        let fixedUri = encodeURIComponent(PlaylistID);
        fixedUri = fixedUri.replace('(', '%28').replace(')', '%29');
        return (
            AppSettings.API_ENDPOINT + '/host/beatsaber/playlist/cover?playlistid=' + fixedUri + '&updateCount=' + this.updateHack
        );
    }
    sortAuthor(PlaylistID: string) {
        this.sortPack(PlaylistID, PlaylistSortMode.LevelAuthor);
    }
    sortDifficulty(PlaylistID: string) {
        this.sortPack(PlaylistID, PlaylistSortMode.MaxDifficulty);
    }
    sortName(PlaylistID: string) {
        this.sortPack(PlaylistID, PlaylistSortMode.Name);
    }
    sortPack(PlaylistID: string, mode: PlaylistSortMode) {
        this.reverseSortToggle = !this.reverseSortToggle;
        const sort = new ClientSortPlaylist();
        sort.PlaylistID = PlaylistID;
        sort.SortMode = mode;
        sort.Reverse = this.reverseSortToggle;
        this.msgSvc.sendClientMessage(sort);
    }
    addEditPlaylist(playlist?: BeatSaberPlaylist) {
        console.log('here');
        const dialogRef = this.dialog.open(AddEditPlaylistDialogComponent, {
            width: '450px',
            height: '320px',
            disableClose: true,
            data: { playlist: playlist || <BeatSaberPlaylist>{}, isNew: !playlist },
        });
        dialogRef.afterClosed().subscribe(res => this.dialogClosed(res));
    }
    dialogClosed(result) {
        //if cancelled
        if (result == null) return;
        if (result['deletePlaylist'] === true) {
            this.removePlaylist(result.playlist.PlaylistID);
        } else {
            //must be a save
            if (result.isNew) {
                this.configSvc.getConfig().subscribe(cfg => {
                    cfg.Config.Playlists.push(result.playlist);
                    const msg = new ClientAddOrUpdatePlaylist();
                    msg.Playlist = result.playlist;
                    this.msgSvc.sendClientMessage(msg);
                });
            } else {
                this.configSvc.getConfig().subscribe(cfg => {
                    let found = null;
                    cfg.Config.Playlists.forEach(p => {
                        if (p.PlaylistID == result.playlist.PlaylistID) {
                            found = p;
                            if (result.playlist.CoverImageBytes && result.playlist.CoverImageBytes > 50) {
                                p.CoverImageBytes = result.playlist.CoverImageBytes;
                            }
                            p.PlaylistName = result.playlist.PlaylistName;
                        }
                    });
                    if (found) {
                        const msg = new ClientAddOrUpdatePlaylist();
                        msg.Playlist = found;
                        this.msgSvc.sendClientMessage(msg);
                        var sub;
                        sub = this.msgSvc.configChangeMessage.subscribe(cfg => {
                            sub.unsubscribe();
                            this.updateHack = new Date().getTime();
                        });
                    }
                });
            }
        }
    }
    removePlaylist(PlaylistID) {
        const msg = new ClientDeletePlaylist();
        msg.PlaylistID = PlaylistID;
        this.msgSvc.sendClientMessage(msg);
    }
    removeSong(SongID) {
        const msg = new ClientDeleteSong();
        msg.SongID = SongID;
        this.msgSvc.sendClientMessage(msg);
    }
    removeSongFromPack(song, pack) {
        if (this.packs.findIndex(x => x.PlaylistID == 'CustomSongs') < 0) {
            const pl: BeatSaberPlaylist = <BeatSaberPlaylist>{ SongList: [] };
            pl.PlaylistID = 'CustomSongs';
            pl.PlaylistName = 'Custom Songs';
            const plMsg = new ClientAddOrUpdatePlaylist();
            plMsg.Playlist = pl;
            this.msgSvc.sendClientMessage(plMsg);
        }
        const msg = new ClientMoveSongToPlaylist();
        msg.ToPlaylistID = 'CustomSongs';
        msg.SongID = song.SongID;
        msg.Index = this.songs.length;
        this.msgSvc.sendClientMessage(msg);
        this.songs.push(song);
        pack.SongList = pack.SongList.filter(s => s !== song);
    }
    selectAll() {
        this.selectAllToggle = !this.selectAllToggle;
        this.checkboxChecked = true;
        this.songs.forEach(s => {
            s.Selected = this.selectAllToggle;
        });
    }
    autoSortName(max: number) {
        var msg = new ClientAutoCreatePlaylists();
        msg.MaxPerNamePlaylist = max;
        msg.SortMode = PlaylistSortMode.Name;
        this.msgSvc.sendClientMessage(msg);
    }
    autoSortDifficulty() {
        var msg = new ClientAutoCreatePlaylists();
        msg.MaxPerNamePlaylist = 5;
        msg.SortMode = PlaylistSortMode.MaxDifficulty;
        this.msgSvc.sendClientMessage(msg);
    }
}
