import { AfterViewInit, Component, OnInit } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { ModCategory, ModDefinition, ModStatusType } from '../models/ModDefinition';
import { ClientSetModStatus } from '../models/ClientSetModStatus';
import { MatSlideToggleChange, MatDialog } from '@angular/material';
import { HostActionResponse } from '../models/HostActionResponse';
import { ECANCELED } from 'constants';
import { NgxSmartModalService } from 'ngx-smart-modal';
import { ConfirmDialogComponent } from '../confirm-dialog/confirm-dialog.component';
import { ClientDeleteMod } from '../models/ClientDeleteMod';
import { AppSettings } from '../appSettings';
import { BeatSaberColor } from '../models/BeatSaberColor';
import { ClientChangeColor, ColorType } from '../models/ClientChangeColor';

@Component({
    selector: 'app-main-mods',
    templateUrl: './main-mods.component.html',
    styleUrls: ['./main-mods.component.scss'],
})
export class MainModsComponent implements OnInit, AfterViewInit {
    config: QuestomConfig = <QuestomConfig>{ Mods: [] };
    modSwitchInProgress: boolean = false;
    modIDBeingSwitched: string = null;
    selectedMod: ModDefinition;
    constructor(
        private configSvc: ConfigService,
        private beatOnApi: BeatOnApiService,
        private msgSvc: HostMessageService,
        public ngxSmartModalService: NgxSmartModalService,
        private dialog: MatDialog
    ) {
        this.configSvc.configUpdated.subscribe((cfg: BeatOnConfig) => {
            this.config = cfg.Config;
        });
    }

    get leftColor() {
        if (this.config && this.config.LeftColor) {
            return (
                'rgba(' +
                this.config.LeftColor.R +
                ', ' +
                this.config.LeftColor.G +
                ', ' +
                this.config.LeftColor.B +
                ', ' +
                this.config.LeftColor.A +
                ')'
            );
        }
        return 'rgba(0,0,0,0)';
    }
    set leftColor(val) {}
    leftColorSelected(color) {
        console.log(color);
        color = color.substr(4, color.length - 5);
        var colors = color.split(',');
        var msg = new ClientChangeColor();
        msg.Color = <BeatSaberColor>{
            R: parseInt(colors[0]),
            G: parseInt(colors[1]),
            B: parseInt(colors[2]),
            A: colors.length > 3 ? Math.ceil(parseFloat(colors[3]) * 255) : 255,
        };
        msg.ColorType = ColorType.LeftColor;
        this.msgSvc.sendClientMessage(msg);
    }
    get rightColor() {
        if (this.config && this.config.RightColor) {
            return (
                'rgba(' +
                this.config.RightColor.R +
                ', ' +
                this.config.RightColor.G +
                ', ' +
                this.config.RightColor.B +
                ', ' +
                this.config.RightColor.A +
                ')'
            );
        }
        return 'rgba(0,0,0,0)';
    }
    set rightColor(val) {}
    rightColorSelected(color) {
        console.log(color);
        color = color.substr(4, color.length - 5);
        var colors = color.split(',');
        var msg = new ClientChangeColor();
        msg.Color = <BeatSaberColor>{
            R: parseInt(colors[0]),
            G: parseInt(colors[1]),
            B: parseInt(colors[2]),
            A: colors.length > 3 ? Math.ceil(parseFloat(colors[3]) * 255) : 255,
        };
        msg.ColorType = ColorType.RightColor;
        this.msgSvc.sendClientMessage(msg);
    }
    ngOnInit() {
        let isInit = false;
        this.configSvc.getConfig().subscribe((cfg: BeatOnConfig) => {
            this.config = cfg.Config;
            /*
            if (!isInit) {
                isInit = true;
                this.selectedMod = cfg.Config.Mods[0];
                const obj: Object = {
                    Name: this.selectedMod.Name,
                    Author: this.selectedMod.Author,
                    Description: this.selectedMod.Description,
                    InfoUrl: this.selectedMod.InfoUrl,
                };
                this.ngxSmartModalService.setModalData(obj, 'myModal');
            }
            var saberMod = new ModDefinition();
            saberMod.TargetBeatSaberVersion = '1.0.0';
            saberMod.ID = '1';
            saberMod.Author = 'Yuuki';
            saberMod.Name = 'Custom Sabers';
            saberMod.InfoUrl = 'http://www.google.com';
            saberMod.Description =
                'Change the color of your sabers! Choose between a wide spectrum of colors and jam with your favorite mix!';
            saberMod.Category = ModCategory.Saber;
            this.config.Mods.push(saberMod);
            var randomSongSelect = new ModDefinition();
            randomSongSelect.TargetBeatSaberVersion = '1.0.0';
            randomSongSelect.ID = '2';
            randomSongSelect.Author = 'Yuuki';
            randomSongSelect.Name = 'Random Song Selection';
            randomSongSelect.InfoUrl = 'http://www.google.com';
            randomSongSelect.Description =
                "Tired of deciding what song to play? This mod gives you the ability to randomly select a song from your long list of maps you'll probably never get to.";
            randomSongSelect.Category = ModCategory.Gameplay;
            this.config.Mods.push(randomSongSelect);*/
        });
    }

    getModBG(mod: ModDefinition) {
        if (!mod.CoverImageFilename) {
            if (mod.Category == ModCategory.Saber) {
                return 'url(../../assets/saber.png)';
            } else if (mod.Category == ModCategory.Note) {
                return 'url(../../assets/note.png)';
            } else if (mod.Category == ModCategory.Gameplay) {
                return 'url(../../assets/gameplay.png)';
            } else if (mod.Category == ModCategory.Library) {
                return 'url(../../assets/library.png)';
            } else {
                return 'url(../../assets/other.png)';
            }
        } else {
            let fixedUri = encodeURIComponent(mod.ID);
            fixedUri = fixedUri.replace('(', '%28').replace(')', '%29');
            return 'url(' + AppSettings.API_ENDPOINT + '/host/beatsaber/mod/cover?modid=' + fixedUri + ')';
        }
    }

    toggleMod(ev: MatSlideToggleChange, mod: ModDefinition) {
        this.modIDBeingSwitched = mod.ID;
        this.modSwitchInProgress = true;
        let msg = new ClientSetModStatus();
        msg.ModID = mod.ID;
        msg.Status = ev.checked ? ModStatusType.Installed : ModStatusType.NotInstalled;
        let sub;
        sub = this.msgSvc.actionResponseMessage.subscribe((ev: HostActionResponse) => {
            if (ev.ResponseToMessageID == msg.MessageID) {
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
        if (mod == null) return false;
        return !(
            (mod.Status != 'Installed' && mod.ID != this.modIDBeingSwitched) ||
            (mod.Status == 'Installed' && mod.ID == this.modIDBeingSwitched)
        );
    }

    onSelect(mod: ModDefinition): void {
        console.log(mod);
        this.selectedMod = mod;
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
    ngAfterViewInit() {}
}
