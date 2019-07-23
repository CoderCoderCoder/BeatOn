import { CollectionViewer, DataSource } from '@angular/cdk/collections';
import { Component, OnInit, Inject, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { MatDialog, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { BehaviorSubject, Observable, Subscription } from 'rxjs';
import { HostMessageService } from '../services/host-message.service';
import { HostSetupEvent, SetupEventType } from '../models/HostSetupEvent';
import { Router } from '@angular/router';
import { AppIntegrationService } from '../services/app-integration.service';

export interface SpinnerData {
    mainText: string;
}

@Component({
    selector: 'app-progress-spinner-dialog',
    templateUrl: './progress-spinner-dialog.component.html',
    styleUrls: ['./progress-spinner-dialog.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProgressSpinnerDialogComponent implements OnInit {
    messages = new Array();

    mainText: string = '';
    constructor(
        public dialogRef: MatDialogRef<ProgressSpinnerDialogComponent>,
        @Inject(MAT_DIALOG_DATA) public data: SpinnerData,
        private msgSvc: HostMessageService,
        private cdr: ChangeDetectorRef,
        private appIntegration: AppIntegrationService
    ) {
        this.mainText = data.mainText;
        var switched = false;
        dialogRef.afterOpened().subscribe(() => {
            if (this.appIntegration.isBrowserShown) {
                if (this.appIntegration.isAppLoaded()) {
                    this.appIntegration.hideBrowser();
                    switched = true;
                }
            }
        });
        dialogRef.beforeClosed().subscribe(() => {
            if (switched && this.appIntegration.isAppLoaded()) {
                this.appIntegration.showBrowser();
            }
        });
        this.msgSvc.setupMessage.subscribe((msg: HostSetupEvent) => {
            if (msg.SetupEvent == SetupEventType.StatusMessage) {
                this.messages.push(msg.Message);
                this.cdr.detectChanges();
            }
        });
    }

    addMessage(event: any) {
        this.messages.push(event.data.Message);
    }
    ngOnInit() {}
}
