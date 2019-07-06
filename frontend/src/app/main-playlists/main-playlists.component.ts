import { Component, OnInit, ViewChild } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { ConfigService } from '../services/config.service';
import { PlaylistTempConfig } from '../models/PlaylistTempConfig';
import { ClientAddOrUpdatePlaylist } from '../models/ClientAddOrUpdatePlaylist';
import { HostMessageService } from '../services/host-message.service';

@Component({
    selector: 'app-main-playlists',
    templateUrl: './main-playlists.component.html',
    styleUrls: ['./main-playlists.component.scss'],
    host: {
        class: 'fullheight',
    },
})
export class MainPlaylistsComponent implements OnInit {
    @ViewChild('songManager', { static: false }) songManager;
    config: QuestomConfig;
    selectedPlaylist: BeatSaberPlaylist = <BeatSaberPlaylist>{};
    customPlaylist: BeatSaberPlaylist;
    hasLastConfig: boolean;
    constructor(private beatOnApi: BeatOnApiService, private configSvc: ConfigService, private msgSvc: HostMessageService) {}

    ngOnInit() {
        this.configSvc.getConfig().subscribe(this.handleConfig.bind(this));
        this.configSvc.configUpdated.subscribe(this.handleConfig.bind(this));
    }

    selectedPlaylistChanged(ev) {
        this.selectedPlaylist = ev;
    }

    saveTempConfig() {
        this.hasLastConfig = !!this.config;
        let playlistTempConfig: PlaylistTempConfig[] = [];
        if (this.config && this.config.Playlists) {
            playlistTempConfig = this.config.Playlists.map(p => ({ PlaylistID: p.PlaylistID, IsOpen: p.IsOpen }));
        }
        return playlistTempConfig;
    }

    restoreTempConfig(playlistTempConfig: PlaylistTempConfig[]) {
        if (this.config && this.config.Playlists) {
            this.config.Playlists.forEach(p => {
                const isOpen = playlistTempConfig.filter(_p => _p.PlaylistID == p.PlaylistID);
                if (isOpen.length) {
                    p.IsOpen = isOpen[0].IsOpen;
                }
            });
            playlistTempConfig = this.config.Playlists.map(p => ({ PlaylistID: p.PlaylistID, IsOpen: p.IsOpen }));
        }
    }

    handleConfig(cfg: BeatOnConfig) {
        const tempConfig = this.saveTempConfig();
        this.config = cfg.Config;
        this.restoreTempConfig(tempConfig);
        this.setupPlaylists();
    }

    setupPlaylists() {
        if (this.config.Playlists.findIndex(x => x.PlaylistID == 'CustomSongs') < 0) {
            console.log('making pl');
            const pl: BeatSaberPlaylist = <BeatSaberPlaylist>{ SongList: [] };
            pl.PlaylistID = 'CustomSongs';
            pl.PlaylistName = 'Custom Songs';
            const plMsg = new ClientAddOrUpdatePlaylist();
            plMsg.Playlist = pl;
            this.msgSvc.sendClientMessage(plMsg);
        }
        const customIndex = this.config.Playlists.map(p => p.PlaylistID).indexOf('CustomSongs');
        if (customIndex > -1) {
            this.customPlaylist = this.config.Playlists[customIndex];
        } else {
            this.customPlaylist = {
                CoverArtFilename: null,
                PlaylistID: '',
                PlaylistName: '',
                SongList: [],
                CoverImageBytes: null,
            };
        }
    }
}
