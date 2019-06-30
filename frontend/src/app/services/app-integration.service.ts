import { Injectable } from '@angular/core';
import { ToastType } from '../models/HostShowToast';

@Injectable({
  providedIn: 'root'
})
export class AppIntegrationService {
  constructor() { }
  
  private static INTERFACE_NAME : string = "BeatOnAppInterface";
  isBrowserShown : boolean = false;

  public isAppLoaded() : boolean {
    var isIt = (window[AppIntegrationService.INTERFACE_NAME] != null);
    console.log("isAppLoaded called, result: " + isIt);
    return isIt;
  }

  public refreshBrowser() {
    console.log("calling interface refresh browser");
    window[AppIntegrationService.INTERFACE_NAME].refreshBrowser();
  }

  public showBrowser() {
    var element = document.getElementById('mainContentDiv')
    var rect = element.getBoundingClientRect();
    console.log("calling interface show browser with offset " + rect.top);
    window[AppIntegrationService.INTERFACE_NAME].showBrowser(rect.top);
    this.isBrowserShown = true;
  }

  public hideBrowser() {
    console.log("calling interface hide browser");
    window[AppIntegrationService.INTERFACE_NAME].hideBrowser();
    this.isBrowserShown = false;
  }

  public browserGoBack() {
    console.log("calling interface browser go back");
    window[AppIntegrationService.INTERFACE_NAME].browserGoBack();
  }

  public navigateBrowser(url: string) {
    console.log("calling navigate browser with " + url);
    window[AppIntegrationService.INTERFACE_NAME].navigateBrowser(url);
  }

  public showToast(title: string, message: string, type : ToastType, duration: number)
  {
    console.log("calling toast");
    window[AppIntegrationService.INTERFACE_NAME].showToast(title, message, ToastType[type], duration);
  }
}
