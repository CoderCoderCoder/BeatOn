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
import { AppIntegrationService } from '../services/app-integration.service';
import { AppButtonEvent, AppButtonType } from '../models/AppButtonEvent';
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
export class SongPackManagerComponent implements OnInit, OnChanges {
    @Input('packs') packs;
    @Input('songs') songs;
    @Input('songsPack') songsPack;
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
        private msgSvc: HostMessageService,
        private changeRef: ChangeDetectorRef,
        public integrationService: AppIntegrationService
    ) {
        this.updateSearchResult = new Subject();
        // this.dragulaService.createGroup(this.BAG, {
        //     copy: (el, source) => {
        //         return false;
        //     },
        //     accepts: (el, target) => {
        //         return (
        //             //target !== this.song_container.nativeElement &&
        //             (el.parentElement === this.pack_container.nativeElement && target === this.pack_container.nativeElement) ||
        //             (el.parentElement !== this.pack_container.nativeElement && target !== this.pack_container.nativeElement)
        //         );
        //     },
        //     moves: (el, source, handle, sibling) => {
        //         return !!~handle.className.indexOf('handle'); // elements are always draggable by default
        //     },
        //     copyItem: (item: any) => ({ ...item }),
        // });
        // this.subs.add(
        //     dragulaService.drop(this.BAG).subscribe(({ name, el, target, source, sibling }) => {
        //         let index;
        //         let playlistId = (target as HTMLElement).dataset.playlist_id;
        //         if (!sibling) {
        //             if (target === this.pack_container.nativeElement) {
        //                 index = this.packs.length + 1; // add one for the custom songs removed
        //             }
        //         } else {
        //             index = Array.prototype.indexOf.call(sibling.parentNode.childNodes, sibling) - 2;
        //         }
        //         if (target === this.pack_container.nativeElement) {
        //             const msg = new ClientMovePlaylist();
        //             msg.PlaylistID = (el as HTMLElement).dataset.playlist_id;
        //             msg.Index = index;
        //             this.msgSvc.sendClientMessage(msg);
        //         } else {
        //             let songId = (el as HTMLElement).dataset.song_id;
        //             if (target === source) {
        //                 const msg = new ClientMoveSongInPlaylist();
        //                 msg.SongID = songId;
        //                 msg.Index = index;
        //                 this.msgSvc.sendClientMessage(msg);
        //             } else {
        //                 if (playlistId == 'CustomSongs' && this.packs.findIndex(x => x.PlaylistID == 'CustomSongs') < 0) {
        //                     console.log('making pl');
        //                     const pl: BeatSaberPlaylist = <BeatSaberPlaylist>{ SongList: [] };
        //                     pl.PlaylistID = 'CustomSongs';
        //                     pl.PlaylistName = 'Custom Songs';
        //                     const plMsg = new ClientAddOrUpdatePlaylist();
        //                     plMsg.Playlist = pl;
        //                     this.msgSvc.sendClientMessage(plMsg);
        //                 }
        //                 const msg = new ClientMoveSongToPlaylist();
        //                 msg.ToPlaylistID = playlistId;
        //                 msg.SongID = songId;
        //                 msg.Index = index;
        //                 this.msgSvc.sendClientMessage(msg);
        //             }
        //         }
        //     })
        // );
        this.subs.add(
            this.integrationService.appButtonPressed.subscribe((be: AppButtonEvent) => {
                const SCROLL_SIZE: number = 300;
                var scrollFunc = ele => {
                    if (be.button == AppButtonType.Down) {
                        if (ele.scrollTop < ele.scrollHeight - ele.offsetHeight) {
                            var scrollAmt = ele.scrollHeight - ele.offsetHeight - ele.scrollTop;
                            if (scrollAmt > SCROLL_SIZE) scrollAmt = SCROLL_SIZE;
                            ele.scrollTo(ele.scrollLeft, ele.scrollTop + scrollAmt);
                        }
                    } else if (be.button == AppButtonType.Up) {
                        if (ele.scrollTop > 0) {
                            var scrollAmt: number = ele.scrollTop;
                            if (scrollAmt > SCROLL_SIZE) scrollAmt = SCROLL_SIZE;
                            ele.scrollTo(ele.scrollLeft, ele.scrollTop - scrollAmt);
                        }
                    }
                };
                var bounds = this.song_container.nativeElement.getBoundingClientRect();
                if (be.x >= bounds.left && be.x <= bounds.right && be.y >= bounds.top && be.y <= bounds.bottom) {
                    //pointer is in the song container when the button was pressed
                    scrollFunc(this.song_container.nativeElement);
                    return;
                }
                bounds = this.pack_container.nativeElement.getBoundingClientRect();
                if (be.x >= bounds.left && be.x <= bounds.right && be.y >= bounds.top && be.y <= bounds.bottom) {
                    //pointer is in the packs container when the button was pressed
                    scrollFunc(this.pack_container.nativeElement);
                    return;
                }
                //check any other elements that may need to respond to button presses
            })
        );
        // this.subs.add(
        //     this.msgSvc.configChangeMessage.subscribe(cfg => {
        //         console.log('Cancelling drag in progress because a config update came in.');
        //         dragulaService.find(this.BAG).drake.cancel(true);
        //     })
        // );
    }
    ngOnDestroy() {
        this.dragulaService.destroy('SONGS');
        this.subs.unsubscribe();
    }
    ngOnInit() {}
    ngOnChanges(changes: SimpleChanges) {
        const doScrollThingie = ele => {
            if (ele && ele.scrollHeight > ele.clientHeight) {
                var lastPackScrollOffset = ele.scrollTop;
                var listener = () => {
                    setTimeout(() => {
                        var old = ele.style.scrollBehavior;
                        ele.style.scrollBehavior = 'unset';
                        ele.scrollTop = lastPackScrollOffset;
                        ele.style.scrollBehavior = old;
                        ele.removeEventListener('scroll', listener);
                    }, 0);
                };
                ele.addEventListener('scroll', listener);
            }
        };
        if (this.pack_container) {
            doScrollThingie(this.pack_container.nativeElement);
        }
        if (this.song_container) {
            doScrollThingie(this.song_container.nativeElement);
        }
    }
    ngAfterViewInit() {
        // let drake = this.dragulaService.find('SONGS').drake;
        // let scroll = autoScroll([window, this.pack_container.nativeElement], {
        //     margin: 150,
        //     autoScroll: function() {
        //         return this.down && drake.dragging;
        //     },
        // });
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
    drop(data) {
        const pack_container = data.container.element.nativeElement as HTMLElement;
        const item_element = data.item.element.nativeElement as HTMLElement;
        const playlistId = pack_container.dataset.playlist_id;
        const index = data.currentIndex;
        if (pack_container === this.pack_container.nativeElement) {
            const msg = new ClientMovePlaylist();
            msg.PlaylistID = item_element.dataset.playlist_id;
            msg.Index = index;
            this.msgSvc.sendClientMessage(msg);
        } else {
            let songId = item_element.dataset.song_id;
            if (pack_container === data.previousContainer.element.nativeElement) {
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
                            console.log(result.playlist.CoverImageBytes);
                            if (result.playlist.CoverImageBytes && result.playlist.CoverImageBytes.length > 50) {
                                console.log('yesimage');
                                found.CoverImageBytes = result.playlist.CoverImageBytes;
                            }
                            found.PlaylistName = result.playlist.PlaylistName;
                        }
                    });
                    if (found) {
                        const msg = new ClientAddOrUpdatePlaylist();
                        msg.Playlist = found;
                        console.log(JSON.stringify(msg));
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
