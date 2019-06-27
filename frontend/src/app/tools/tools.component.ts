import { Component, OnInit } from '@angular/core';
import { ProgressSpinnerDialogComponent } from "../progress-spinner-dialog/progress-spinner-dialog.component";
import { MatDialog, MatDialogRef } from '@angular/material';
import {BeatOnApiService} from '../services/beat-on-api.service';
import { HostShowToast, ToastType } from '../models/HostShowToast';
import { NetInfo } from '../models/NetInfo';
import { Router } from '@angular/router';

@Component({
  selector: 'app-tools',
  templateUrl: './tools.component.html',
  styleUrls: ['./tools.component.scss']
})
export class ToolsComponent implements OnInit {

  constructor(private beatOnApi: BeatOnApiService, private dialog : MatDialog, private router: Router) { 
   
  }
  netInfo : NetInfo;
  clickUninstallBeatSaber() {
    const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
      width: '450px',
      height: '350px',
      disableClose: true,
      data: {mainText:"Please wait..."}
    });
    this.beatOnApi.uninstallBeatSaber()
      .subscribe((data: any) => { 
        dialogRef.close();
        this.router.navigateByUrl('/');
    }, (err) => {
      dialogRef.close();
    });
  }
  
  clickResetAssets() {
    const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
      width: '450px',
      height: '350px',
      disableClose: true,
      data: {mainText:"Please wait..."}
    });
    this.beatOnApi.resetAssets()
      .subscribe((data: any) => { 
        dialogRef.close();
        window.dispatchEvent(new MessageEvent('host-message', {
          data:  <HostShowToast> {
            ToastType : ToastType.Info,
            Timeout : 3000,        
            Title : "Assets reset.",        
            Message : ""} }));
    }, (err) => {
      dialogRef.close();
      window.dispatchEvent(new MessageEvent('host-message', {
          data:  <HostShowToast> {
            ToastType : ToastType.Error,
            Timeout : 8000,        
            Title : "Error resetting assets!",        
            Message : err} }));
    });
  }

  clickReloadSongsFolder() {
    const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
      width: '450px',
      height: '350px',
      disableClose: true,
      data: {mainText:"Loading Songs Folder.  Please wait..."}
    });
    this.beatOnApi.reloadSongsFromFolders()
      .subscribe((data: any) => { 
        dialogRef.close();
    }, (err) => {
      dialogRef.close();
      window.dispatchEvent(new MessageEvent('host-message', {
          data:  <HostShowToast> {
            ToastType : ToastType.Error,
            Timeout : 8000,        
            Title : "Error reloading songs folder!",        
            Message : err} }));
    });
  }

  ngOnInit() {
    this.beatOnApi.getNetInfo().subscribe((ni : NetInfo) =>
    {
      this.netInfo = ni;
    });
  }

}
