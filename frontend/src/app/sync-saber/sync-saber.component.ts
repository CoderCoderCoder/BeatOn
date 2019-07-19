import { Component, OnInit } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { FeedReader } from '../models/FeedReader';
import Timer = NodeJS.Timer;
import { PlaylistSortMode } from '../models/PlaylistSortMode';
import { ClientAutoCreatePlaylists } from '../models/ClientAutoCreatePlaylists';
import { HostMessageService } from '../services/host-message.service';
import { ClientSetBeastSaberUsername } from '../models/ClientSetBeastSaberUsername';
import { ClientUpdateFeedReader } from '../models/ClientUpdateFeedReader';
import { ClientSyncSaberSync } from '../models/ClientSyncSaberSync';
import { HostActionResponse } from '../models/HostActionResponse';
import { COMMA, ENTER } from '@angular/cdk/keycodes';
import { MatChipInputEvent } from '@angular/material';

@Component({
    selector: 'app-sync-saber',
    templateUrl: './sync-saber.component.html',
    styleUrls: ['./sync-saber.component.scss'],
})
export class SyncSaberComponent implements OnInit {
    BeastSaberUsername: string;
    SyncService_BeastSaberFOLLOWING: FeedReader;
    SyncService_BeastSaberBOOKMARKS: FeedReader;
    SyncService_BeastSaberCURATOR_RECOMMENDED: FeedReader;
    SyncService_ScoreSaberTRENDING: FeedReader;
    SyncService_ScoreSaberLATEST_RANKED: FeedReader;
    SyncService_ScoreSaberTOP_PLAYED: FeedReader;
    SyncService_ScoreSaberTOP_RANKED: FeedReader;
    SyncService_BeatSaverAUTHOR: FeedReader;
    SyncService_BeatSaverLATEST: FeedReader;
    SyncService_BeatSaverHOT: FeedReader;
    SyncService_BeatSaverDOWNLOADS: FeedReader;
    syncDisabled: boolean = false;
    nameUpdateTimeout: Timer;
    readonly separatorKeysCodes: number[] = [ENTER, COMMA];
    constructor(private beatOnApi: BeatOnApiService, private configSvc: ConfigService, private msgSvc: HostMessageService) {}

    ngOnInit() {
        this.configSvc.getConfig().subscribe(this.handleConfig.bind(this));
        this.configSvc.configUpdated.subscribe(this.handleConfig.bind(this));
    }

    setBeastSaberUsername() {
        clearTimeout(this.nameUpdateTimeout);
        this.nameUpdateTimeout = setTimeout(() => {
            const msg = new ClientSetBeastSaberUsername();
            msg.BeastSaberUsername = this.BeastSaberUsername;
            this.msgSvc.sendClientMessage(msg);
        }, 750);
    }
    feedSettingTimeouts: any = {};
    updateFeedSetting(reader: FeedReader) {
        if (this.feedSettingTimeouts[reader.ID]) {
            clearTimeout(this.feedSettingTimeouts[reader.ID]);
            this.feedSettingTimeouts[reader.ID] = null;
        }
        this.feedSettingTimeouts[reader.ID] = setTimeout(() => {
            const msg = new ClientUpdateFeedReader();
            msg.FeedConfig = reader;
            msg.ID = reader.ID;
            this.msgSvc.sendClientMessage(msg);
        }, 750);
    }
    clickSyncNow() {
        const msg = new ClientSyncSaberSync();
        this.msgSvc.actionResponseMessage.subscribe((rsp: HostActionResponse) => {
            if (rsp.ResponseToMessageID == msg.MessageID) {
                this.syncDisabled = false;
            }
        });
        this.syncDisabled = true;
        this.msgSvc.sendClientMessage(msg);
    }
    addAuthor(event: MatChipInputEvent) {
        const input = event.input;
        const value = event.value;
        if ((value || '').trim()) {
            this.SyncService_BeatSaverAUTHOR.Authors.push(value.trim());
        }
        if (input) {
            input.value = '';
        }
        this.updateFeedSetting(this.SyncService_BeatSaverAUTHOR);
    }
    removeAuthor(author) {
        const index = this.SyncService_BeatSaverAUTHOR.Authors.indexOf(author);

        if (index >= 0) {
            this.SyncService_BeatSaverAUTHOR.Authors.splice(index, 1);
        }
        this.updateFeedSetting(this.SyncService_BeatSaverAUTHOR);
    }
    handleConfig(cfg: BeatOnConfig) {
        this.BeastSaberUsername = cfg.SyncConfig.BeastSaberUsername;
        cfg.SyncConfig.FeedReaders.forEach((reader: FeedReader) => {
            reader.MaxSongs = reader.MaxSongs || 0;
            switch (reader.PlaylistID) {
                case 'SyncService_BeastSaberFOLLOWING':
                    this.SyncService_BeastSaberFOLLOWING = reader;
                    break;
                case 'SyncService_BeastSaberBOOKMARKS':
                    this.SyncService_BeastSaberBOOKMARKS = reader;
                    break;
                case 'SyncService_BeastSaberCURATOR_RECOMMENDED':
                    this.SyncService_BeastSaberCURATOR_RECOMMENDED = reader;
                    break;
                case 'SyncService_ScoreSaberTRENDING':
                    this.SyncService_ScoreSaberTRENDING = reader;
                    break;
                case 'SyncService_ScoreSaberLATEST_RANKED':
                    this.SyncService_ScoreSaberLATEST_RANKED = reader;
                    break;
                case 'SyncService_ScoreSaberTOP_PLAYED':
                    this.SyncService_ScoreSaberTOP_PLAYED = reader;
                    break;
                case 'SyncService_ScoreSaberTOP_RANKED':
                    this.SyncService_ScoreSaberTOP_RANKED = reader;
                    break;
                case 'SyncService_BeatSaverAUTHOR':
                    this.SyncService_BeatSaverAUTHOR = reader;
                    break;
                case 'SyncService_BeatSaverLATEST':
                    this.SyncService_BeatSaverLATEST = reader;
                    break;
                case 'SyncService_BeatSaverHOT':
                    this.SyncService_BeatSaverHOT = reader;
                    break;
                case 'SyncService_BeatSaverDOWNLOADS':
                    this.SyncService_BeatSaverDOWNLOADS = reader;
                    break;
            }
        });
    }
}
