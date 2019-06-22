import {CollectionViewer, DataSource} from '@angular/cdk/collections';
import {Component, OnInit, Inject, ChangeDetectionStrategy, ChangeDetectorRef} from '@angular/core';
import {MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {BehaviorSubject, Observable, Subscription} from 'rxjs';
import { HostMessageService } from '../services/host-message.service';
import { HostSetupEvent, SetupEventType } from '../models/HostSetupEvent';

export interface SpinnerData {
  mainText : string;
}

@Component({
  selector: 'app-progress-spinner-dialog',
  templateUrl: './progress-spinner-dialog.component.html',
  styleUrls: ['./progress-spinner-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush

})
export class ProgressSpinnerDialogComponent implements OnInit {

  messages = new Array();

  mainText : string = "";
  constructor(public dialogRef: MatDialogRef<ProgressSpinnerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SpinnerData, private msgSvc: HostMessageService, private cdr : ChangeDetectorRef) { 
    this.mainText = data.mainText;
  }

  addMessage(event : any) {
    this.messages.push(event.data.Message);
  }
  ngOnInit() {
    this.msgSvc.setupMessage.subscribe((msg : HostSetupEvent) =>
    {
      console.log("Got message in spinner " + msg.SetupEvent);
      if (msg.SetupEvent == SetupEventType.StatusMessage) {
        console.log("pussing message to view");
        this.messages.push(msg.Message);
        //this.cdr.detectChanges();
      }
    });
  }

}