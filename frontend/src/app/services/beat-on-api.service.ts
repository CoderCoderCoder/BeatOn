import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class BeatOnApiService {

  constructor(private http: HttpClient) { }

  getModStatus() {
    return this.http.get("/host/mod/status");
  }

  installModStep1() {
    return this.http.post("/host/mod/install/step1", "");
  }

  installModStep2() {
    return this.http.post("/host/mod/install/step2", "");
  }

  installModStep3() {
      return this.http.post("/host/mod/install/step3", "");
  }

  getConfig() {
    return this.http.get("/host/beatsaber/config");
  }

}
