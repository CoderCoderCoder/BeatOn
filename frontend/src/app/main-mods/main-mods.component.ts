import { Component, OnInit } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { HostMessageService } from '../services/host-message.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatOnConfig } from '../models/BeatOnConfig';

@Component({
  selector: 'app-main-mods',
  templateUrl: './main-mods.component.html',
  styleUrls: ['./main-mods.component.scss']
})
export class MainModsComponent implements OnInit {
  config : QuestomConfig = <QuestomConfig> {Mods: []};
  constructor(private configSvc : ConfigService, private beatOnApi : BeatOnApiService, private msgSvc : HostMessageService) { 
    this.configSvc.configUpdated.subscribe((cfg : BeatOnConfig)=> {console.log("god mods?");  this.config = cfg.Config; });
  }

  ngOnInit() {
    this.configSvc.getConfig().subscribe((cfg : BeatOnConfig) => { console.log("god mods?"); this.config = cfg.Config; });
  }

}
