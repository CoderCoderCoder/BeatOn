import { Component, OnInit } from '@angular/core';
import { ConfigService } from '../services/config.service';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { BeatOnConfig } from '../models/BeatOnConfig';

@Component({
    selector: 'app-sync-saber',
    templateUrl: './sync-saber.component.html',
    styleUrls: ['./sync-saber.component.scss'],
})
export class SyncSaberComponent implements OnInit {
    constructor(private beatOnApi: BeatOnApiService, private configSvc: ConfigService) {}

    ngOnInit() {
        this.configSvc.getConfig().subscribe(this.handleConfig.bind(this));
        this.configSvc.configUpdated.subscribe(this.handleConfig.bind(this));
    }

    handleConfig(cfg: BeatOnConfig) {
        console.log(cfg);
    }
}
