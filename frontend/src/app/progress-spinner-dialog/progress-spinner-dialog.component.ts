import {CollectionViewer, DataSource} from '@angular/cdk/collections';
import {Component, Inject, ChangeDetectionStrategy} from '@angular/core';
import {MatDialog, MatDialogRef, MAT_DIALOG_DATA} from '@angular/material/dialog';
import {BehaviorSubject, Observable, Subscription} from 'rxjs';

export interface SpinnerData {
  mainText : string;
}

@Component({
  selector: 'app-progress-spinner-dialog',
  templateUrl: './progress-spinner-dialog.component.html',
  styleUrls: ['./progress-spinner-dialog.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush

})
export class ProgressSpinnerDialogComponent  {

  messages = new Array();

  mainText : string = "";
  constructor(public dialogRef: MatDialogRef<ProgressSpinnerDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: SpinnerData) { 
    this.mainText = data.mainText;
  }

  addMessage(event : any) {
    this.messages.push(event.data);
  }


}