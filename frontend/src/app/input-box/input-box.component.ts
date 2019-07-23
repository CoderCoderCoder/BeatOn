import { Component, OnInit, Injectable, Inject } from '@angular/core';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material';
import { CookieService } from 'ngx-cookie-service';
import { AppIntegrationService } from '../services/app-integration.service';

@Component({
    selector: 'app-input-box',
    templateUrl: './input-box.component.html',
    styleUrls: ['./input-box.component.scss'],
})
@Injectable({
    providedIn: 'root',
})
export class InputBoxComponent implements OnInit {
    constructor(
        public dialogRef: MatDialogRef<InputBoxComponent>,
        @Inject(MAT_DIALOG_DATA) public data,
        private cookies: CookieService,
        private appIntegration: AppIntegrationService
    ) {
        if (data.key) {
            var cookieHist = cookies.get('inputhistory_' + data.key);
            if (cookieHist) {
                this.historyItems = JSON.parse(cookieHist);
            } else {
                this.historyItems = [];
            }
        }
    }
    historyItems: string[] = [];
    inputText: string;
    wasBrowserHidden: boolean = false;
    ngOnInit() {
        this.wasBrowserHidden = false;
        if (this.appIntegration.isBrowserShown) {
            this.wasBrowserHidden = true;
            this.appIntegration.hideBrowser();
        }
    }

    historyClick(item) {
        this.inputText = item;
        this.clickButton1();
    }
    clickButton1() {
        if (!this.inputText) {
            return;
        }
        if (this.data.key && this.inputText) {
            if (!this.historyItems) {
                this.historyItems = [];
            }
            var idx = this.historyItems.indexOf(this.inputText);
            if (idx >= 0) {
                this.historyItems.splice(idx, 1);
            }
            this.historyItems.unshift(this.inputText);
            while (this.historyItems.length > 5) {
                this.historyItems.pop();
            }
            this.cookies.set('inputhistory_' + this.data.key, JSON.stringify(this.historyItems));
        }
        if (this.wasBrowserHidden) {
            this.wasBrowserHidden = false;
            this.appIntegration.showBrowser();
        }
        this.dialogRef.close(this.inputText);
    }
    clickCancel() {
        if (this.wasBrowserHidden) {
            this.wasBrowserHidden = false;
            this.appIntegration.showBrowser();
        }
        this.dialogRef.close(null);
    }
    keyDownFunction(ev) {
        if (ev.keyCode == 13) {
            this.clickButton1();
        }
    }
}
