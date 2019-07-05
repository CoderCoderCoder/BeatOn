import { Component, OnInit } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { ModDefinition, ModStatusType } from '../models/ModDefinition';
import { ClientSetModStatus } from '../models/ClientSetModStatus';
import { MatSlideToggleChange, MatDialog } from '@angular/material';
import { HostActionResponse } from '../models/HostActionResponse';
import { ECANCELED } from 'constants';
import { ClientDeleteMod } from '../models/ClientDeleteMod';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { ClientDeleteSong } from '../models/ClientDeleteSong.cs';

@Component({
    selector: 'app-main-mods',
    templateUrl: './main-mods.component.html',
    styleUrls: ['./main-mods.component.scss'],
})
export class MainModsComponent implements OnInit {
    config: QuestomConfig = <QuestomConfig>{ Mods: [] };
    modSwitchInProgress: boolean = false;
    modIDBeingSwitched: string = null;
    constructor(
        private configSvc: ConfigService,
        private beatOnApi: BeatOnApiService,
        private msgSvc: HostMessageService,
        private dialog: MatDialog
    ) {
        this.configSvc.configUpdated.subscribe((cfg: BeatOnConfig) => {
            this.config = cfg.Config;
        });
    }

    ngOnInit() {
        this.configSvc.getConfig().subscribe((cfg: BeatOnConfig) => {
            this.config = cfg.Config;
        });
    }

    toggleMod(ev: MatSlideToggleChange, mod: ModDefinition) {
        this.modIDBeingSwitched = mod.ID;
        this.modSwitchInProgress = true;
        var msg = new ClientSetModStatus();
        msg.ModID = mod.ID;
        msg.Status = ev.checked ? ModStatusType.Installed : ModStatusType.NotInstalled;
        console.log('sending message for mod ID ' + msg.ModID);
        var sub;
        sub = this.msgSvc.actionResponseMessage.subscribe((ev: HostActionResponse) => {
            if (ev.ResponseToMessageID == msg.MessageID) {
                console.log('Got response message in mods for mod ID ' + msg.ModID);
                this.modIDBeingSwitched = null;
                this.modSwitchInProgress = false;
                sub.unsubscribe();
                if (!ev.Success) {
                    //todo: show error
                    console.log('mod id ' + msg.ModID + ' did not switch properly');
                }
            }
        });
        this.msgSvc.sendClientMessage(msg);
    }
    getModSwitch(mod) {
        console.log('getting mod status for mod id ' + mod.ID);
        if (mod == null) return false;
        if (
            (mod.Status != 'Installed' && mod.ID != this.modIDBeingSwitched) ||
            (mod.Status == 'Installed' && mod.ID == this.modIDBeingSwitched)
        )
            return false;
        return true;
    }

    clickDeleteMod(mod) {
        var dialogRef = this.dialog.open(ConfirmDialogComponent, {
            width: '450px',
            height: '180px',
            disableClose: true,
            data: { title: 'Delete ' + mod.Name + '?', subTitle: 'Are you sure you want to delete this mod?', button1Text: 'Yes' },
        });
        dialogRef.afterClosed().subscribe(res => {
            if (res == 1) {
                this.modSwitchInProgress = true;
                var msg = new ClientDeleteMod();
                msg.ModID = mod.ID;
                var sub;
                sub = this.msgSvc.actionResponseMessage.subscribe((ev: HostActionResponse) => {
                    if (ev.ResponseToMessageID == msg.MessageID) {
                        console.log('Got response message in mods for mod ID ' + msg.ModID);
                        this.modIDBeingSwitched = null;
                        this.modSwitchInProgress = false;
                        sub.unsubscribe();
                        if (!ev.Success) {
                            //todo: show error
                            console.log('mod id ' + msg.ModID + ' could not delete!');
                        }
                    }
                });
                console.log('Sending message to delete mod id ' + mod.ID);
                this.msgSvc.sendClientMessage(msg);
            }
        });
    }
}
