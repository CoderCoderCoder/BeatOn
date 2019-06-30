import { Component, OnInit, ViewChild, ElementRef, OnDestroy } from '@angular/core';
import { ToolbarEventsService } from '../services/toolbar-events.service';
import { AppIntegrationService } from '../services/app-integration.service';

@Component({
  selector: 'app-main-browser',
  templateUrl: './main-browser.component.html',
  styleUrls: ['./main-browser.component.scss'],
  host: {
    class:'fullheight'
  }
})
export class MainBrowserComponent implements OnInit, OnDestroy {
  @ViewChild('browser',null) browser: ElementRef;
  browserUrl : string = "https://www.bsaber.com";
  hideIframe: boolean = false;
  constructor(private toolbarEvents : ToolbarEventsService, private appIntegration : AppIntegrationService) {
    toolbarEvents.backClicked.subscribe(()=>{
      if (this.appIntegration.isAppLoaded())
      {
        appIntegration.browserGoBack();
      }
      else
      {
        window.history.back(); 
      }
    });
    toolbarEvents.refreshClicked.subscribe(()=>
    {
      if (this.appIntegration.isAppLoaded())
      {
        appIntegration.refreshBrowser();
      }
      else
      {
        this.browser.nativeElement.src = this.browserUrl;
      }
    });
    toolbarEvents.navigate.subscribe((url)=>
    {
      if (this.appIntegration.isAppLoaded())
      {
        appIntegration.navigateBrowser(url);
      }
      else
      {
        this.browserUrl = url;
      }      
    });
   }

  ngOnInit() {
      console.log("trying to show browser");
      if (this.appIntegration.isAppLoaded()) {
        this.hideIframe = true;
        this.appIntegration.showBrowser();
        this.appIntegration.navigateBrowser(this.browserUrl);
      }
  }
  ngOnDestroy() {
    console.log("trying to hide browser");
    if (this.appIntegration.isAppLoaded()) {
      this.appIntegration.hideBrowser();
    }
  }

  iframeLoaded(e) {
    console.log("iframe has loaded "+ e.src);
  }

}
