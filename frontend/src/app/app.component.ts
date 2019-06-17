import { Component, OnInit } from '@angular/core';
import { BeatOnApiService } from './services/beat-on-api.service';
import {Router, NavigationStart} from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  constructor(private beatOnApi: BeatOnApiService, private router : Router) { 
    this.router.events.subscribe((ev) => {
      if (ev instanceof NavigationStart) { 
        //TODO: prevent routing based on mod status?
      }
    });
  }
  modStatusLoaded: boolean = false;
  title = 'Beat On';
  resultJson = '';
  modStatus = { CurrentStatus: '' };
  ngOnInit() {
    this.checkModStatus();
  }
  clientEvent(event : any) {
    this.checkModStatus();
  }
  checkModStatus() : void
  {
    this.beatOnApi.getModStatus()
    .subscribe((data: any) => {
       this.modStatusLoaded = true;
       this.modStatus = data; 
       if (this.modStatus.CurrentStatus == 'ModInstallNotStarted') {
        this.router.navigateByUrl('/setup');
       }
       else if (this.modStatus.CurrentStatus == 'ReadyForModApply') {
        this.router.navigateByUrl('/setupstep2');
       }
       else if (this.modStatus.CurrentStatus == 'ReadyForInstall') {
        this.router.navigateByUrl('/setupstep3');
       }
       else if (this.modStatus.CurrentStatus == 'ModInstalled') {
        this.router.navigateByUrl('/main');
       }
    });
  }

  public onClickModStatus() {
    
  }
  public onClickInstallModStep1() {
    this.beatOnApi.installModStep1()
      .subscribe((data: any) => { this.modStatusLoaded = true; this.modStatus = data; this.resultJson = JSON.stringify(data);});
  }
  public onClickInstallModStep2() {
    this.beatOnApi.installModStep2()
      .subscribe((data: any) => { this.modStatus = data; this.resultJson = JSON.stringify(data);});
  }
  
}
