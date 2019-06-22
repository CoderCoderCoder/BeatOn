import { Component, OnInit } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { BeatSaberPlaylist } from '../models/BeatSaberPlaylist';
import { ConfigService } from '../services/config.service';

@Component({
  selector: 'app-main-playlists',
  templateUrl: './main-playlists.component.html',
  styleUrls: ['./main-playlists.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainPlaylistsComponent implements OnInit {
   config : QuestomConfig;
  selectedPlaylist: BeatSaberPlaylist;

  constructor(private beatOnApi : BeatOnApiService, private configSvc : ConfigService) { }
    
  selectedPlaylistChanged(ev) {
    this.selectedPlaylist = ev;
  }


  ngOnInit() {
   this.configSvc.getConfig().subscribe((cfg : QuestomConfig) => { this.config = cfg; });
   this.configSvc.configUpdated.subscribe((cfg : QuestomConfig)=> { this.config = cfg; });
  }

}
