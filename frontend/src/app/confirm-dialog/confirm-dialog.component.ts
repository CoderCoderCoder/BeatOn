import { Component, OnInit, Injectable, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { AddEditPlaylistDialogComponent } from '../add-edit-playlist-dialog/add-edit-playlist-dialog.component';

@Component({
  selector: 'app-confirm-dialog',
  templateUrl: './confirm-dialog.component.html',
  styleUrls: ['./confirm-dialog.component.scss']
})
@Injectable({
  providedIn: 'root'
})
export class ConfirmDialogComponent implements OnInit {

  constructor(public dialogRef: MatDialogRef<AddEditPlaylistDialogComponent>, @Inject(MAT_DIALOG_DATA) public data) { }

  ngOnInit() {
  }
  clickButton1() {
    this.dialogRef.close(1);
  }
  clickCancel() {
    this.dialogRef.close(-1);
  }
}
