import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ProgressSpinnerDialogComponent } from '../progress-spinner-dialog/progress-spinner-dialog.component';
import { MatDialog, MatDialogRef } from '@angular/material';
import { BeatOnApiService } from '../services/beat-on-api.service';
import { Router, NavigationStart } from '@angular/router';
import { routerTransition } from './router.animations';
import { QuestomConfig } from '../models/QuestomConfig';
import { ConfigService } from '../services/config.service';

@Component({
    selector: 'app-main',
    animations: [routerTransition],
    templateUrl: './main.component.html',
    styleUrls: ['./main.component.scss'],
    host: {
        class: 'fullheight',
    },
})
export class MainComponent implements OnInit {
    constructor(
        private beatOnApi: BeatOnApiService,
        private dialog: MatDialog,
        private router: Router,
        private configSvc: ConfigService
    ) {}
    activeLinkIndex = -1;
    navLinks = [];

    testHtml: string = '';
    tabMouseDown(link) {
        this.activeLinkIndex = link.index;
    }
    clickStuff() {
        //how to suppress others?
    }
    ngOnInit() {
        this.configSvc.refreshConfig();
        let onQuest: boolean = (<any>window).isQuestHosted();
        if (!onQuest) {
            console.log('Not hosted on the quest in Beat On, showing upload rather than browser.');
        } else {
            console.log('Hosted in Beat On on the quest, showing browser rather than upload.');
        }
        this.navLinks.push({
            label: 'Playlists',
            path: './playlists',
            index: 0,
        });
        if (onQuest) {
            this.navLinks.push({
                label: 'Browser',
                path: './browser',
                index: 1,
            });
        } else {
            this.navLinks.push({
                label: 'Upload',
                path: './upload',
                index: 1,
            });
        }
        this.navLinks.push({
            label: 'Mods',
            path: './mods',
            index: 3,
        });
        this.navLinks.push({
            label: 'Tools',
            path: './tools',
            index: 2,
        });
        this.navLinks.push({
            label: 'Credits',
            path: './credits',
            index: 2,
        });
    }

    getState(outlet) {
        return outlet.activatedRoute.snapshot.routeConfig.path;
    }
}
