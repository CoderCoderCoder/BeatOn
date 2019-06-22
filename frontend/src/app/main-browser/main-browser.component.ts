import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-main-browser',
  templateUrl: './main-browser.component.html',
  styleUrls: ['./main-browser.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainBrowserComponent implements OnInit {

  browserUrl : string = "https://www.bsaber.com";
  constructor() { }

  ngOnInit() {
  }
  iframeLoaded(e) {
    console.log("iframe has loaded "+ e.src);
  }

}
