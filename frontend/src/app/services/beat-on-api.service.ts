import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import {AppSettings} from '../appSettings';

@Injectable({
  providedIn: 'root'
})
export class BeatOnApiService {

  hostname : string = AppSettings.API_ENDPOINT;
  constructor(private http: HttpClient) { }

  getModStatus() {
    return this.http.get(this.hostname+"/host/mod/status");
  }

  installModStep1() {
    return this.http.post(this.hostname+"/host/mod/install/step1", "");
  }

  installModStep2() {
    return this.http.post(this.hostname+"/host/mod/install/step2", "");
  }

  installModStep3() {
      return this.http.post(this.hostname+"/host/mod/install/step3", "");
  }

  resetAssets() {
    return this.http.post(this.hostname+"/host/mod/resetassets", "");
  }

  uninstallBeatSaber() {
    return this.http.post(this.hostname+"/host/mod/uninstallbeatsaber", "");
  }

  getConfig() {
    return this.http.get(this.hostname+"/host/beatsaber/config");
  }

}
