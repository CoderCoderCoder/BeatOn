import { Component, OnInit, ElementRef, ViewChild } from '@angular/core';
import { ProgressSpinnerDialogComponent } from "../progress-spinner-dialog/progress-spinner-dialog.component";
import { MatDialog, MatDialogRef } from '@angular/material';
import {BeatOnApiService} from '../services/beat-on-api.service';
import {Router, NavigationStart} from '@angular/router';
import { routerTransition } from './router.animations';


@Component({
  selector: 'app-main',
  animations: [routerTransition],
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainComponent implements OnInit {
  constructor(private beatOnApi: BeatOnApiService, private dialog : MatDialog, private router : Router) { }
  activeLinkIndex = -1;
  navLinks = [
    {
        label: 'Playlists',
        path: './playlists',
        index: 0
    }, {
        label: 'Browser',
        path: './browser',
        index: 1
    }, {
      label: 'Tools',
      path: './tools',
      index: 2
  }
  ];

  testHtml : string = "";

  config;

  ngOnInit() {
   // this.refreshConfig();

  }

  refreshConfig() : void {
    
    this.beatOnApi.getConfig()
      .subscribe(
        (data: any) => { 
          this.testHtml = JSON.stringify(data);
          this.config = data;
        },
        (err: any) => {
          this.testHtml = JSON.stringify(err);
        },
    );

  }
  getState(outlet) {
    return outlet.activatedRoute.snapshot.routeConfig.path
  }
}
