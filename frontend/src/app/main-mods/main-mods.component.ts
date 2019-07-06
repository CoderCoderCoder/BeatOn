import { Component, OnInit} from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatOnConfig } from '../models/BeatOnConfig';
import {ModCategory, ModDefinition, ModStatusType} from '../models/ModDefinition';
import { ClientSetModStatus } from '../models/ClientSetModStatus';
import { MatSlideToggleChange } from '@angular/material';
import { HostActionResponse } from '../models/HostActionResponse';
import { ECANCELED } from 'constants';

@Component({
  selector: 'app-main-mods',
  templateUrl: './main-mods.component.html',
  styleUrls: ['./main-mods.component.scss']
})
export class MainModsComponent implements OnInit {
  config : QuestomConfig = <QuestomConfig> {Mods: []};
  modSwitchInProgress : boolean = false;
  modIDBeingSwitched : string = null;
  selectedMod: ModDefinition;
  opened: boolean;
  constructor(private configSvc : ConfigService, private beatOnApi : BeatOnApiService, private msgSvc : HostMessageService) {
    this.configSvc.configUpdated.subscribe((cfg : BeatOnConfig)=> { this.config = cfg.Config; });
  }

  ngOnInit() {
    this.configSvc.getConfig().subscribe((cfg : BeatOnConfig) => {
      // this.config = cfg.Config;
      var saberMod = new ModDefinition();
      saberMod.TargetBeatSaberVersion = "1.0.0"
      saberMod.ID = "1"
      saberMod.Author = "Yuuki"
      saberMod.Name = "Custom Sabers"
      saberMod.InfoUrl = "http://www.google.com"
      saberMod.Description = "Change the color of your sabers! Choose between a wide spectrum of colors and jam with your favorite mix!"
      saberMod.Category = ModCategory.Saber
      this.config.Mods.push(saberMod)
      var randomSongSelect = new ModDefinition();
      randomSongSelect.TargetBeatSaberVersion = "1.0.0"
      randomSongSelect.ID = "2"
      randomSongSelect.Author = "Yuuki"
      randomSongSelect.Name = "Random Song Selection"
      randomSongSelect.InfoUrl = "http://www.google.com"
      randomSongSelect.Description = "Tired of deciding what song to play? This mod gives you the ability to randomly select a song from your long list of maps you'll probably never get to."
      randomSongSelect.Category = ModCategory.Gameplay
      this.config.Mods.push(randomSongSelect)
    });
  }

  toggleMod(ev : MatSlideToggleChange, mod : ModDefinition) {
    this.modIDBeingSwitched = mod.ID;
    this.modSwitchInProgress = true;
    var msg = new ClientSetModStatus();
    msg.ModID = mod.ID;
    msg.Status = ev.checked?ModStatusType.Installed:ModStatusType.NotInstalled;
    console.log("sending message for mod ID " + msg.ModID);
    var sub;
    sub = this.msgSvc.actionResponseMessage.subscribe((ev : HostActionResponse) => {
      if (ev.ResponseToMessageID == msg.MessageID) {
        console.log("Got response message in mods for mod ID " + msg.ModID);
        this.modIDBeingSwitched = null;
        this.modSwitchInProgress = false;
        sub.unsubscribe();
        if (!ev.Success) {
          //todo: show error
          console.log("mod id "+ msg.ModID + " did not switch properly");
        }

      }
    });
    this.msgSvc.sendClientMessage(msg);

  }
  getModSwitch(mod) {
    console.log("getting mod status for mod id " + mod.ID);
    if (mod == null)
      return false;
    return !((mod.Status != 'Installed' && mod.ID != this.modIDBeingSwitched) || (mod.Status == 'Installed' && mod.ID == this.modIDBeingSwitched));
  }

  onSelect(mod : ModDefinition): void{
    this.selectedMod = mod;
  }
}
