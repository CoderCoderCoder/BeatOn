import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { ToolbarEventsService } from '../services/toolbar-events.service';

@Component({
  selector: 'app-main-browser',
  templateUrl: './main-browser.component.html',
  styleUrls: ['./main-browser.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainBrowserComponent implements OnInit {
  @ViewChild('browser',null) browser: ElementRef;
  browserUrl : string = "https://www.bsaber.com";
  constructor(private toolbarEvents : ToolbarEventsService) {
    toolbarEvents.backClicked.subscribe(()=>{
      window.history.back(); 
    });
    toolbarEvents.refreshClicked.subscribe(()=>
    {
      this.browser.nativeElement.src = this.browserUrl;
    });
    toolbarEvents.navigate.subscribe((url)=>
    {
      this.browserUrl = url;
    });
   }

  ngOnInit() {
  }
  iframeLoaded(e) {
    console.log("iframe has loaded "+ e.src);
  }

}
