import { Component, OnInit } from '@angular/core';
import { ProgressSpinnerDialogComponent } from "../progress-spinner-dialog/progress-spinner-dialog.component";
import { MatDialog, MatDialogRef } from '@angular/material';
import {BeatOnApiService} from '../services/beat-on-api.service';
import {Router, NavigationStart} from '@angular/router';


@Component({
  selector: 'app-main',
  templateUrl: './main.component.html',
  styleUrls: ['./main.component.scss']
})
export class MainComponent implements OnInit {

  constructor(private beatOnApi: BeatOnApiService, private dialog : MatDialog, private router : Router) { }
  
  navLinks = [
    {
        label: 'Playlists',
        link: './playlists',
        index: 0
    }, {
        label: 'Browser',
        link: './browser',
        index: 1
    }
  ];

  testHtml : string = "";

  config;

  ngOnInit() {
   // this.refreshConfig();
    // this.router.events.subscribe((res) => {
    //   this.activeLinkIndex = this.navLinks.indexOf(this.navLinks.find(tab => tab.link === '.' + this.router.url));
    // });
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

}
