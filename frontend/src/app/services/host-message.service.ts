import { Injectable, Output, EventEmitter } from '@angular/core';
import { HostSetupEvent } from '../models/HostSetupEvent';
import { HostShowToast } from '../models/HostShowToast';


@Injectable({
  providedIn: 'root'
})
export class HostMessageService {

  constructor() { 
    window.addEventListener('host-message', (event : MessageEvent) => {
      this.onHostMessage(event.data);
    });
  }

  @Output() setupMessage = new EventEmitter<HostSetupEvent>();
  @Output() toastMessage = new EventEmitter<HostShowToast>();


  private onHostMessage(data) {
    console.log("got host message");
    if (data.Type == 'SetupEvent') {
      this.setupMessage.emit(<HostSetupEvent>data);
    } else if (data.Type == 'Toast') {
        this.toastMessage.emit(<HostShowToast>data);
    } else {
      console.log(`Unknown host message: ${JSON.stringify(data)}`)
    }
  }

}
