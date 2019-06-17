import { Component, OnInit } from '@angular/core';
import { ProgressSpinnerDialogComponent } from "../progress-spinner-dialog/progress-spinner-dialog.component";
import { MatDialog, MatDialogRef } from '@angular/material';
import {BeatOnApiService} from '../services/beat-on-api.service';

@Component({
  selector: 'app-setup-step1',
  templateUrl: './setup-step1.component.html',
  styleUrls: ['./setup-step1.component.scss']
})
export class SetupStep1Component implements OnInit {

  constructor(private beatOnApi: BeatOnApiService, private dialog : MatDialog) { 
   
  }

  ngOnInit() {
  }


  clickBegin() : void {
    const dialogRef = this.dialog.open(ProgressSpinnerDialogComponent, {
      width: '450px',
      height: '350px',
      disableClose: true,
      data: {mainText:"Please wait..."}
    });
    this.beatOnApi.installModStep1()
      .subscribe((data: any) => { 
        dialogRef.close();
    });

  }

}
