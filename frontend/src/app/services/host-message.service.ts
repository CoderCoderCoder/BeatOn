import { Injectable, Output, EventEmitter, ApplicationRef } from '@angular/core';
import { HostSetupEvent } from '../models/HostSetupEvent';
import { HostShowToast } from '../models/HostShowToast';
import { AppSettings } from '../appSettings';
import * as Rx from 'rxjs';
import { HostDownloadStatus } from '../models/HostDownloadStatus';
import { BeatOnConfig } from '../models/BeatOnConfig';
import { HostConfigChangeEvent } from '../models/HostConfigChangeEvent';
import { NetInfo } from '../models/NetInfo';
import { BeatOnApiService } from './beat-on-api.service';
import { ToastrService } from 'ngx-toastr';
import { Observable } from 'rxjs';
import { HostOpStatus } from '../models/HostOpStatus';
import { MessageBase } from '../models/MessageBase';
import { debugOutputAstAsTypeScript } from '@angular/compiler';
import { HostActionResponse } from '../models/HostActionResponse';

@Injectable({
    providedIn: 'root',
})
export class HostMessageService {
    private websocket: WebSocket;
    private wsObsv: Rx.Observable<MessageEvent>;
    private netConfig: NetInfo;
    private connectionStatus: ConnectionStatus = ConnectionStatus.Disconnected;

    constructor(private appRef: ApplicationRef, private beatOnApi: BeatOnApiService) {
        if (AppSettings.WS_ENDPOINT_OVERRIDE) {
            this.netConfig = { Url: AppSettings.API_ENDPOINT, WebSocketUrl: AppSettings.WS_ENDPOINT_OVERRIDE };
            this.openSocket();
        } else {
            beatOnApi.getNetInfo().subscribe(
                (netInfo: NetInfo) => {
                    this.netConfig = netInfo;
                    console.log('HostMessageService got net config info, websocket is at ' + this.netConfig.WebSocketUrl);
                    this.openSocket();
                },
                err => {
                    console.log('Critical error: could not get net info for web socket!');
                }
            );
        }
    }
    private openSocket() {
        if (this.websocket != null && this.websocket.readyState === WebSocket.OPEN) {
            console.log('HostMessageService.openSocket called, but the connection is already open.');
            return;
        }
        if (this.websocket != null) this.websocket.close();

        this.connectionStatus = ConnectionStatus.Connecting;
        this.connectionStatusChanged.emit(this.connectionStatus);

        this.websocket = new WebSocket(this.netConfig.WebSocketUrl);
        this.websocket.onopen = (ev: Event) => {
            console.log('Connection opened');
            this.connectionStatus = ConnectionStatus.Connected;
            this.connectionStatusChanged.emit(this.connectionStatus);
        };

        this.websocket.onmessage = (ev: MessageEvent) => {
            //assuming if we get a message it's connected
            if (this.connectionStatus != ConnectionStatus.Connected) {
                this.connectionStatus = ConnectionStatus.Connected;
                this.connectionStatusChanged.emit(this.connectionStatus);
            }
            var reader = new FileReader();
            reader.onload = () => {
                let msgStr = <string>reader.result;
                //console.log("got message: " + msgStr);
                let msgEvent = JSON.parse(msgStr);
                if (msgEvent.Type == 'SetupEvent') {
                    this.setupMessage.emit(<HostSetupEvent>msgEvent);
                } else if (msgEvent.Type == 'Toast') {
                    this.toastMessage.emit(<HostShowToast>msgEvent);
                } else if (msgEvent.Type == 'DownloadStatus') {
                    this.downloadStatusMessage.emit(<HostDownloadStatus>msgEvent);
                } else if (msgEvent.Type == 'ConfigChange') {
                    this.configChangeMessage.emit(<HostConfigChangeEvent>msgEvent);
                } else if (msgEvent.Type == 'OpStatus') {
                    this.opStatusMessage.emit(<HostOpStatus>msgEvent);
                } else if (msgEvent.Type == 'ActionResponse') {
                    this.actionResponseMessage.emit(<HostActionResponse>msgEvent);
                } else {
                    console.log(`Unknown host message: ${msgStr}`);
                }
                this.appRef.tick();
            };
            reader.readAsText(ev.data);
        };

        this.websocket.onclose = (ev: Event) => {
            this.connectionStatus = ConnectionStatus.Disconnected;
            this.connectionStatusChanged.emit(this.connectionStatus);
            console.log('Connection was closed, reconnecting in several seconds...');
            // setTimeout(() => {
            //   console.log("Trying to reopen socket...");
            //   this.openSocket();
            // }, 2000);
        };

        this.websocket.onerror = (ev: Event) => {
            this.connectionStatus = ConnectionStatus.Error;
            this.connectionStatusChanged.emit(this.connectionStatus);
            console.log('WEBSOCKET ERROR OH NOOOOO!');
            //   console.log("Web socket error!  Closing and reconnecting in several seconds.");
            //   setTimeout(() => {
            //     console.log("Trying to reopen socket...");
            //     this.openSocket();
            //   }, 2000);
        };
    }

    public reconnect() {
        //don't try to reconnect if it's open
        if (this.websocket != null && this.websocket.readyState === WebSocket.OPEN) return;

        this.openSocket();
    }

    sendClientMessage(message: MessageBase) {
        if (this.connectionStatus != ConnectionStatus.Connected) {
            var sub;
            sub = this.connectionStatusChanged.subscribe(constat => {
                if (constat == ConnectionStatus.Connected) {
                    var msg = JSON.stringify(message);
                    this.websocket.send(msg);
                    sub.unsubscribe();
                }
            });
            if (this.connectionStatus == ConnectionStatus.Disconnected) {
                this.reconnect();
            }
        } else {
            var msg = JSON.stringify(message);
            this.websocket.send(msg);
        }
    }

    @Output() setupMessage = new EventEmitter<HostSetupEvent>();
    @Output() toastMessage = new EventEmitter<HostShowToast>();
    @Output() downloadStatusMessage = new EventEmitter<HostDownloadStatus>();
    @Output() configChangeMessage = new EventEmitter<HostConfigChangeEvent>();
    @Output() opStatusMessage = new EventEmitter<HostOpStatus>();
    @Output() connectionStatusChanged = new EventEmitter<ConnectionStatus>();
    @Output() actionResponseMessage = new EventEmitter<HostActionResponse>();
}

export enum ConnectionStatus {
    Disconnected,
    Connecting,
    Connected,
    Error,
}
