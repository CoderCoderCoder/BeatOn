import { Injectable, Output, EventEmitter, ApplicationRef } from '@angular/core';
import { HostSetupEvent } from '../models/HostSetupEvent';
import { HostShowToast } from '../models/HostShowToast';
import { AppSettings } from '../appSettings';
import * as Rx from "rxjs";
import { HostDownloadStatus } from '../models/HostDownloadStatus';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { HostConfigChangeEvent } from '../models/HostConfigChangeEvent';
import { NetInfo } from '../models/NetInfo';
import { BeatOnApiService } from './beat-on-api.service';
import { ToastrService } from 'ngx-toastr';
@Injectable({
  providedIn: 'root'
})
export class HostMessageService {
  private websocket : WebSocket;
  private wsObsv : Rx.Observable<MessageEvent>;
  private netConfig: NetInfo;

  constructor(private appRef : ApplicationRef, private beatOnApi: BeatOnApiService) { 
    beatOnApi.getNetInfo().subscribe((netInfo : NetInfo) => {
      this.netConfig = netInfo;
      console.log("HostMessageService got net config info, websocket is at " + this.netConfig.WebSocketUrl);
      this.openSocket();
    }, (err) => {
      console.log("Critical error: could not get net info for web socket!");
    });
    
  }

  private openSocket() {
    if (this.websocket != null && this.websocket.readyState === WebSocket.OPEN) {
      console.log("HostMessageService.openSocket called, but the connection is already open.");
      return;
    }

    this.websocket = new WebSocket(this.netConfig.WebSocketUrl);
    this.websocket.onopen = (ev:Event) => {
      console.log("Connection opened");
    };

    this.websocket.onmessage = (ev: MessageEvent) => {
      var reader = new FileReader();
      reader.onload = () => {
        let msgStr =<string>reader.result;
        console.log("got parsed message " + msgStr);
        let msgEvent = JSON.parse(msgStr);
        console.log("Message Event:");
        console.log(ev);
        if (msgEvent.Type == 'SetupEvent') {
          this.setupMessage.emit(<HostSetupEvent>msgEvent);
        } else if (msgEvent.Type == 'Toast') {
          this.toastMessage.emit(<HostShowToast>msgEvent);
        } else if (msgEvent.Type == 'DownloadStatus') {
          this.downloadStatusMessage.emit(<HostDownloadStatus>msgEvent);
        } else if (msgEvent.Type == 'ConfigChange') {
          console.log("emitting config change message");
          this.configChangeMessage.emit(<HostConfigChangeEvent>msgEvent);
        } else {
          console.log(`Unknown host message: ${msgStr}`)
        }
        this.appRef.tick();
      };
      reader.readAsText(ev.data);
    };

    this.websocket.onclose = (ev : Event) => {
      console.log("Connection was closed, reconnecting in several seconds...");
      setTimeout(() => {
        console.log("Trying to reopen socket...");
        this.openSocket();
      }, 2000);
    };

    this.websocket.onerror = (ev: Event) => {
      console.log("Web socket error!  Closing and reconnecting in several seconds.");
      setTimeout(() => {
        console.log("Trying to reopen socket...");
        this.openSocket();
      }, 2000);
    };
  }

  @Output() setupMessage = new EventEmitter<HostSetupEvent>();
  @Output() toastMessage = new EventEmitter<HostShowToast>();
  @Output() downloadStatusMessage = new EventEmitter<HostDownloadStatus>();
  @Output() configChangeMessage = new EventEmitter<HostConfigChangeEvent>();

}
