import { Component, OnInit, ɵMethodFn } from '@angular/core';
import { BeatOnApiService } from './services/beat-on-api.service';
import {Router, NavigationStart, RouterEvent} from '@angular/router';
import {trigger, animate, style, group, query, transition, state} from '@angular/animations';
import { ToastrService } from 'ngx-toastr';
import { HostMessageService } from './services/host-message.service';
import { HostShowToast, ToastType } from './models/HostShowToast';
import { HostSetupEvent, SetupEventType } from './models/HostSetupEvent';


@Component({
  selector: 'app-root',
  animations: [
    trigger('fade', [
      
      transition(':enter', [style({ opacity: 0 }), animate('0.2s', style({ opacity: 1}))]),
      transition(':leave', [style({ opacity: 1 }), animate('0.2s', style({ opacity: 0 }))]),
      state('*', style({ opacity: 1 }))
    ])
  ],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class AppComponent implements OnInit {
  constructor(private beatOnApi: BeatOnApiService, private router: Router, private msgSvc: HostMessageService, private toastr: ToastrService) { 
    this.router.events.subscribe((ev) => {
      if (ev instanceof NavigationStart) { 
        //TODO: prevent routing based on mod status?
      }
    });
    this.msgSvc.toastMessage.subscribe((ev) => this.showToast(ev));
  }
  modStatusLoaded: boolean = false;
  title : string = 'Beat On';
  showRefreshButton : boolean = false;
  showBackButton : boolean = false;
  resultJson = '';
  modStatus = { CurrentStatus: '' };
  ngOnInit() {

   this.checkModStatus();

   this.router.events.subscribe((routeEvent : RouterEvent) => {
     if (routeEvent instanceof NavigationStart)
     {
        this.showBackButton = (routeEvent.url == '/main/browser');
        this.showRefreshButton = (routeEvent.url == '/main/browser');
     }
   } );
   this.msgSvc.setupMessage.subscribe((msg : HostSetupEvent) =>
    {
      switch (msg.SetupEvent)
      {
        case SetupEventType.Step1Complete:
          this.router.navigateByUrl("/setupstep2");
          break;
        case SetupEventType.Step2Complete:
          this.router.navigateByUrl('/setupstep3');
          break;
        case SetupEventType.Step3Complete:
          this.router.navigateByUrl('/');
          break;
      }
    })
  }

  private showToast(toastMsg : HostShowToast) {
    console.log("got toast");
    switch (toastMsg.ToastType)
    {
      case ToastType.Error:
        this.toastr.error(toastMsg.Message, toastMsg.Title, { timeOut: toastMsg.Timeout });
        break;
      case ToastType.Info:
          this.toastr.info(toastMsg.Message, toastMsg.Title, { timeOut: toastMsg.Timeout });
        break;
      case ToastType.Success:
          this.toastr.success(toastMsg.Message, toastMsg.Title, { timeOut: toastMsg.Timeout });
        break;
      case ToastType.Warning:
          this.toastr.warning(toastMsg.Message, toastMsg.Title, { timeOut: toastMsg.Timeout });
        break;
    }
  }

  checkModStatus() : void
  {
    //TEST FOR NO BACK END
    // this.modStatus = { CurrentStatus: 'ModInstalled'};
    // this.modStatusLoaded = true;
    // this.router.navigateByUrl('/main/playlists');
    // return;
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
        this.router.navigateByUrl('/main/playlists');
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
