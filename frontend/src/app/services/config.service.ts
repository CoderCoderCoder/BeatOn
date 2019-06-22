import { Injectable, EventEmitter, Output } from '@angular/core';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { QuestomConfig } from '../models/QuestomConfig';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  @Output() configUpdated = new EventEmitter<QuestomConfig>();

  constructor(private beatOnApi : BeatOnApiService) { }

  public getConfig() : Observable<QuestomConfig>
  {
    return new Observable<QuestomConfig>((observable) =>
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

  currentConfig : QuestomConfig;
  
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
