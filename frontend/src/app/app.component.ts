import { Component, OnInit, ɵMethodFn } from '@angular/core';
import { BeatOnApiService } from './services/beat-on-api.service';
import {Router, NavigationStart, RouterEvent} from '@angular/router';
import {trigger, animate, style, group, query, transition, state} from '@angular/animations';
import { ToastrService } from 'ngx-toastr';
import { HostMessageService, ConnectionStatus } from './services/host-message.service';
import { HostShowToast, ToastType } from './models/HostShowToast';
import { HostSetupEvent, SetupEventType } from './models/HostSetupEvent';
import { ConfigService } from './services/config.service';
import { BeatOnConfig } from './models/BeatOnConfig';
import { ProgressSpinnerDialogComponent } from "./progress-spinner-dialog/progress-spinner-dialog.component";
import { MatDialog, MatDialogRef } from '@angular/material';
import { HostOpStatus, OpStatus } from './models/HostOpStatus';
import { ToolbarEventsService } from './services/toolbar-events.service';
import { AppIntegrationService } from './services/app-integration.service';

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
  constructor(private beatOnApi: BeatOnApiService, 
          private router: Router, 
          private msgSvc: HostMessageService,
          private toastr: ToastrService,
          private cfgSvc : ConfigService,
          private dialog : MatDialog,
          private toolbarEvents : ToolbarEventsService,
          private appIntegration : AppIntegrationService       
          ) { 
            this.msgSvc.opStatusMessage.subscribe((ev : HostOpStatus) => {
                this.opInProgress = (ev.Ops.findIndex(x => x.Status != OpStatus.Failed) > -1);

                
            });
    this.router.events.subscribe((ev) => {
      if (ev instanceof NavigationStart) { 
        //TODO: prevent routing based on mod status?
      }
      
    });
    this.msgSvc.toastMessage.subscribe((ev) => this.showToast(ev));
    this.cfgSvc.configUpdated.subscribe((cfg : BeatOnConfig) =>
      {
        this.config = cfg;
      });
      this.msgSvc.connectionStatusChanged.subscribe(stat => {
        this.connectionStatus = stat;
      });

      this.router.events.subscribe((routeEvent : RouterEvent) => {
        if (routeEvent instanceof NavigationStart)
        {
           this.showBackButton = (routeEvent.url == '/main/browser');
           this.showRefreshButton = (routeEvent.url == '/main/browser');
           this.showBrowser = (routeEvent.url == '/main/browser');
           if (routeEvent.url == '/') {
             this.modStatusLoaded = false;
             this.checkModStatus();
           }
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
  
  opInProgress: boolean;
  modStatusLoaded: boolean = false;
  title : string = 'Beat On';
  showRefreshButton : boolean = false;
  showBackButton : boolean = false;
  showBrowser : boolean = false;
  resultJson = '';
  modStatus = { CurrentStatus: '' };
  config : BeatOnConfig = { IsCommitted: true,
                            Config: null};
  ngOnInit() {
   
   this.checkModStatus();
  }

 commitConfig(){
  const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
    width: '450px',
    height: '350px',
    disableClose: true,
    data: {mainText:"Updating config...  Do not exit Beat On yet!"}
  });
  this.beatOnApi.commitConfig()
    .subscribe((data: any) => { 
      dialogRef.close();
  }, (err) =>
  {
    dialogRef.close();
  });

}
  private showToast(toastMsg : HostShowToast) {
    if (this.appIntegration.isBrowserShown) {
      console.log("redirecting toast to host since browser is visible");
      this.appIntegration.showToast(toastMsg.Title, toastMsg.Message, toastMsg.ToastType, toastMsg.Timeout);
      return;
    } else {
      console.log("browser is not shown, doing toast on web");
    }
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

  connectionStatus : ConnectionStatus = ConnectionStatus.Disconnected;

  getConnStatusColor() {
      if (this.connectionStatus == ConnectionStatus.Connected)
        return "green";
      else if (this.connectionStatus == ConnectionStatus.Connecting)
        return "orange";
      else
        return "gray";
  }
  
  getConnStatusIcon() {
    return (this.connectionStatus == ConnectionStatus.Connected || this.connectionStatus == ConnectionStatus.Connecting);
      
  }

  reconnect() {
    this.msgSvc.reconnect();
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
        this.cfgSvc.getConfig().subscribe((cfg) =>
        {
          this.config = cfg;
          if (this.router.url == '/' || this.router.url.indexOf('setup') > -1 || this.router.url == '/main')
          {
            if (this.appIntegration.isAppLoaded())
              this.router.navigateByUrl('/main/browser');
          }
          
        });
        
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

  clickBack() {
    this.toolbarEvents.triggerBackClicked();
  }

  clickRefresh() {
    this.toolbarEvents.triggerRefreshClicked();
  }

  linkSelected(ev) {
    this.toolbarEvents.triggerNavigate(ev);
  }
  
}
