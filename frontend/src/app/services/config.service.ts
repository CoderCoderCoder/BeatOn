import { Injectable, EventEmitter, Output, OnInit } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { Observable } from 'rxjs';
import { HostMessageService } from './host-message.service';
import { HostConfigChangeEvent } from '../models/HostConfigChangeEvent';


@Injectable({
  providedIn: 'root'
})
export class ConfigService implements OnInit {
  @Output() configUpdated = new EventEmitter<BeatOnConfig>();

  constructor(private beatOnApi : BeatOnApiService, private msgSvc : HostMessageService) { 
    console.log("config service subscribing to host config change event");
    this.msgSvc.configChangeMessage.subscribe((cfg : HostConfigChangeEvent) =>
    {
      console.log("config service got updated config message");
      this.currentConfig = cfg.UpdatedConfig;
      this.configUpdated.emit(cfg.UpdatedConfig);
    });
  }

  public getConfig() : Observable<BeatOnConfig>
  {
    return new Observable<BeatOnConfig>((observable) =>
    {
      if (this.currentConfig != null) {
        observable.next(this.currentConfig);
      }
      else {
        this.beatOnApi.getConfig()
            .subscribe((data: any) => observable.next(data));
      }
    });
  }

  ngOnInit() {
   
  }

  currentConfig : BeatOnConfig;
  
  refreshConfig()  {
    
    this.beatOnApi.getConfig()
      .subscribe(
        (data: any) => { 
          this.currentConfig = data;
          this.configUpdated.emit(data);
        },
        (err: any) => {
          console.log("ERROR" + err);
        },
    );

  }
}
