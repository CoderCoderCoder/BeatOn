import { Component, OnInit } from '@angular/core';
import { HostMessageService, ConnectionStatus } from '../services/host-message.service';
import { HostOpStatus, OpStatus } from '../models/HostOpStatus';
import { ClientGetOps } from '../models/ClientGetOps';

@Component({
  selector: 'app-op-indicator',
  templateUrl: './op-indicator.component.html',
  styleUrls: ['./op-indicator.component.scss']
})
export class OpIndicatorComponent implements OnInit {
  
  constructor(private msgSvc: HostMessageService) {
    var gotOps = false;
    this.msgSvc.opStatusMessage.subscribe((ev : HostOpStatus) => {
      console.log("there are " + this.ops.Ops.length + " ops to display");
      console.log(JSON.stringify(this.ops.Ops));
        console.log("got ops update status message");
        this.ops = ev;
        gotOps = true;
    });
    msgSvc.connectionStatusChanged.subscribe(ev => {
      if (ev == ConnectionStatus.Connected && !gotOps) {
        var msg = new ClientGetOps();
        msg.ClearFailedOps = false;
        this.msgSvc.sendClientMessage(msg);
      }
    });
  }

  getColor() {
    if (this.showError())
    {
      return 'warn';
    }
    return '';
  }

  ops : HostOpStatus = { Ops: []};

  showCheck() {
    return this.ops.Ops.length < 1;
  }

  showSpin() {
    var idx = this.ops.Ops.findIndex(x => x.Status != OpStatus.Failed);
    return (idx > -1);
  }

  showError() {
    var idx = this.ops.Ops.findIndex(x => x.Status == OpStatus.Failed);
    return (idx > -1);
  }

  opClick(op)
  {
    if (op.Error)
      alert(op.Error);
  }

  ngOnInit() {

  }

  clickClearFailedOps() {
    var msg = new ClientGetOps();
    msg.ClearFailedOps = true;
    this.msgSvc.sendClientMessage(msg);
  }

}
