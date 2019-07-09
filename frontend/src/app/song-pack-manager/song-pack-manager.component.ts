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
    ElementRef,
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
import { AppIntegrationService } from '../services/app-integration.service';
import { AppButtonEvent, AppButtonType } from '../models/AppButtonEvent';
import { CdkDragPlaceholder } from '@angular/cdk/drag-drop';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { BeatSaberSong } from '../models/BeatSaberSong';
import { NoCustomSongsPipe } from '../pipes/no-custom-songs';
import { OnlyCustomSongsPipe } from '../pipes/only-custom-songs';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { ExtendedScrollToOptions } from '@angular/cdk/scrolling';
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
export class SongPackManagerComponent implements OnInit {
    @ViewChild('song_container', { static: false }) song_container;
    @ViewChild('pack_song_container', { static: false }) pack_song_container;
    @ViewChild('pack_container', { static: false }) pack_container;
    @ViewChild('mirror_holder', { static: false }) mirror_holder;
    @ViewChild('empty', { static: true }) empty;
    selectedPlaylist: BeatSaberPlaylist;
    packs: BeatSaberPlaylist[];
    songs: BeatSaberSong[];
    songsPack: BeatSaberPlaylist;
    private config: BeatOnConfig;
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
    private SCROLL_ACCEL_INCREMENT: number = 50;
    private SCROLL_ACCEL_DELAY_MS: number = 350;
    private scrollLastButton;
    private scrollAccel: number = 0;
    private scrollLastTime: number = 0;
    public constructor(
        private dialog: MatDialog,
        private configSvc: ConfigService,
        private msgSvc: HostMessageService,
        public integrationService: AppIntegrationService,
        private nocustomsongs: NoCustomSongsPipe,
        private onlycustomsongs: OnlyCustomSongsPipe
    ) {
        this.updateSearchResult = new Subject();
        this.subs.add(
            this.integrationService.appButtonPressed.subscribe((be: AppButtonEvent) => {
                const SCROLL_SIZE: number = 300;
                if (
                    this.scrollLastButton == be.button &&
                    new Date().getTime() - this.scrollLastTime <= this.SCROLL_ACCEL_DELAY_MS
                ) {
                    this.scrollAccel = this.scrollAccel + this.SCROLL_ACCEL_INCREMENT;
                } else {
                    this.scrollAccel = 0;
                }
                this.scrollLastButton = be.button;
                this.scrollLastTime = new Date().getTime();
                var scrollFunc = ele => {
                    if (be.button == AppButtonType.Down) {
                        if (ele.scrollTop < ele.scrollHeight - ele.offsetHeight) {
                            var scrollAmt = ele.scrollHeight - ele.offsetHeight - ele.scrollTop;
                            if (scrollAmt > SCROLL_SIZE) scrollAmt = SCROLL_SIZE;
                            ele.scrollTo(ele.scrollLeft, ele.scrollTop + scrollAmt + this.scrollAccel);
                        }
                    } else if (be.button == AppButtonType.Up) {
                        if (ele.scrollTop > 0) {
                            var scrollAmt: number = ele.scrollTop;
                            if (scrollAmt > SCROLL_SIZE) scrollAmt = SCROLL_SIZE;
                            ele.scrollTo(ele.scrollLeft, ele.scrollTop - (scrollAmt + this.scrollAccel));
                        }
                    }
                };
                var cdkScrollFunc = ele => {
                    var ex: ExtendedScrollToOptions = <ExtendedScrollToOptions>{};
                    ex.left = 0;
                    if (be.button == AppButtonType.Down) {
                        ex.bottom = ele.measureScrollOffset('bottom') - (SCROLL_SIZE + this.scrollAccel);
                        if (ex.bottom < 0) ex.bottom = 0;
                        ele.scrollTo(ex);
                    } else if (be.button == AppButtonType.Up) {
                        ex.top = ele.measureScrollOffset('top') - (SCROLL_SIZE + this.scrollAccel);
                        if (ex.top < 0) ex.top = 0;
                        ele.scrollTo(ex);
                    }
                };
                var bounds;
                if (this.song_container) {
                    bounds = this.song_container.elementRef.nativeElement.getBoundingClientRect();
                    if (be.x >= bounds.left && be.x <= bounds.right && be.y >= bounds.top && be.y <= bounds.bottom) {
                        //pointer is in the song container when the button was pressed
                        cdkScrollFunc(this.song_container);
                        return;
                    }
                }
                if (this.pack_container) {
                    bounds = this.pack_container.nativeElement.getBoundingClientRect();
                    if (be.x >= bounds.left && be.x <= bounds.right && be.y >= bounds.top && be.y <= bounds.bottom) {
                        //pointer is in the packs container when the button was pressed
                        scrollFunc(this.pack_container.nativeElement);
                        return;
                    }
                }
                if (this.pack_song_container) {
                    bounds = this.pack_song_container.elementRef.nativeElement.getBoundingClientRect();
                    if (be.x >= bounds.left && be.x <= bounds.right && be.y >= bounds.top && be.y <= bounds.bottom) {
                        //pointer is in the packs container when the button was pressed
                        cdkScrollFunc(this.pack_song_container);
                        return;
                    }
                }
                //check any other elements that may need to respond to button presses
            })
        );
    }

    ngOnDestroy() {
        this.subs.unsubscribe();
    }
    ngOnInit() {
        this.configSvc.getConfig().subscribe(this.handleConfig.bind(this));
        this.configSvc.configUpdated.subscribe(this.handleConfig.bind(this));
    }
    ngAfterViewInit() {}
    hasSelected() {
        return this.songs.filter(s => s.Selected).length;
    }
    private lastDragPointer;
    dragMove(e) {
        this.lastDragPointer = e.pointerPosition;
    }
    handleConfig(cfg: BeatOnConfig) {
        this.config = cfg;
        this.packs = this.nocustomsongs.transform(cfg.Config.Playlists);
        var customSongsPack = this.onlycustomsongs.transform(cfg.Config.Playlists);
        if (customSongsPack.length < 1) {
            const pl: BeatSaberPlaylist = <BeatSaberPlaylist>{ SongList: [] };
            pl.PlaylistID = 'CustomSongs';
            pl.PlaylistName = 'Custom Songs';
            const plMsg = new ClientAddOrUpdatePlaylist();
            plMsg.Playlist = pl;
            this.msgSvc.sendClientMessage(plMsg);
            this.songsPack = pl;
        } else {
            this.songsPack = customSongsPack[0];
        }
        this.songs = this.songsPack.SongList;
    }
    drop(data) {
        const target_pack = data.container.element.nativeElement as HTMLElement;
        const item_element = data.item.element.nativeElement as HTMLElement;
        var playlistId;
        if (this.pack_container && this.pack_container.id == data.container.element.id && item_element.dataset.song_id) {
            //if it's being dropped on top of a playlist, find the playlist under the coordinates of the drop.  cdk dragdrop isn't great in some ways.
            if (this.pack_container && data.container.element.nativeElement === this.pack_container.nativeElement) {
                var findPl;
                findPl = ele => {
                    if (ele == null) return null;
                    if (ele.attributes['data-playlist_id']) {
                        return ele.attributes['data-playlist_id'].value;
                    } else {
                        if (ele.id == target_pack.id) return;
                        else if (ele.parentElement == null) return null;
                        else return findPl(ele.parentElement);
                    }
                };
                playlistId = findPl(document.elementFromPoint(this.lastDragPointer.x, this.lastDragPointer.y));
            }
            if (playlistId == null) {
                playlistId = this.packs[data.currentIndex].PlaylistID;
            }
        } else {
            playlistId = target_pack.dataset.playlist_id;
        }
        let index = data.currentIndex;
        if (!this.selectedPlaylist && target_pack === this.pack_container.nativeElement && !item_element.dataset.song_id) {
            const csongsindex = this.config.Config.Playlists.findIndex(x => x.PlaylistID == this.songsPack.PlaylistID);
            if (csongsindex > -1 && index > csongsindex) index = index + 1;
            const msg = new ClientMovePlaylist();
            msg.PlaylistID = item_element.dataset.playlist_id;
            msg.Index = index;
            this.msgSvc.sendClientMessage(msg);
        } else {
            const rr = this.song_container.getRenderedRange();
            var rrPack;
            if (this.pack_song_container) {
                rrPack = this.pack_song_container.getRenderedRange();
            }
            let songId = item_element.dataset.song_id;
            let lastIndex = data.currentIndex;
            var from: string;
            var to: string;
            if (data.container.element.nativeElement === this.song_container.elementRef.nativeElement) {
                to = 'songs';
                index = data.currentIndex + rr.start;
            } else if (
                this.pack_song_container &&
                data.container.element.nativeElement === this.pack_song_container.elementRef.nativeElement
            ) {
                to = 'packsongs';
                index = data.currentIndex + rrPack.start;
            } else if (this.pack_container && data.container.element.nativeElement == this.pack_container.nativeElement) {
                to = 'pack';
            }
            if (data.previousContainer.element.nativeElement == this.song_container.elementRef.nativeElement) {
                from = 'songs';
                lastIndex = lastIndex + rr.start;
            } else if (
                this.pack_song_container &&
                data.previousContainer.element.nativeElement == this.pack_song_container.elementRef.nativeElement
            ) {
                from = 'packsongs';
                lastIndex = lastIndex + rrPack.start;
            } else if (this.pack_container && data.previousContainer.element.nativeElement == this.pack_container.nativeElement) {
                from = 'pack';
            }
            if (from == 'songs' && to == 'pack') {
                const pl = this.packs.find(x => x.PlaylistID == playlistId);
                if (pl) {
                    index = pl.SongList.length;
                } else {
                    index = 0;
                }
            }
            console.log(
                'drop moving songid from ' +
                    from +
                    ' at index ' +
                    lastIndex +
                    ' to ' +
                    to +
                    ' (playlist ID ' +
                    playlistId +
                    ') at index ' +
                    index
            );
            if (from == 'pack' && to == 'songs') {
                var nextIndex = index;
                data.item.data.SongList.forEach(x => {
                    const msg = new ClientMoveSongToPlaylist();
                    msg.ToPlaylistID = playlistId;
                    msg.SongID = x.SongID;
                    msg.Index = nextIndex;
                    this.msgSvc.sendClientMessage(msg);
                    nextIndex = nextIndex + 1;
                });
                this.removePlaylist(data.item.data.PlaylistID);
            } else {
                if (from == to) {
                    const msg = new ClientMoveSongInPlaylist();
                    msg.SongID = songId;
                    msg.Index = index;
                    this.msgSvc.sendClientMessage(msg);
                } else {
                    const msg = new ClientMoveSongToPlaylist();
                    msg.ToPlaylistID = playlistId;
                    msg.SongID = songId;
                    msg.Index = index;
                    this.msgSvc.sendClientMessage(msg);
                }
                data.previousContainer.data.splice(lastIndex, 1);
                data.container.data.splice(index, 0, data.item.data);
            }
        }
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
        this.songs.forEach(s => (s.Selected = false));
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
        const dialogRef = this.dialog.open(AddEditPlaylistDialogComponent, {
            width: '450px',
            height: '320px',
            disableClose: true,
            data: {
                playlist: playlist
                    ? <BeatSaberPlaylist>{ PlaylistID: playlist.PlaylistID, PlaylistName: playlist.PlaylistName }
                    : null || <BeatSaberPlaylist>{},
                isNew: !playlist,
            },
        });
        dialogRef.afterClosed().subscribe(res => this.dialogClosed(res));
    }
    deletePlaylist(playlist: BeatSaberPlaylist) {
        if (!playlist.SongList.length) {
            this.removePlaylist(playlist.PlaylistID);
        } else {
            const dialogRef = this.dialog.open(ConfirmDialogComponent, {
                width: '450px',
                height: '200px',
                disableClose: true,
                data: {
                    title: 'Delete ' + playlist.PlaylistName + '?',
                    subTitle:
                        'Are you sure you want to delete this playlist' +
                        (playlist.SongList.length ? ' and all ' + playlist.SongList.length + ' songs on it?' : '?'),
                    button1Text: 'Yes',
                },
            });
            dialogRef.afterClosed().subscribe(res => {
                if (res == 1) {
                    this.removePlaylist(playlist.PlaylistID);
                }
            });
        }
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
                            if (result.playlist.CoverImageBytes && result.playlist.CoverImageBytes.length > 50) {
                                found.CoverImageBytes = result.playlist.CoverImageBytes;
                            }
                            found.PlaylistName = result.playlist.PlaylistName;
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
    openPack() {}
}
